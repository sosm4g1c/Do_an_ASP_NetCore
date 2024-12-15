using AutoMapper;
using BookManagement.Constant;
using BookManagement.Data;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using X.PagedList;
using static BookManagement.Constant.Enumerations;

namespace BookManagement.Service
{
    public class CartService : BaseService<Cart>, ICartService
    {
        private readonly IMapper _mapper;
        private readonly IBaseService<User> _userService;
        private readonly IBaseService<Book> _bookService;
        private readonly IBaseService<Order> _orderService;
        private readonly IBaseService<Voucher> _voucherService;
        private readonly IBaseService<Delivery> _deliveryService;
        private readonly IBaseService<OrderDetail> _orderDetailService;

        public CartService(IGenericRepository<Cart> baseRepo,
            ILogger<Cart> logger,
            IBaseService<User> userService,
            IBaseService<Book> bookService,
            IBaseService<Order> orderService,
            IBaseService<Voucher> voucherService,
            IBaseService<Delivery> deliveryService,
            IBaseService<OrderDetail> orderDetailService,
            IMapper mapper) : base(baseRepo, logger)
        {
            _mapper = mapper;
            _userService = userService;
            _bookService = bookService;
            _orderService = orderService;
            _voucherService = voucherService;
            _deliveryService = deliveryService;
            _orderDetailService = orderDetailService;
        }

        public async Task CreateNewOrder(int userId, CartConfirmModel model)
        {
            // Xử lý đặt hàng, insert vào bảng đơn hàng
            var order = new Order()
            {
                UserId = userId,
                VoucherId = model.VoucherId,
                OrderCode = model.OrderCode,
                PaymentType = model.PaymentType,
                DeliveryId = model.DeliveryId,
                CustomerName = model.CustomerName,
                CustomerAddress = model.CustomerAddress,
                PhoneNumber = model.PhoneNumber,
                OrderNote = model.OrderNote,
                Discount = model.Discount,
                ShipCost = model.ShipCost,
                TotalMoney = model.TotalMoney,
                Status = Constant.Enumerations.OrderStatus.Waiting,
                CancelReason = string.Empty,
                CreatedDate = DateTime.Now,
            };
            await _orderService.Insert(order);

            // Insert Detail
            var cartList = await this.GetList(x => x.UserId == userId);
            var books = await _bookService.GetList(x => cartList.Select(x => x.BookId).Contains(x.Id) && x.IsActive);

            var joinBook = from c in cartList
                           join b in books on c.BookId equals b.Id
                           select new OrderDetail()
                           {
                               UserId = userId,
                               OrderId = order.Id,
                               BookId = c.BookId,
                               Quantity = c.Quantity,
                               BookImage = b.BookImage,
                               BookName = b.BookName,
                               PriceBuy = (b.PriceDiscount != null && b.PriceDiscount != 0 ? (int)b.PriceDiscount : b.Price) * c.Quantity,
                               CreatedDate = DateTime.Now
                           };

            await _orderDetailService.InsertMulti(joinBook.ToList());

            // trừ số lượng còn của sản phẩm
            foreach (var item in joinBook)
            {
                var book = await _bookService.GetEntityById(item.BookId);
                book.SoldQuantity += item.Quantity;
                book.Quantity = (book.Quantity - item.Quantity < 0) ? 0 : (book.Quantity - item.Quantity);
                await _bookService.Update(book);
            }

            // Trừ số lượng nếu có sử dụng voucher
            if (model.VoucherId != null && model.VoucherId > 0)
            {
                var voucher = await _voucherService.GetEntityById(model.VoucherId ?? 0);

                voucher.UsedNumber += 1;
                await _voucherService.Update(voucher);
            }

            // Xóa trong bảng giỏ hàng
            await _baseRepo.DeleteRange(cartList);
            await _baseRepo.SaveChangeAsync();
        }

        public async Task<PagingModel<OrderViewModel>> GetPagingOrder(OrderStatus status, int? pageIndex, int? userId = null)
        {
            var orders = await _orderService.GetList(x => (userId == null ? x.Id > 0 : x.UserId == userId) && x.Status == status);
            var orderData = _mapper.Map<List<OrderViewModel>>(orders);

            foreach (var order in orderData)
            {
                var user = await _userService.GetEntityById(order.UserId);
                order.UserName = user.UserName;
                var delivery = await _deliveryService.GetEntityById(order.DeliveryId);
                order.DeliveryName = delivery.DeliveryName;

                order.OrderDetails = await _orderDetailService.GetList(x => x.OrderId == order.Id);
            }

            var pagingResult = new PagingModel<OrderViewModel>()
            {
                TotalRecord = orders.Count(),
                DataPaging = orderData.OrderByDescending(x => x.UpdatedDate).ToPagedList(pageIndex ?? 1, 10),
            };

            return pagingResult;
        }

        public async Task<OrderViewModel> GetOrderDetail(int orderId)
        {
            var order = await _orderService.GetEntityById(orderId);
            var orderData = _mapper.Map<OrderViewModel>(order);

            var user = await _userService.GetEntityById(order.UserId);
            orderData.UserName = user.UserName;
            var delivery = await _deliveryService.GetEntityById(order.DeliveryId);
            orderData.DeliveryName = delivery.DeliveryName;

            orderData.OrderDetails = await _orderDetailService.GetList(x => x.OrderId == order.Id);

            return orderData;
        }

        public async Task UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = await _orderService.GetEntityById(orderId);

            if (order != null)
            {
                order.Status = status;
                await _orderService.Update(order);
            }
        }

        public async Task CancelOrder(int orderId, string reason)
        {
            var order = await _orderService.GetEntityById(orderId);

            if (order != null)
            {
                order.Status = OrderStatus.Cancel;
                order.CancelReason = reason;
                await _orderService.Update(order);

                // Update cộng lại số sách trong đơn vào số lượng
                var orderDetail = await _orderDetailService.GetList(x => x.OrderId == order.Id);

                foreach (var item in orderDetail)
                {
                    var book = await _bookService.GetEntityById(item.BookId);

                    if (book != null)
                    {
                        book.Quantity += item.Quantity;
                        await _bookService.Update(book);
                    }
                }
            }
        }
    }
}
