
namespace OAIM.Application.IServices
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CreateOrderDto orderDto);
        Order GetOrderById(int orderId);
        Task<PagedResult<Order>> GetAllOrders(int pageNumber, int pageSize);
        Task<Order> UpdateOrder(int id, CreateOrderDto orderDto);
        void DeleteOrder(int orderId);
    }
}
