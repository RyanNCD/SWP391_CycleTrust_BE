using System;
using System.Collections.Generic;

namespace CycleTrust.Application.DTOs
{
    // Dashboard Summary Response
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

    // Revenue Data Point
    public class RevenueDataDto
    {
        public string Period { get; set; } = string.Empty; // "Tháng 1", "Tuần 5", etc.
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    // Order Status Distribution
    public class OrderStatusDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    // User Growth Data
    public class UserGrowthDto
    {
        public string Period { get; set; } = string.Empty;
        public int Buyers { get; set; }
        public int Sellers { get; set; }
    }

    // Top Listing Data
    public class TopListingDto
    {
        public string Name { get; set; } = string.Empty;
        public int Sales { get; set; }
        public decimal Revenue { get; set; }
    }

    // Recent Activity
    public class RecentActivityDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // order, user, listing, dispute
        public string Message { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }

    // Complete Dashboard Response
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
