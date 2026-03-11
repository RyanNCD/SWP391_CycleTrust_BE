using CycleTrust.Core.Enums;

namespace CycleTrust.Application.DTOs;

public class VNPayPaymentRequestDto
{
    public long OrderId { get; set; }
    public long Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
    public PaymentType PaymentType { get; set; }
}

public class VNPayPaymentResponseDto
{
    public bool Success { get; set; }
    public string PaymentUrl { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class VNPayReturnDto
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
    public string? BankCode { get; set; }
    public string? CardType { get; set; }
    public DateTime PayDate { get; set; }
    public string? Message { get; set; }
}

public static class VNPayResponseCode
{
    public const string SUCCESS = "00";
    public const string PENDING = "01";
    public const string INVALID_SIGNATURE = "97";
    public const string FAILURE = "99";

    public static string GetMessage(string code)
    {
        return code switch
        {
            SUCCESS => "Giao dịch thành công",
            PENDING => "Giao dịch đang chờ xử lý",
            "02" => "Merchant không hợp lệ",
            "03" => "Dữ liệu gửi sang không đúng định dạng",
            "04" => "Không cho phép thanh toán",
            "05" => "Giao dịch không thành công",
            "06" => "Giao dịch bị reversal",
            "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ",
            "09" => "Thẻ/Tài khoản chưa đăng ký dịch vụ InternetBanking",
            "10" => "Thẻ/Tài khoản không đúng hoặc chưa được kích hoạt",
            "11" => "Thẻ/Tài khoản đã hết hạn",
            "12" => "Thẻ/Tài khoản bị khóa",
            "13" => "Sai mật khẩu xác thực giao dịch (OTP)",
            "24" => "Khách hàng hủy giao dịch",
            "51" => "Tài khoản không đủ số dư",
            "65" => "Tài khoản đã vượt quá hạn mức giao dịch trong ngày",
            "75" => "Ngân hàng thanh toán đang bảo trì",
            "79" => "Giao dịch vượt quá số lần nhập sai mật khẩu",
            INVALID_SIGNATURE => "Chữ ký không hợp lệ",
            FAILURE => "Giao dịch thất bại",
            _ => "Lỗi không xác định"
        };
    }
}
