// ...new file...
using CelebrateMeProto.Models;

namespace CelebrateMeProto.ViewModels;

public class AdminDashboardViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<Order> RecentOrders { get; set; } = new();

    public int TotalOrders => RecentOrders?.Count ?? 0;
    public int PendingOrders => RecentOrders?.Count(o => o.Status == OrderStatus.Pending) ?? 0;
}