
namespace OAIM.Application.IServices
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto orderDto);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<PagedResult<Order>> GetAllOrders(int pageNumber, int pageSize);
        Task<OrderResponseDto> UpdateOrder(int id, CreateOrderDto orderDto);
        Task DeleteOrder(int orderId, string user);
        Task<List<Order>> GetOrdersByUserIdAsync(Guid userId, int pageNumber, int pageSize);
    }
}
