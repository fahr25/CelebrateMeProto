// ...new file...
using CelebrateMeProto.Models;

namespace CelebrateMeProto.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task AddAsync(Order order);
    Task UpdateStatusAsync(int id, OrderStatus status);
    Task RefundAsync(int id);
}