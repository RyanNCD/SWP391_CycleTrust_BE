using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using CycleTrust.Application.DTOs;
using CycleTrust.Core.Entities;
using CycleTrust.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Enums;

namespace CycleTrust.Application.Services;

public interface IVNPayService
{
    Task<VNPayPaymentResponseDto> CreatePaymentUrl(VNPayPaymentRequestDto request, string ipAddress);
    Task<VNPayReturnDto> ProcessCallback(Dictionary<string, string> queryParams);
    bool ValidateSignature(Dictionary<string, string> queryParams, string secureHash);
}

public class VNPayService : IVNPayService
{
    private readonly IConfiguration _configuration;
    private readonly CycleTrustDbContext _context;
    private readonly string _tmnCode;
    private readonly string _hashSecret;
    private readonly string _baseUrl;
    private readonly string _returnUrl;

    public VNPayService(IConfiguration configuration, CycleTrustDbContext context)
    {
        _configuration = configuration;
        _context = context;
        _tmnCode = configuration["VNPay:TmnCode"] ?? throw new ArgumentNullException("VNPay TmnCode not configured");
        _hashSecret = configuration["VNPay:HashSecret"] ?? throw new ArgumentNullException("VNPay HashSecret not configured");
        _baseUrl = configuration["VNPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        _returnUrl = configuration["VNPay:ReturnUrl"] ?? "http://localhost:5173/payment/vnpay-return";
    }

    public async Task<VNPayPaymentResponseDto> CreatePaymentUrl(VNPayPaymentRequestDto request, string ipAddress)
    {
        try
        {
            // Validate order exists
            var order = await _context.Orders
                .Include(o => o.Listing)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
            {
                return new VNPayPaymentResponseDto
                {
                    Success = false,
                    Message = "Đơn hàng không tồn tại"
                };
            }

            // Create VNPay parameters
            var vnpay = new SortedDictionary<string, string>
            {
                { "vnp_Version", _configuration["VNPay:Version"] ?? "2.1.0" },
                { "vnp_Command", _configuration["VNPay:Command"] ?? "pay" },
                { "vnp_TmnCode", _tmnCode },
                { "vnp_Amount", (request.Amount * 100).ToString() }, // VNPay uses smallest currency unit
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", _configuration["VNPay:CurrCode"] ?? "VND" },
                { "vnp_IpAddr", ipAddress },
                { "vnp_Locale", _configuration["VNPay:Locale"] ?? "vn" },
                { "vnp_OrderInfo", request.OrderInfo },
                { "vnp_OrderType", "billpayment" },
                { "vnp_ReturnUrl", request.ReturnUrl ?? _returnUrl },
                { "vnp_TxnRef", $"{DateTime.Now.Ticks}_{order.Id}" } // Unique transaction reference
            };

            // Build query string and hash
            var queryString = BuildQueryString(vnpay);
            var signData = queryString;
            var vnpSecureHash = HmacSHA512(_hashSecret, signData);
            
            var paymentUrl = $"{_baseUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";

            // Create payment record
            var payment = new Payment
            {
                OrderId = order.Id,
                Type = request.PaymentType,
                Status = PaymentStatus.PENDING,
                Amount = request.Amount,
                Currency = "VND",
                Provider = "VNPay",
                ProviderTxnId = vnpay["vnp_TxnRef"],
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return new VNPayPaymentResponseDto
            {
                Success = true,
                PaymentUrl = paymentUrl
            };
        }
        catch (Exception ex)
        {
            return new VNPayPaymentResponseDto
            {
                Success = false,
                Message = $"Lỗi tạo URL thanh toán: {ex.Message}"
            };
        }
    }

    public async Task<VNPayReturnDto> ProcessCallback(Dictionary<string, string> queryParams)
    {
        try
        {
            var secureHash = queryParams.ContainsKey("vnp_SecureHash") ? queryParams["vnp_SecureHash"] : "";
            
            // Remove hash from params for validation
            var paramsToValidate = new Dictionary<string, string>(queryParams);
            paramsToValidate.Remove("vnp_SecureHash");
            paramsToValidate.Remove("vnp_SecureHashType");

            // Validate signature
            if (!ValidateSignature(paramsToValidate, secureHash))
            {
                return new VNPayReturnDto
                {
                    Success = false,
                    ResponseCode = VNPayResponseCode.INVALID_SIGNATURE,
                    Message = "Chữ ký không hợp lệ"
                };
            }

            var responseCode = queryParams.GetValueOrDefault("vnp_ResponseCode", "");
            var transactionStatus = queryParams.GetValueOrDefault("vnp_TransactionStatus", "");
            var txnRef = queryParams.GetValueOrDefault("vnp_TxnRef", "");
            var amount = long.Parse(queryParams.GetValueOrDefault("vnp_Amount", "0")) / 100; // Convert back from smallest unit
            var bankCode = queryParams.GetValueOrDefault("vnp_BankCode", null);
            var cardType = queryParams.GetValueOrDefault("vnp_CardType", null);
            var payDateStr = queryParams.GetValueOrDefault("vnp_PayDate", "");
            var orderInfo = queryParams.GetValueOrDefault("vnp_OrderInfo", "");

            DateTime payDate = DateTime.Now;
            if (!string.IsNullOrEmpty(payDateStr))
            {
                DateTime.TryParseExact(payDateStr, "yyyyMMddHHmmss", null, DateTimeStyles.None, out payDate);
            }

            // Extract order ID from TxnRef (format: timestamp_orderId)
            var orderId = ExtractOrderIdFromTxnRef(txnRef);

            // Update payment record
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.ProviderTxnId == txnRef);

            if (payment != null)
            {
                payment.Status = responseCode == VNPayResponseCode.SUCCESS 
                    ? PaymentStatus.PAID 
                    : PaymentStatus.FAILED;
                payment.PaidAt = responseCode == VNPayResponseCode.SUCCESS ? payDate : null;

                // Update order status
                var order = await _context.Orders.FindAsync(payment.OrderId);
                if (order != null && responseCode == VNPayResponseCode.SUCCESS)
                {
                    if (payment.Type == PaymentType.DEPOSIT)
                    {
                        order.Status = OrderStatus.DEPOSIT_PAID;
                        order.DepositPaidAt = payDate;
                    }
                    order.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return new VNPayReturnDto
            {
                Success = responseCode == VNPayResponseCode.SUCCESS,
                TransactionId = txnRef,
                OrderId = orderId,
                Amount = amount,
                OrderInfo = orderInfo,
                ResponseCode = responseCode,
                TransactionStatus = transactionStatus,
                BankCode = bankCode,
                CardType = cardType,
                PayDate = payDate,
                Message = VNPayResponseCode.GetMessage(responseCode)
            };
        }
        catch (Exception ex)
        {
            return new VNPayReturnDto
            {
                Success = false,
                ResponseCode = VNPayResponseCode.FAILURE,
                Message = $"Lỗi xử lý callback: {ex.Message}"
            };
        }
    }

    public bool ValidateSignature(Dictionary<string, string> queryParams, string secureHash)
    {
        var sortedParams = new SortedDictionary<string, string>(queryParams);
        var queryString = BuildQueryString(sortedParams);
        var checkSum = HmacSHA512(_hashSecret, queryString);
        return checkSum.Equals(secureHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string BuildQueryString(SortedDictionary<string, string> data)
    {
        var queryString = string.Join("&", 
            data.Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));
        return queryString;
    }

    private string HmacSHA512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA512(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    private string ExtractOrderIdFromTxnRef(string txnRef)
    {
        // Format: timestamp_orderId
        var parts = txnRef.Split('_');
        return parts.Length > 1 ? parts[1] : "0";
    }
}
