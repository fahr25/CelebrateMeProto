// ...new file...
using System.ComponentModel.DataAnnotations;

namespace CelebrateMeProto.Models;

public enum OrderStatus
{
    Pending,
    Completed,
    Cancelled
}

public class Order
{
    [Key]
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    // PointsAssigned captured at order start; PointsUsed computed when checking out.
    public int PointsAssigned { get; set; }
    public int PointsUsed { get; set; }

    public int TotalItems { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}