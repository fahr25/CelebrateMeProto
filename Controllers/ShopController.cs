using Microsoft.AspNetCore.Mvc;
using CelebrateMeProto.Models;
using ProductCategory = CelebrateMeProto.Models.Category;
using CelebrateMeProto.Repositories;
using CelebrateMeProto.Data;
using System.Text.Json;

namespace CelebrateMeProto.Controllers;

public class ShopController : Controller
{
    private readonly IProductRepository _products;
    private readonly IOrderRepository _orders;
    private readonly ApplicationDbContext _db;
    private const string SESSION_KEY = "OrderDraft";

    public ShopController(IProductRepository products, IOrderRepository orders, ApplicationDbContext db)
    {
        _products = products;
        _orders = orders;
        _db = db;
    }

    // GET: /Shop/StartOrder
    public IActionResult StartOrder()
    {
        var draft = GetDraft() ?? new OrderDraft();
        return View(draft);
    }

    // POST: /Shop/StartOrder
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult StartOrder(OrderDraft model)
    {
        if (!ModelState.IsValid) return View(model);
        SetDraft(model);
        return RedirectToAction("Category", new { index = 1 });
    }

    // GET: /Shop/Category/{index}
    public async Task<IActionResult> Category(int index)
    {
        var categories = new[] { ProductCategory.Books, ProductCategory.Toys, ProductCategory.Gifts, ProductCategory.Treats };
        if (index < 1 || index > categories.Length) return RedirectToAction("Review");

        var cat = categories[index - 1];
        var prods = (await _products.GetAllAsync()).Where(p => p.Category == cat).ToList();
        ViewData["Step"] = index;
        ViewData["TotalSteps"] = categories.Length;
        ViewData["PointsRemaining"] = GetDraft()?.PointsRemaining ?? 0;
        return View(prods);
    }

    // POST: /Shop/AddItem
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(int productId, int quantity = 1)
    {
        var p = await _products.GetByIdAsync(productId);
        var draft = GetDraft() ?? new OrderDraft();
        if (p == null) return NotFound();

        if (quantity < 1) quantity = 1;
        if (quantity > p.Inventory)
        {
            TempData["Error"] = "Requested quantity exceeds inventory.";
            return RedirectToAction("Category", new { index = 1 });
        }

        var wantPoints = p.Points * quantity;
        if (draft.PointsRemaining < wantPoints)
        {
            TempData["Error"] = "Not enough points to add this item.";
            return RedirectToAction("Category", new { index = 1 });
        }

        var existing = draft.Items.FirstOrDefault(i => i.ProductId == p.Id);
        if (existing != null) existing.Quantity += quantity;
        else draft.Items.Add(new CartItem
        {
            ProductId = p.Id,
            ProductName = p.Name,
            UnitPoints = p.Points,
            Quantity = quantity
        });

        SetDraft(draft);
        return RedirectToAction("Review");
    }

    // GET: /Shop/Review
    public IActionResult Review()
    {
        var draft = GetDraft() ?? new OrderDraft();
        return View(draft);
    }

    // POST: /Shop/Checkout
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        var draft = GetDraft();
        if (draft == null || !draft.Items.Any()) return RedirectToAction("StartOrder");
        if (draft.PointsRemaining < 0)
        {
            TempData["Error"] = "Insufficient points.";
            return RedirectToAction("Review");
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                CustomerName = draft.CustomerName,
                CustomerEmail = draft.CustomerEmail,
                PointsAssigned = draft.PointsAssigned,
                PointsUsed = draft.PointsUsed,
                TotalItems = draft.Items.Sum(i => i.Quantity),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            };

            foreach (var ci in draft.Items)
            {
                var p = await _db.Products.FindAsync(ci.ProductId);
                if (p == null) throw new Exception("Product not found during checkout.");
                if (p.Inventory < ci.Quantity) throw new Exception($"Insufficient inventory for {p.Name}.");
                p.Inventory -= ci.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.ProductName,
                    UnitPoints = ci.UnitPoints,
                    Quantity = ci.Quantity,
                    SubtotalPoints = ci.UnitPoints * ci.Quantity
                });

                _db.Products.Update(p);
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            HttpContext.Session.Remove(SESSION_KEY);
            return RedirectToAction("Confirmation", new { id = order.Id });
        }
        catch
        {
            await tx.RollbackAsync();
            TempData["Error"] = "Checkout failed. Try again.";
            return RedirectToAction("Review");
        }
    }

    // GET: /Shop/Confirmation/{id}
    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await _orders.GetByIdAsync(id);
        if (order == null) return NotFound();
        return View(order);
    }

    // session helpers
    private OrderDraft? GetDraft()
    {
        var str = HttpContext.Session.GetString(SESSION_KEY);
        if (string.IsNullOrEmpty(str)) return null;
        return JsonSerializer.Deserialize<OrderDraft>(str);
    }

    private void SetDraft(OrderDraft draft)
    {
        HttpContext.Session.SetString(SESSION_KEY, JsonSerializer.Serialize(draft));
    }
}