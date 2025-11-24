// ...new file...
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelebrateMeProto.Models;

public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Order")]
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UnitPoints { get; set; }
    public int Quantity { get; set; }
    public int SubtotalPoints { get; set; }
}