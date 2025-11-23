using System.ComponentModel.DataAnnotations;

namespace PsCoreDemo.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;

    public int Points { get; set; }
    public int Inventory { get; set; }

    public int MinAge { get; set; }
    public int MaxAge { get; set; }

    public Category Category { get; set; }
}
