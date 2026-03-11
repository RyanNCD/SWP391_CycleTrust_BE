using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;

namespace CycleTrust.Application.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardDto> GetDashboardDataAsync(string period, DateTime? fromDate, DateTime? toDate);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly CycleTrustDbContext _context;

        public AnalyticsService(CycleTrustDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardDataAsync(string period = "monthly", DateTime? fromDate = null, DateTime? toDate = null)
        {
            toDate ??= DateTime.Now;
            fromDate ??= period switch
            {
                "daily" => toDate.Value.AddDays(-30),
                "weekly" => toDate.Value.AddDays(-90),
                _ => toDate.Value.AddYears(-1)
            };

            var dashboard = new DashboardDto
            {
                Summary = await GetSummaryAsync(fromDate.Value, toDate.Value),
                RevenueData = await GetRevenueDataAsync(period, fromDate.Value, toDate.Value),
                OrderStatusData = await GetOrderStatusDataAsync(),
                UserGrowthData = await GetUserGrowthDataAsync(period, fromDate.Value, toDate.Value),
                TopListings = await GetTopListingsAsync(),
                RecentActivities = await GetRecentActivitiesAsync()
            };

            return dashboard;
        }

        private async Task<DashboardSummaryDto> GetSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            var completedOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.COMPLETED)
                .ToListAsync();

            var currentPeriodOrders = completedOrders
                .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
                .ToList();

            var previousPeriodStart = fromDate.AddDays(-(toDate - fromDate).Days);
            var previousPeriodOrders = completedOrders
                .Where(o => o.CreatedAt >= previousPeriodStart && o.CreatedAt < fromDate)
                .ToList();

            var totalRevenue = currentPeriodOrders.Sum(o => o.PriceAmount);
            var previousRevenue = previousPeriodOrders.Sum(o => o.PriceAmount);
            var revenueGrowth = previousRevenue > 0 
                ? ((totalRevenue - previousRevenue) / previousRevenue) * 100 
                : 0;

            var totalOrders = currentPeriodOrders.Count;
            var previousOrderCount = previousPeriodOrders.Count;
            var orderGrowth = previousOrderCount > 0
                ? ((totalOrders - previousOrderCount) / (decimal)previousOrderCount) * 100
                : 0;

            var totalUsers = await _context.Users.CountAsync();
            var previousUsers = await _context.Users
                .Where(u => u.CreatedAt < fromDate)
                .CountAsync();
            var userGrowth = previousUsers > 0
                ? ((totalUsers - previousUsers) / (decimal)previousUsers) * 100
                : 0;

            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            return new DashboardSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalUsers = totalUsers,
                AverageOrderValue = avgOrderValue,
                RevenueGrowth = (decimal)Math.Round((double)revenueGrowth, 1),
                OrderGrowth = (decimal)Math.Round((double)orderGrowth, 1),
                UserGrowth = (decimal)Math.Round((double)userGrowth, 1)
            };
        }

        private async Task<List<RevenueDataDto>> GetRevenueDataAsync(string period, DateTime fromDate, DateTime toDate)
        {
            var orders = await _context.Orders
                .Where(o => o.Status == OrderStatus.COMPLETED 
                    && o.CreatedAt >= fromDate 
                    && o.CreatedAt <= toDate)
                .ToListAsync();

            var groupedData = period switch
            {
                "daily" => orders
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new RevenueDataDto
                    {
                        Period = g.Key.ToString("dd/MM"),
                        Revenue = g.Sum(o => o.PriceAmount),
                        Orders = g.Count()
                    })
                    .OrderBy(x => x.Period)
                    .ToList(),

                "weekly" => orders
                    .GroupBy(o => new
                    {
                        Year = o.CreatedAt.Year,
                        Week = System.Globalization.CultureInfo.CurrentCulture.Calendar
                            .GetWeekOfYear(o.CreatedAt, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                    })
                    .Select(g => new RevenueDataDto
                    {
                        Period = $"T{g.Key.Week}",
                        Revenue = g.Sum(o => o.PriceAmount),
                        Orders = g.Count()
                    })
                    .OrderBy(x => x.Period)
                    .ToList(),

                _ => orders
                    .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                    .Select(g => new RevenueDataDto
                    {
                        Period = $"Tháng {g.Key.Month}",
                        Revenue = g.Sum(o => o.PriceAmount),
                        Orders = g.Count()
                    })
                    .OrderBy(x => x.Period)
                    .ToList()
            };

            return groupedData;
        }

        private async Task<List<OrderStatusDto>> GetOrderStatusDataAsync()
        {
            var statusCounts = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var statusColors = new Dictionary<OrderStatus, (string Name, string Color)>
            {
                { OrderStatus.COMPLETED, ("Hoàn thành", "#52c41a") },
                { OrderStatus.SHIPPING, ("Đang giao", "#1890ff") },
                { OrderStatus.CONFIRMED, ("Chờ xác nhận", "#faad14") },
                { OrderStatus.CANCELED, ("Đã hủy", "#ff4d4f") },
                { OrderStatus.DISPUTED, ("Tranh chấp", "#fa8c16") },
                { OrderStatus.DEPOSIT_PENDING, ("Chờ cọc", "#13c2c2") },
                { OrderStatus.DEPOSIT_PAID, ("Đã cọc", "#722ed1") },
            };

            return statusCounts.Select(sc =>
            {
                var (name, color) = statusColors.GetValueOrDefault(sc.Status, (sc.Status.ToString(), "#d9d9d9"));
                return new OrderStatusDto
                {
                    Name = name,
                    Value = sc.Count,
                    Color = color
                };
            }).ToList();
        }

        private async Task<List<UserGrowthDto>> GetUserGrowthDataAsync(string period, DateTime fromDate, DateTime toDate)
        {
            var users = await _context.Users
                .Where(u => u.CreatedAt >= fromDate && u.CreatedAt <= toDate)
                .ToListAsync();

            var groupedData = period switch
            {
                "daily" => users
                    .GroupBy(u => u.CreatedAt.Date)
                    .Select(g => new UserGrowthDto
                    {
                        Period = g.Key.ToString("dd/MM"),
                        Buyers = g.Count(u => u.Role == UserRole.BUYER),
                        Sellers = g.Count(u => u.Role == UserRole.SELLER)
                    })
                    .OrderBy(x => x.Period)
                    .ToList(),

                "weekly" => users
                    .GroupBy(u => new
                    {
                        Year = u.CreatedAt.Year,
                        Week = System.Globalization.CultureInfo.CurrentCulture.Calendar
                            .GetWeekOfYear(u.CreatedAt, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                    })
                    .Select(g => new UserGrowthDto
                    {
                        Period = $"T{g.Key.Week}",
                        Buyers = g.Count(u => u.Role == UserRole.BUYER),
                        Sellers = g.Count(u => u.Role == UserRole.SELLER)
                    })
                    .OrderBy(x => x.Period)
                    .ToList(),

                _ => users
                    .GroupBy(u => u.CreatedAt.Month)
                    .Select(g => new UserGrowthDto
                    {
                        Period = $"T{g.Key}",
                        Buyers = g.Count(u => u.Role == UserRole.BUYER),
                        Sellers = g.Count(u => u.Role == UserRole.SELLER)
                    })
                    .OrderBy(x => x.Period)
                    .ToList()
            };

            return groupedData;
        }

        private async Task<List<TopListingDto>> GetTopListingsAsync()
        {
            var topListings = await _context.Orders
                .Where(o => o.Status == OrderStatus.COMPLETED)
                .Include(o => o.Listing)
                .GroupBy(o => new { o.ListingId, ListingTitle = o.Listing.Title })
                .Select(g => new TopListingDto
                {
                    Name = g.Key.ListingTitle ?? "Unknown",
                    Sales = g.Count(),
                    Revenue = g.Sum(o => o.PriceAmount)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            return topListings;
        }

        private async Task<List<RecentActivityDto>> GetRecentActivitiesAsync()
        {
            var activities = new List<RecentActivityDto>();

            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.UpdatedAt)
                .Take(3)
                .Select(o => new RecentActivityDto
                {
                    Id = (int)o.Id,
                    Type = "order",
                    Message = $"Đơn hàng #{o.Id} - {o.Status}",
                    Time = GetRelativeTime(o.UpdatedAt)
                })
                .ToListAsync();
            activities.AddRange(recentOrders);

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(2)
                .Select(u => new RecentActivityDto
                {
                    Id = (int)u.Id,
                    Type = "user",
                    Message = $"User mới đăng ký: {u.Email}",
                    Time = GetRelativeTime(u.CreatedAt)
                })
                .ToListAsync();
            activities.AddRange(recentUsers);

            return activities.OrderByDescending(a => a.Id).Take(5).ToList();
        }

        private static string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Vừa xong";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} giờ trước";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} ngày trước";
            
            return dateTime.ToString("dd/MM/yyyy");
        }
    }
}
