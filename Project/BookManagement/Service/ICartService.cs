using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using static BookManagement.Constant.Enumerations;

namespace BookManagement.Service
{
    public interface ICartService : IBaseService<Cart>
    {
        Task CreateNewOrder(int userId, CartConfirmModel model);
        Task<PagingModel<OrderViewModel>> GetPagingOrder(OrderStatus status, int? pageIndex, int? userId = null);
        Task<OrderViewModel> GetOrderDetail(int orderId);
        Task UpdateOrderStatus(int orderId, OrderStatus status);
        Task CancelOrder(int orderId, string reason);
    }
}
