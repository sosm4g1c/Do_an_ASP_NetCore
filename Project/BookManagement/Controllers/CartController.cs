using AutoMapper;
using BookManagement.Constant;
using BookManagement.Data;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using BookManagement.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using System.Text;
using X.PagedList;
using static BookManagement.Constant.Enumerations;

namespace BookManagement.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUserConfig _userConfig;
        private readonly ICartService _cartService;
        private readonly IBaseService<Book> _bookService;
        private readonly IBaseService<Category> _cateService;
        private readonly IBaseService<Order> _orderService;
        private readonly IBaseService<Voucher> _voucherService;
        private readonly IBaseService<Delivery> _deliveryService;
        private readonly IBaseService<OrderDetail> _orderDetailService;

        public CartController(IUserConfig userConfig,
            IBaseService<OrderDetail> orderDetailService,
            IBaseService<Delivery> deliveryService,
            IBaseService<Voucher> voucherService,
            IBaseService<Category> cateService,
            IBaseService<Order> orderService,
            IBaseService<Book> bookService,
            ICartService cartService,
            IMapper mapper)
        {
            _mapper = mapper;
            _userConfig = userConfig;
            _cartService = cartService;
            _bookService = bookService;
            _cateService = cateService;
            _orderService = orderService;
            _voucherService = voucherService;
            _deliveryService = deliveryService;
            _orderDetailService = orderDetailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? voucherId, int? deliveryId)
        {
            var userId = _userConfig.GetUserId();

            ViewBag.ToastType = Constants.None;

            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var cartList = await _cartService.GetList(x => x.UserId == userId);

            var model = await GetCartModel(cartList);

            ViewBag.CartCount = cartList.Count;
            ViewBag.ErrorProduct = model.CartItems.Count(x => !string.IsNullOrEmpty(x.ErrorMessage));

            var deliveries = await _deliveryService.GetList(x => x.IsActive);

            var deliveryModels = deliveries.OrderBy(x => x.Cost).Select(x => new ItemDropdownModel()
            {
                Id = x.Id,
                Value = x.Cost,
                Name = string.Concat(x.DeliveryName, $" ({x.Cost.ToString("#,##0")}đ)")
            });

            ViewBag.DeliveryList = new SelectList(deliveryModels, "Id", "Name");

            // Mặc định set cho hình thức vận chuyển
            if (deliveryId != null && deliveryId > 0)
            {
                var delivery = await _deliveryService.GetEntityById(deliveryId ?? 0);
                if (delivery != null)
                {
                    model.DeliveryId = delivery.Id;
                    model.ShipCost = delivery.Cost;
                }
            }
            else
            {
                var delivery = deliveries.OrderBy(x => x.Cost).FirstOrDefault();
                if (delivery != null)
                {
                    model.DeliveryId = delivery.Id;
                    model.ShipCost = delivery.Cost;
                }
            }

            if(voucherId != null && voucherId > 0)
            {
                var voucher = await _voucherService.GetEntityById(voucherId ?? 0);
                if (voucher != null)
                {
                    model.VoucherId = voucher.Id;
                    model.VoucherCode = voucher.VoucherCode;
                    model.Discount = voucher.Discount;
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string voucherCode, int deliveryId)
        {
            var userId = _userConfig.GetUserId();
            var cartList = await _cartService.GetList(x => x.UserId == userId);
            ViewBag.CartCount = cartList?.Count ?? 0;

            var model = await GetCartModel(cartList);
            ViewBag.ErrorProduct = model.CartItems.Count(x => !string.IsNullOrEmpty(x.ErrorMessage));

            var delivery = await _deliveryService.GetEntityById(deliveryId);
            if (delivery != null)
            {
                model.DeliveryId = delivery.Id;
                model.ShipCost = delivery.Cost;
            }

            if (string.IsNullOrEmpty(voucherCode))
            {
                ViewBag.ToastType = Constants.Error;
                ViewBag.ToastMessage = "Vui lòng nhập mã.";
            }
            else
            {
                var voucher = await _voucherService.Get(x => x.IsActive && x.VoucherCode.Trim().ToLower().Equals(voucherCode.Trim().ToLower()));

                if (voucher == null)
                {
                    ViewBag.ToastType = Constants.Error;
                    ViewBag.ToastMessage = "Mã giảm giá không khả dụng.";
                }
                else
                {
                    if (voucher.Quantity <= voucher.UsedNumber)
                    {
                        ViewBag.ToastType = Constants.Error;
                        ViewBag.ToastMessage = "Mã giảm giá đã sử dụng hết, vui lòng chọn mã khác.";
                    }
                    else if (voucher.MinAmount > model.TotalMoney)
                    {
                        ViewBag.ToastType = Constants.Error;
                        ViewBag.ToastMessage = $"Chưa đạt giá trị đơn hàng tối thiểu {voucher.MinAmount.ToString("#,##0")} đ";
                    }
                    else
                    {
                        ViewBag.ToastType = Constants.Success;
                        ViewBag.ToastMessage = "Đã áp mã giảm giá.";

                        model.VoucherId = voucher.Id;
                        model.VoucherCode = voucher.VoucherCode;
                        model.Discount = voucher.Discount;
                    }
                }
            }

            var deliveries = await _deliveryService.GetList(x => x.IsActive);

            var deliveryModels = deliveries.OrderBy(x => x.Cost).Select(x => new ItemDropdownModel()
            {
                Id = x.Id,
                Value = x.Cost,
                Name = string.Concat(x.DeliveryName, $" ({x.Cost.ToString("#,##0")}đ)")
            });

            ViewBag.DeliveryList = new SelectList(deliveryModels, "Id", "Name");

            return View(model);
        }

        private async Task<CartModel> GetCartModel(List<Cart> cartList)
        {
            var model = new CartModel();

            if (cartList != null && cartList.Any())
            {
                var books = await _bookService.GetList(x => cartList.Select(x => x.BookId).Contains(x.Id));

                var joinBook = from c in cartList
                               join b in books on c.BookId equals b.Id
                               join ca in _cateService.GetDbSet().ToList()
                               on b.CategoryId equals ca.Id
                               select new CartItemModel
                               {
                                   Id = c.Id,
                                   UserId = c.UserId,
                                   BookId = c.BookId,
                                   Quantity = c.Quantity,
                                   BookImage = b.BookImage,
                                   BookName = b.BookName,
                                   MaxQuantity = b.Quantity,
                                   Price = (b.PriceDiscount != null && b.PriceDiscount != 0 ? (int)b.PriceDiscount : b.Price),
                                   PriceOriginal = (b.PriceDiscount != null && b.PriceDiscount != 0 ? b.Price : null),
                                   ErrorMessage = ((b.Quantity <= 0 || !b.IsActive || !ca.IsActive) ? "Sản phẩm đã bán hết hoặc không khả dụng"
                                                    : (c.Quantity > b.Quantity ? "Số lượng sản phẩm vượt quá số lượng có sẵn" : string.Empty))
                               };

                model.CartItems = joinBook.ToList();
            }

            return model;
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmOrder(int deliveryId, int? voucherId)
        {
            var userId = _userConfig.GetUserId();
            var cartList = await _cartService.GetList(x => x.UserId == userId);
            ViewBag.CartCount = cartList?.Count ?? 0;

            var cartModel = await GetCartModel(cartList);

            if (voucherId != null && voucherId > 0)
            {
                var voucher = await _voucherService.GetEntityById(voucherId ?? 0);

                cartModel.VoucherCode = voucher.VoucherCode;
                cartModel.Discount = voucher.Discount;
            }

            var delivery = await _deliveryService.GetEntityById(deliveryId);
            if (delivery != null)
            {
                cartModel.DeliveryId = deliveryId;
                cartModel.ShipCost = delivery.Cost;
            }

            ViewBag.CartInfo = cartModel;

            var model = new CartConfirmModel()
            {
                OrderCode = RandomString(10),
                VoucherId = voucherId,
                ShipCost = cartModel.ShipCost,
                Discount = cartModel.Discount,
                TotalMoney = cartModel.TotalMoney,
                PaymentType = PaymentType.Cod,
                DeliveryId = deliveryId,
            };

  

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(CartConfirmModel model)
        {
            Console.WriteLine(model.PaymentType);
            var userId = _userConfig.GetUserId();
            var cartList = await _cartService.GetList(x => x.UserId == userId);

            if (ModelState.IsValid)
            {
                if (cartList == null || !cartList.Any())
                {
                    TempData["ToastMessage"] = "Không có sản phẩm trong giỏ hàng.";
                    TempData["ToastType"] = Constants.Error;

                    return RedirectToAction("Index", "Cart");
                }
                else
                {
                    await _cartService.CreateNewOrder(userId, model);

                    TempData["ToastMessage"] = "Đã xác nhận đơn hàng.";
                    TempData["ToastType"] = Constants.Success;

                    return RedirectToAction("Waiting", "Cart");
                }
            }

            ViewBag.CartCount = cartList?.Count ?? 0;

            var cartModel = await GetCartModel(cartList);

            if (model.VoucherId != null && model.VoucherId > 0)
            {
                var voucher = await _voucherService.GetEntityById(model.VoucherId ?? 0);

                cartModel.VoucherCode = voucher.VoucherCode;
                cartModel.Discount = voucher.Discount;
            }

            var delivery = await _deliveryService.GetEntityById(model.DeliveryId);
            if (delivery != null)
            {
                cartModel.DeliveryId = delivery.Id;
                cartModel.ShipCost = delivery.Cost;
            }

            ViewBag.CartInfo = cartModel;

            return View(model);
        }

        public async Task<IActionResult> Waiting(int? pageIndex)
        {
            ViewBag.ToastType = Constants.None;

            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var userId = _userConfig.GetUserId();
            ViewBag.CartCount = await _cartService.Count(x => x.UserId == userId);

            var pagingResult = await _cartService.GetPagingOrder(Enumerations.OrderStatus.Waiting, pageIndex, userId);

            return View(pagingResult);
        }
        public async Task<IActionResult> Delivering(int? pageIndex)
        {
            ViewBag.ToastType = Constants.None;

            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var userId = _userConfig.GetUserId();
            ViewBag.CartCount = await _cartService.Count(x => x.UserId == userId);

            var pagingResult = await _cartService.GetPagingOrder(Enumerations.OrderStatus.Shipping, pageIndex, userId);

            return View(pagingResult);
        }
        public async Task<IActionResult> OrderComplete(int? pageIndex)
        {
            ViewBag.ToastType = Constants.None;

            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var userId = _userConfig.GetUserId();
            ViewBag.CartCount = await _cartService.Count(x => x.UserId == userId);

            var pagingResult = await _cartService.GetPagingOrder(Enumerations.OrderStatus.Complete, pageIndex, userId);

            return View(pagingResult);
        }
        public async Task<IActionResult> OrderCancel(int? pageIndex)
        {
            ViewBag.ToastType = Constants.None;

            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var userId = _userConfig.GetUserId();
            ViewBag.CartCount = await _cartService.Count(x => x.UserId == userId);

            var pagingResult = await _cartService.GetPagingOrder(Enumerations.OrderStatus.Cancel, pageIndex, userId);

            return View(pagingResult);
        }

        [HttpGet]
        public async Task<IActionResult> AddToCart(int bookId, int? quantity)
        {
            var userId = _userConfig.GetUserId();
            var book = await _bookService.GetEntityById(bookId);
            var bookUser = await _cartService.Get(x => x.UserId == userId && x.BookId == bookId);

            if (bookUser != null)
            {
                bookUser.Quantity += ((quantity != null && quantity > 0) ? quantity.Value : 1);
                bookUser.Quantity = bookUser.Quantity > book.Quantity ? book.Quantity : bookUser.Quantity;

                await _cartService.Update(bookUser);
            }
            else
            {
                var newCart = new Cart()
                {
                    BookId = bookId,
                    UserId = userId,
                    Quantity = (quantity != null && quantity > 0) ? quantity.Value : 1
                };

                newCart.Quantity = newCart.Quantity > book.Quantity ? book.Quantity : newCart.Quantity;

                await _cartService.Insert(newCart);
            }

            TempData["ToastMessage"] = "Đã thêm sản phẩm vào giỏ hàng.";
            TempData["ToastType"] = Constants.Success;

            return RedirectToAction("Index");
        }

        [HttpPut]
        public async Task<IActionResult> ChangeQuantity(int id, int quantity)
        {
            var redirectUrl = Url.Action("Index", "Cart");
            var cart = await _cartService.GetEntityById(id);

            cart.Quantity = quantity;
            await _cartService.Update(cart);

            return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveBook(int id)
        {
            var redirectUrl = Url.Action("Index", "Cart");

            await _cartService.Delete(id);

            return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var redirectUrl = string.Empty;
            await _cartService.UpdateOrderStatus(orderId, status);

            switch (status)
            {
                case OrderStatus.Shipping:
                    TempData["ToastMessage"] = "Sẵn sàng bàn giao cho đơn vị vận chuyển.";
                    TempData["ToastType"] = Constants.Success;

                    redirectUrl = Url.Action("Delivering", "Admin");
                    break;
                case OrderStatus.Complete:
                    TempData["ToastMessage"] = "Đơn hàng đã hoàn thành.";
                    TempData["ToastType"] = Constants.Success;

                    redirectUrl = Url.Action("OrderComplete", "Cart");
                    break;
            }

            return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
        }

        [HttpPut]
        public async Task<IActionResult> CancelOrder(int orderId, ReasonCancel reason)
        {
            var redirectUrl = Url.Action("OrderCancel", "Cart");
            var strReason = string.Empty;

            switch (reason)
            {
                case ReasonCancel.ChangeInfo:
                    strReason = "(người mua): Thay đổi thông tin giao hàng";
                    break;
                case ReasonCancel.NotBuy:
                    strReason = "(người mua): Đổi ý, không muốn mua nữa";
                    break;
                case ReasonCancel.WrongOrder:
                    strReason = "(người mua): Đặt nhầm sản phẩm";
                    break;
                case ReasonCancel.NotVoucher:
                    strReason = "(người mua): Chưa áp mã giảm giá";
                    break;
                case ReasonCancel.Other:
                    strReason = "(người mua): Lý do khác";
                    break;
            }

            await _cartService.CancelOrder(orderId, strReason);

            TempData["ToastMessage"] = "Đã hủy đơn hàng.";
            TempData["ToastType"] = Constants.Success;

            return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
        }

        [HttpPut]
        public async Task<IActionResult> CancelOrderShop(int orderId, ReasonCancelShop reason)
        {
            var redirectUrl = Url.Action("OrderCancel", "Admin");
            var strReason = string.Empty;

            switch (reason)
            {
                case ReasonCancelShop.SoldOut:
                    strReason = "(người bán): Hết hàng";
                    break;
                case ReasonCancelShop.NoContact:
                    strReason = "(người bán): Không liên hệ được khách hàng";
                    break;
                case ReasonCancelShop.Other:
                    strReason = "(người bán): Lý do khác";
                    break;
            }

            await _cartService.CancelOrder(orderId, strReason);

            TempData["ToastMessage"] = "Đã hủy đơn hàng.";
            TempData["ToastType"] = Constants.Success;

            return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
        }

        public IActionResult PrintOrder()
        {
            return View();
        }

        public async Task<IActionResult> PrintOrderPdf(int orderId)
        {
            var result = await _cartService.GetOrderDetail(orderId);

            var report = new ViewAsPdf("PrintOrder", result)
            {
                PageSize = Rotativa.AspNetCore.Options.Size.A5,
            };
            return report;
        }

        // Generates a random string with a given size.
        public string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.

            // char is a single Unicode character
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length = 26

            for (var i = 0; i < size; i++)
            {
                var @char = (char)new Random().Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }
    }
}
