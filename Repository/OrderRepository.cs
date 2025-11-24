// ...new file...
using Microsoft.EntityFrameworkCore;
using CelebrateMeProto.Data;
using CelebrateMeProto.Models;

namespace CelebrateMeProto.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _db;
    public OrderRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Order>> GetAllAsync() =>
        _db.Orders.Include(o => o.Items).OrderByDescending(o => o.CreatedAt).ToListAsync();

    public Task<Order?> GetByIdAsync(int id) =>
        _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);

    public async Task AddAsync(Order order)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateStatusAsync(int id, OrderStatus status)
    {
        var o = await _db.Orders.FindAsync(id);
        if (o == null) return;
        o.Status = status;
        await _db.SaveChangesAsync();
    }

    public async Task RefundAsync(int id)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return;

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // restore inventory
            foreach (var it in order.Items)
            {
                var p = await _db.Products.FindAsync(it.ProductId);
                if (p != null) p.Inventory += it.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}