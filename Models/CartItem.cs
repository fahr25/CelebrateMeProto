// ...new file...
namespace CelebrateMeProto.Models;

// lightweight session DTO for the shop flow
public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UnitPoints { get; set; }
    public int Quantity { get; set; }
    public int Subtotal => UnitPoints * Quantity;
}

public class OrderDraft
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int PointsAssigned { get; set; }
    public List<CartItem> Items { get; set; } = new();
    public int PointsUsed => Items.Sum(i => i.Subtotal);
    public int PointsRemaining => PointsAssigned - PointsUsed;
}