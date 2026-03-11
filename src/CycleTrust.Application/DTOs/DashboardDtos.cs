using System;
using System.Collections.Generic;

namespace CycleTrust.Application.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal RevenueGrowth { get; set; } // Percentage
        public decimal OrderGrowth { get; set; }
        public decimal UserGrowth { get; set; }
    }

    public class RevenueDataDto
    {
        public string Period { get; set; } = string.Empty; // "Tháng 1", "Tuần 5", etc.
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class OrderStatusDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class UserGrowthDto
    {
        public string Period { get; set; } = string.Empty;
        public int Buyers { get; set; }
        public int Sellers { get; set; }
    }

    public class TopListingDto
    {
        public string Name { get; set; } = string.Empty;
        public int Sales { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RecentActivityDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // order, user, listing, dispute
        public string Message { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }

    public class DashboardDto
    {
        public DashboardSummaryDto Summary { get; set; } = new();
        public List<RevenueDataDto> RevenueData { get; set; } = new();
        public List<OrderStatusDto> OrderStatusData { get; set; } = new();
        public List<UserGrowthDto> UserGrowthData { get; set; } = new();
        public List<TopListingDto> TopListings { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
    }
}
