using AutoMapper;
using BookManagement.Constant;
using BookManagement.Data;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using X.PagedList;
using static BookManagement.Constant.Enumerations;

namespace BookManagement.Service
{
    public class AdminService : IAdminService
    {
        private readonly IMapper _mapper;
        private readonly IBaseService<User> _userService;
        private readonly IBaseService<Book> _bookService;
        private readonly IBaseService<Order> _orderService;
        private readonly IBaseService<Voucher> _voucherService;
        private readonly IBaseService<Category> _categoryService;
        private readonly IBaseService<OrderDetail> _orderDetailService;

        public AdminService(
            IBaseService<User> userService,
            IBaseService<Book> bookService,
            IBaseService<Order> orderService,
            IBaseService<Voucher> voucherService,
            IBaseService<Category> categoryService,
            IBaseService<OrderDetail> orderDetailService,
            IMapper mapper)
        {
            _mapper = mapper;
            _userService = userService;
            _bookService = bookService;
            _orderService = orderService;
            _voucherService = voucherService;
            _categoryService = categoryService;
            _orderDetailService = orderDetailService;
        }

        public async Task<DashboardModel> GetDashboardOverview(int? viewType)
        {
            var model = new DashboardModel();

            model.ViewType = viewType ?? (int)DashboardViewType.Week;
            model.TotalBook = await _bookService.Count(x => x.IsActive);
            model.TotalCategory = await _categoryService.Count(x => x.IsActive);
            model.TotalVoucher = await _voucherService.Count(x => x.IsActive);
            model.OrderWaiting = await _orderService.Count(x => x.Status == OrderStatus.Waiting);

            var parseDate = GetStartDateEndDate(model.ViewType);
            model.StartDate = parseDate.Item1;
            model.EndDate = parseDate.Item2;

            model.TotalOrder.Waiting = await _orderService.Count(x => x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate && x.Status == OrderStatus.Waiting);
            model.TotalOrder.Delivery = await _orderService.Count(x => x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate && x.Status == OrderStatus.Shipping);
            model.TotalOrder.Complete = await _orderService.Count(x => x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate && x.Status == OrderStatus.Complete);
            model.TotalOrder.Cancel = await _orderService.Count(x => x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate && x.Status == OrderStatus.Cancel);

            model.TotalMoney.Waiting = (await _orderService.GetList(x => x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate && x.Status == OrderStatus.Waiting)).Sum(x => x.TotalMoney);
            model.TotalMoney.Delivery = (await _orderService.GetList(x => x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate && x.Status == OrderStatus.Shipping)).Sum(x => x.TotalMoney);
            model.TotalMoney.Complete = (await _orderService.GetList(x => x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate && x.Status == OrderStatus.Complete)).Sum(x => x.TotalMoney);
            model.TotalMoney.Cancel = (await _orderService.GetList(x => x.CreatedDate >= model.StartDate && x.CreatedDate <= model.EndDate && x.Status == OrderStatus.Cancel)).Sum(x => x.TotalMoney);

            var totalOrder = _orderDetailService.GetDbSet();

            var groupOrder = totalOrder.GroupBy(x => new { x.BookId, x.BookName }).Select(x => new BookBestSeller
            {
                BookId = x.Key.BookId,
                BookName = x.Key.BookName,
                TotalSold = x.Sum(t => t.Quantity)
            });

            model.BestSeller = groupOrder.OrderByDescending(x => x.TotalSold).ThenBy(x => x.BookName).Skip(0).Take(10).ToList();

            return model;
        }

        public object UploadImage(UploadModel upload)
        {
            var imgName = Guid.NewGuid().ToString();

            using (Image image = Base64ToImage(upload.BookImageUri))
            {
                string bookDetailImageUri = "\\wwwroot\\uploads\\" + imgName + ".jpg";
                string strFileName = Directory.GetCurrentDirectory() + bookDetailImageUri;
                image.Save(strFileName, ImageFormat.Jpeg);

                if (System.IO.File.Exists(strFileName))
                {
                    return new { Success = true, FileName = imgName + ".jpg" };
                }
                else
                {
                    return new { Success = false };
                }
            }
        }

        /// <summary>
        /// Convert từ Base64 sang Image
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        private Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            Bitmap tempBmp;
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                // Convert byte[] to Image
                ms.Write(imageBytes, 0, imageBytes.Length);
                using (Image image = Image.FromStream(ms, true))
                {
                    //Create another object image for dispose old image handler
                    tempBmp = new Bitmap(image.Width, image.Height);
                    Graphics g = Graphics.FromImage(tempBmp);
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                }
            }
            return tempBmp;
        }

        private Tuple<DateTime, DateTime> GetStartDateEndDate(int viewType)
        {
            switch (viewType)
            {

                case (int)DashboardViewType.Week:
                    // Tính toán ngày đầu tuần (Thứ Hai của tuần này)
                    DateTime firstDayOfWeek = DateTime.Now.AddDays(DayOfWeek.Monday - DateTime.Now.DayOfWeek).Date;

                    // Nếu ngày hôm nay là Chủ Nhật, chúng ta sẽ tính lại từ tuần trước
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        firstDayOfWeek = firstDayOfWeek.AddDays(-7); // Quay lại tuần trước
                    }
                    // Tính ngày cuối tuần (Chủ Nhật của tuần này)
                    DateTime lastDayOfWeek = firstDayOfWeek.AddDays(6).Date; // Chủ Nhật của tuần này

                    // Điều chỉnh để chắc chắn ngày Chủ nhật cũng được tính vào tuần
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        lastDayOfWeek = lastDayOfWeek.AddDays(1); // Nếu hôm nay là Chủ nhật, tính thêm một ngày
                    }
                    return Tuple.Create(firstDayOfWeek, lastDayOfWeek);

                case (int)DashboardViewType.Month:
                    DateTime firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
                    DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddSeconds(-1);

                    return Tuple.Create(firstDayOfMonth, lastDayOfMonth);
                case (int)DashboardViewType.Quarter:
                    int quarterNumber = (DateTime.Now.Month - 1) / 3 + 1;
                    DateTime firstDayOfQuarter = new DateTime(DateTime.Now.Year, (quarterNumber - 1) * 3 + 1, 1).Date;
                    DateTime lastDayOfQuarter = firstDayOfQuarter.AddMonths(3).AddSeconds(-1);

                    return Tuple.Create(firstDayOfQuarter, lastDayOfQuarter);
            }

            return Tuple.Create(DateTime.Now, DateTime.Now);
        }
    }
}
