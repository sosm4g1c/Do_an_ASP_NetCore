using AutoMapper;
using BookManagement.Constant;
using BookManagement.Data;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using BookManagement.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing;
using System.Drawing.Imaging;
using X.PagedList;
using static BookManagement.Constant.Enumerations;

namespace BookManagement.Controllers
{
    //[Authorize(Roles = Role.Admin)]
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IBaseService<Category> _categoryService;
        private readonly IBaseService<Delivery> _deliveryService;
        private readonly IBaseService<Voucher> _voucherService;
        private readonly IBaseService<Order> _orderService;
        private readonly IBaseService<News> _newsService;
        private readonly IBookService _bookService;
        private readonly IBaseService<User> _userService;
        private readonly IBaseService<BookReview> _reviewService;
        private readonly IConfiguration _configuration;
        private readonly IAdminService _adminService;
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private readonly IUserConfig _userConfig;
        private readonly IMapper _mapper;

        public AdminController(IBaseService<Category> categoryService,
            IBaseService<BookReview> reviewService,
            IWebHostEnvironment environment,
            IBaseService<Delivery> deliveryService,
            IBaseService<Voucher> voucherService,
            IBaseService<Order> orderService,
            IBaseService<News> newsService,
            IBookService bookService,
            IBaseService<User> userService,
            IConfiguration configuration,
            IAdminService adminService,
            ICartService cartService,
            IAuthService authService,
            IUserConfig userConfig,
            IMapper mapper)
        {
            _hostingEnvironment = environment;
            _reviewService = reviewService;
            _deliveryService = deliveryService;
            _categoryService = categoryService;
            _voucherService = voucherService;
            _configuration = configuration;
            _adminService = adminService;
            _orderService = orderService;
            _newsService = newsService;
            _bookService = bookService;
            _cartService = cartService;
            _userService = userService;
            _authService = authService;
            _userConfig = userConfig;
            _mapper = mapper;
        }

        //GET: /Admin/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard(int? viewType)
        {
            var model = await _adminService.GetDashboardOverview(viewType);

            return View(model);
        }

        #region Category Management
        //GET: /Admin/CategoryManagement
        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> CategoryManagement(int? pageIndex)
        {
            ViewBag.ToastType = Constants.None;
            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var categoryAll = _categoryService.GetAll();

            // Join lấy thông tin số lượng sách
            var categories = _mapper.Map<List<CategoryModel>>(categoryAll);

            foreach (var category in categories)
            {
                category.TotalBook = await _bookService.Count(x => x.CategoryId == category.Id);
            }

            var pagingResult = new PagingModel<CategoryModel>()
            {
                TotalRecord = categories.Count(),
                DataPaging = categories.OrderByDescending(x => x.UpdatedDate).ToPagedList(pageIndex ?? 1, 10),
            };

            return View(pagingResult);
        }

        //Get: /Admin/CategoryById
        [HttpGet]
        public async Task<JsonResult> CategoryById(int id)
        {
            var entity = await _categoryService.GetEntityById(id);

            return Json(entity);
        }

        //Get: /Admin/ExistCategoryCode
        [HttpGet]
        public async Task<JsonResult> ExistCategoryCode(string code, int? id)
        {
            var exist = await _categoryService.Exist(x => x.CategoryCode.Trim().ToLower().Equals(code.Trim().ToLower())
                                                            && (id != null && id != 0 ? x.Id != id : x.Id > 0));

            return Json(exist);
        }

        //Post: /Admin/InsertCategory
        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> InsertCategory([FromBody] Category model)
        {
            var redirectUrl = Url.Action("CategoryManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _categoryService.Insert(model);

                TempData["ToastMessage"] = "Thêm mới danh mục thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }

        //Put: /Admin/UpdateCategory
        [HttpPut]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> UpdateCategory([FromBody] Category model)
        {
            var redirectUrl = Url.Action("CategoryManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _categoryService.Update(model);

                TempData["ToastMessage"] = "Cập nhật danh mục thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }

        //Delete: /Admin/DeleteCategory
        [HttpDelete]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var redirectUrl = Url.Action("CategoryManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _categoryService.Delete(id);

                TempData["ToastMessage"] = "Xóa danh mục thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }
        #endregion

        #region Book Management
        //GET: /Admin/BookManagement
        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> BookManagement(int? pageIndex, string? keyword, int? categoryId)
        {
            ViewBag.ToastType = Constants.None;
            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var result = new BookPagingModel()
            {
                CategoryId = categoryId,
                Keyword = keyword,
                //Paging = new PagingModel<Book>()
            };

            var books = await _bookService.GetList(x => (string.IsNullOrEmpty(keyword) ? x.Id > 0 : x.BookName.ToLower().Contains(keyword.ToLower().Trim()))
                && (categoryId != null && categoryId != 0 ? x.CategoryId == categoryId : x.Id > 0));

            result.Paging = new PagingModel<Book>()
            {
                TotalRecord = books.Count(),
                DataPaging = books.OrderByDescending(x => x.UpdatedDate).ToPagedList(pageIndex ?? 1, 10),
            };

            var cate = _categoryService.GetAll();
            cate.Insert(0, new Category()
            {
                CategoryName = "Chọn danh mục"
            });

            SelectList cateList = new SelectList(cate, "Id", "CategoryName");
            // Set vào ViewBag
            ViewBag.CategoryList = cateList;

            return View(result);
        }

        //GET: /Admin/BookDetail
        [HttpGet]
        public async Task<IActionResult> BookDetail(int? id)
        {
            if (id != null)
            {
                ViewData["HeaderTitle"] = "Sửa thông tin sách";
            }
            else
            {
                ViewData["HeaderTitle"] = "Thêm sách mới";
            }

            var cate = await _categoryService.GetList(x => x.IsActive);
            cate.Insert(0, new Category()
            {
                CategoryName = "Chọn danh mục"
            });

            SelectList cateList = new SelectList(cate, "Id", "CategoryName");
            // Set vào ViewBag
            ViewBag.CategoryList = cateList;

            var book = (await _bookService.GetEntityById(id ?? 0)) ?? new Book();

            return View(book);
        }

        //Post: /Admin/BookDetail
        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> BookDetail(Book model)
        {
            var cate = await _categoryService.GetList(x => x.IsActive);
            cate.Insert(0, new Category()
            {
                CategoryName = "Chọn danh mục"
            });

            SelectList cateList = new SelectList(cate, "Id", "CategoryName");
            // Set vào ViewBag
            ViewBag.CategoryList = cateList;

            bool isValid = true;

            if (ModelState.IsValid)
            {
                if (model.CategoryId == 0)
                {
                    ModelState.AddModelError("CategoryId", "Vui lòng chọn danh mục");
                    isValid = false;
                }

                if (model.PriceDiscount > 0 && model.PriceDiscount >= model.Price)
                {
                    ModelState.AddModelError("PriceDiscount", "Giá khuyến mại phải nhỏ hơn giá bán");
                    isValid = false;
                }

                var bookExist = await _bookService.Exist(x => x.BookCode.ToLower().Trim().Equals(model.BookCode.ToLower().Trim()) && model.Id != x.Id);
                if (bookExist)
                {
                    ModelState.AddModelError("BookCode", "Mã sách đã tồn tại");
                    isValid = false;
                }

                if (!isValid)
                {
                    return View(model);
                }

                if (model.Id != 0)
                {
                    await _bookService.Update(model);

                    TempData["ToastMessage"] = "Cập nhật thông tin sách thành công.";
                    TempData["ToastType"] = Constants.Success;
                    return RedirectToAction("BookManagement");
                }
                else
                {
                    await _bookService.Insert(model);

                    TempData["ToastMessage"] = "Thêm sách mới thành công.";
                    TempData["ToastType"] = Constants.Success;
                    return RedirectToAction("BookManagement");
                }
            }

            return View(model);
        }

        //Delete: /Admin/DeleteBook
        [HttpDelete]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var redirectUrl = Url.Action("BookManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _bookService.Delete(id);

                TempData["ToastMessage"] = "Xóa sách thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }

        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public IActionResult UploadImage([FromBody] UploadModel upload)
        {
            if (!string.IsNullOrEmpty(upload.BookImageUri))
            {
                var result = _adminService.UploadImage(upload);
                return Json(result);
            }
            else
            {
                return Json(new { Success = false });
            }
        }
        #endregion

        #region Cart Management
        //GET: /Admin/WaitingDelivery
        [HttpGet]
        public async Task<IActionResult> WaitingDelivery(int? pageIndex)
        {
            var pagingResult = await _cartService.GetPagingOrder(Enumerations.OrderStatus.Waiting, pageIndex);

            return View(pagingResult);
        }
        //GET: /Admin/Delivering
        [HttpGet]
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

            var pagingResult = await _cartService.GetPagingOrder(Enumerations.OrderStatus.Shipping, pageIndex);

            return View(pagingResult);
        }
        //GET: /Admin/OrderComplete
        [HttpGet]
        public async Task<IActionResult> OrderComplete(int? pageIndex)
        {
            var pagingResult = await _cartService.GetPagingOrder(Enumerations.OrderStatus.Complete, pageIndex);

            return View(pagingResult);
        }
        //GET: /Admin/OrderCancel
        [HttpGet]
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

            var pagingResult = await _cartService.GetPagingOrder(Enumerations.OrderStatus.Cancel, pageIndex);

            return View(pagingResult);
        }
        #endregion

        #region Voucher Management
        //GET: /Admin/VoucherManagement
        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public IActionResult VoucherManagement(int? pageIndex)
        {
            ViewBag.ToastType = Constants.None;
            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var vouchers = _voucherService.GetAll();

            var pagingResult = new PagingModel<Voucher>()
            {
                TotalRecord = vouchers.Count(),
                DataPaging = vouchers.OrderByDescending(x => x.UpdatedDate).ToPagedList(pageIndex ?? 1, 10),
            };

            return View(pagingResult);
        }

        //Get: /Admin/VoucherById
        [HttpGet]
        public async Task<JsonResult> VoucherById(int id)
        {
            var entity = await _voucherService.GetEntityById(id);

            return Json(entity);
        }

        //Get: /Admin/ExistVoucherCode
        [HttpGet]
        public async Task<JsonResult> ExistVoucherCode(string code, int? id)
        {
            var exist = await _voucherService.Exist(x => x.VoucherCode.Trim().ToLower().Equals(code.Trim().ToLower())
                                                            && (id != null && id != 0 ? x.Id != id : x.Id > 0));

            return Json(exist);
        }

        //Post: /Admin/InsertVoucher
        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> InsertVoucher([FromBody] Voucher model)
        {
            var redirectUrl = Url.Action("VoucherManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _voucherService.Insert(model);

                TempData["ToastMessage"] = "Thêm mã khuyến mại thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }

        //Put: /Admin/UpdateVoucher
        [HttpPut]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> UpdateVoucher([FromBody] Voucher model)
        {
            var redirectUrl = Url.Action("VoucherManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _voucherService.Update(model);

                TempData["ToastMessage"] = "Cập nhật mã khuyến mại thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }

        //Delete: /Admin/DeleteVoucher
        [HttpDelete]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            var redirectUrl = Url.Action("VoucherManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _voucherService.Delete(id);

                TempData["ToastMessage"] = "Xóa mã khuyến mại thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }
        #endregion

        #region User Management
        //GET: /Admin/UserManagement
        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> UserManagement(int? pageIndex, string? keyword)
        {
            ViewBag.ToastType = Constants.None;
            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var result = new UserPagingModel()
            {
                Keyword = keyword,
                Paging = new PagingModel<UserManagementModel>()
            };
            var currentUserId = _userConfig.GetUserId();

            var users = await _userService.GetList(x => x.Id != currentUserId && !x.IsDelete && (string.IsNullOrEmpty(keyword) ? x.Id > 0 : (x.UserName.ToLower().Contains(keyword.ToLower().Trim())
                || x.LastName.ToLower().Contains(keyword.ToLower().Trim()) || x.FirstName.ToLower().Contains(keyword.ToLower().Trim()))));

            var orders = await _orderService.GetList(x => users.Select(x => x.Id).Contains(x.UserId));

            var userJoin = users.Select(u => new UserManagementModel()
            {
                Id = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                CreatedDate = u.CreatedDate,
                TotalMoney = orders.Where(x => x.UserId == u.Id).Sum(x => x.TotalMoney),
                TotalOrder = orders.Where(x => x.UserId == u.Id).Count(),
                IsActive = u.IsActive,
                RoleType = u.RoleType,
                RoleName = u.RoleName,
            });

            result.Paging = new PagingModel<UserManagementModel>()
            {
                TotalRecord = users.Count(),
                DataPaging = userJoin.OrderByDescending(x => x.CreatedDate).ToPagedList(pageIndex ?? 1, 10),
            };

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> UserDetail(int? id)
        {
            if (id != null)
            {
                ViewData["HeaderTitle"] = "Sửa thông tin tài khoản";
            }
            else
            {
                ViewData["HeaderTitle"] = "Thêm tài khoản";
            }

            // Set vào ViewBag
            ViewBag.GenderList = new SelectList(new List<ItemDropdownModel>()
            {
                new ItemDropdownModel(){ Value = 0, Name = "Chọn giới tính" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Male, Name = "Nam" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Female, Name = "Nữ" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Other, Name = "Khác" },
            }, "Value", "Name");

            ViewBag.RoleList = new SelectList(new List<ItemDropdownModel>()
            {
                new ItemDropdownModel(){ Value = (int)RoleEnum.User, Name = "Người dùng" },
                new ItemDropdownModel(){ Value = (int)RoleEnum.Staff, Name = "Nhân viên" },
            }, "Value", "Name");

            var user = (await _userService.GetEntityById(id ?? 0)) ?? new User()
            {
                Password = await _authService.HashPassword(_configuration.GetSection("Password:Default").Value)
            };

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> UserDetail(User model)
        {
            // Set vào ViewBag
            ViewBag.GenderList = new SelectList(new List<ItemDropdownModel>()
            {
                new ItemDropdownModel(){ Value = 0, Name = "Chọn giới tính" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Male, Name = "Nam" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Female, Name = "Nữ" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Other, Name = "Khác" },
            }, "Value", "Name");

            ViewBag.RoleList = new SelectList(new List<ItemDropdownModel>()
            {
                new ItemDropdownModel(){ Value = (int)RoleEnum.User, Name = "Người dùng" },
                new ItemDropdownModel(){ Value = (int)RoleEnum.Staff, Name = "Nhân viên" },
            }, "Value", "Name");

            var user = await _userService.GetEntityById(model.Id);

            if (user != null)
            {
                model.Password = user.Password;
            }
            else
            {
                model.Password = await _authService.HashPassword(_configuration.GetSection("Password:Default").Value);
            }

            if (ModelState.IsValid)
            {
                var userExist = await _userService.Exist(x => x.UserName.ToLower().Trim().Equals(model.UserName.ToLower().Trim()) && model.Id != x.Id);
                var emailExist = await _userService.Exist(x => x.Email.ToLower().Trim().Equals(model.Email.ToLower().Trim()) && model.Id != x.Id);
                if (userExist && emailExist)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(model);
                }
                else if (userExist)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }
                else if (emailExist)
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(model);
                }
                else
                {
                    if (model.Id != 0)
                    {
                        await _userService.Update(model);

                        TempData["ToastMessage"] = "Cập nhật thông tin tài khoản thành công.";
                        TempData["ToastType"] = Constants.Success;
                        return RedirectToAction("UserManagement");
                    }
                    else
                    {
                        await _userService.Insert(model);

                        TempData["ToastMessage"] = "Đã thêm tài khoản mới với mật khẩu mặc định";
                        TempData["ToastType"] = Constants.Success;
                        return RedirectToAction("UserManagement");
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> LockUser(int userId)
        {
            var user = await _userService.GetEntityById(userId);

            user.IsActive = !user.IsActive;
            await _userService.Update(user);

            TempData["ToastMessage"] = user.IsActive ? $"Đã mở khóa cho tài khoản {user.UserName}." : $"Đã khóa tài khoản {user.UserName}.";
            TempData["ToastType"] = Constants.Success;
            return RedirectToAction("UserManagement");
        }

        [HttpDelete]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var redirectUrl = Url.Action("UserManagement", "Admin");

            if (ModelState.IsValid)
            {
                var user = await _userService.GetEntityById(userId);

                user.IsDelete = true;
                await _userService.Update(user);

                TempData["ToastMessage"] = $"Đã xóa tài khoản {user.UserName} thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }
        #endregion

        #region News Management
        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> NewsManagement(int? pageIndex, string? keyword)
        {
            ViewBag.ToastType = Constants.None;
            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var result = new NewsPagingModel()
            {
                Keyword = keyword,
                Paging = new PagingModel<News>()
            };

            var news = await _newsService.GetList(x => string.IsNullOrEmpty(keyword) ? x.Id > 0 : x.Title.ToLower().Contains(keyword.ToLower().Trim()));

            result.Paging = new PagingModel<News>()
            {
                TotalRecord = news.Count(),
                DataPaging = news.OrderByDescending(x => x.CreatedDate).ToPagedList(pageIndex ?? 1, 10),
            };

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> NewsDetail(int? id)
        {
            if (id != null)
            {
                ViewData["HeaderTitle"] = "Sửa tin tức";
            }
            else
            {
                ViewData["HeaderTitle"] = "Thêm tin tức";
            }

            var news = (await _newsService.GetEntityById(id ?? 0)) ?? new News();

            return View(news);
        }

        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> NewsDetail(News model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id != 0)
                {
                    await _newsService.Update(model);

                    TempData["ToastMessage"] = "Cập nhật tin tức thành công.";
                    TempData["ToastType"] = Constants.Success;
                    return RedirectToAction("NewsManagement");
                }
                else
                {
                    await _newsService.Insert(model);

                    TempData["ToastMessage"] = "Thêm tin tức thành công.";
                    TempData["ToastType"] = Constants.Success;
                    return RedirectToAction("NewsManagement");
                }
            }

            return View(model);
        }

        [HttpDelete]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteNews(int id)
        {
            var redirectUrl = Url.Action("NewsManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _newsService.Delete(id);

                TempData["ToastMessage"] = "Xóa tin tức thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }
        #endregion

        #region Delivery Management
        //GET: /Admin/DeliveryManagement
        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public IActionResult DeliveryManagement(int? pageIndex)
        {
            ViewBag.ToastType = Constants.None;
            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var deliveries = _deliveryService.GetAll();

            var pagingResult = new PagingModel<Delivery>()
            {
                TotalRecord = deliveries.Count(),
                DataPaging = deliveries.OrderByDescending(x => x.UpdatedDate).ToPagedList(pageIndex ?? 1, 10),
            };

            return View(pagingResult);
        }

        //Get: /Admin/DeliveryById
        [HttpGet]
        public async Task<JsonResult> DeliveryById(int id)
        {
            var entity = await _deliveryService.GetEntityById(id);

            return Json(entity);
        }

        //Get: /Admin/ExistDeliveryCode
        [HttpGet]
        public async Task<JsonResult> ExistDeliveryCode(string code, int? id)
        {
            var exist = await _deliveryService.Exist(x => x.DeliveryCode.Trim().ToLower().Equals(code.Trim().ToLower())
                                                            && (id != null && id != 0 ? x.Id != id : x.Id > 0));

            return Json(exist);
        }

        //Post: /Admin/InsertDelivery
        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> InsertDelivery([FromBody] Delivery model)
        {
            var redirectUrl = Url.Action("DeliveryManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _deliveryService.Insert(model);

                TempData["ToastMessage"] = "Thêm phương thức vận chuyển thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }

        //Put: /Admin/UpdateDelivery
        [HttpPut]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> UpdateDelivery([FromBody] Delivery model)
        {
            var redirectUrl = Url.Action("DeliveryManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _deliveryService.Update(model);

                TempData["ToastMessage"] = "Cập nhật phương thức vận chuyển thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }

        //Delete: /Admin/DeleteDelivery
        [HttpDelete]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteDelivery(int id)
        {
            var redirectUrl = Url.Action("DeliveryManagement", "Admin");

            if (ModelState.IsValid)
            {
                await _deliveryService.Delete(id);

                TempData["ToastMessage"] = "Xóa phương thức vận chuyển thành công.";
                TempData["ToastType"] = Constants.Success;
                return Json(new { redirectToUrl = redirectUrl, status = Constants.Success });
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return Json(new { redirectToUrl = redirectUrl, status = Constants.Error });
        }
        #endregion

        #region Review Management
        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> ReviewManagement(int? pageIndex)
        {
            ViewBag.ToastType = Constants.None;
            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            var pagingResult = await _bookService.GetPagingReviewBook(ApproveStatus.None, pageIndex);

            return View(pagingResult);
        }

        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> ApproveReview(int id)
        {
            if (ModelState.IsValid)
            {
                var review = await _reviewService.GetEntityById(id);

                if(review != null)
                {
                    review.Status = ApproveStatus.Approve;
                    await _reviewService.Update(review);

                    TempData["ToastMessage"] = "Duyệt đánh giá thành công.";
                    TempData["ToastType"] = Constants.Success;
                    return RedirectToAction("ReviewManagement");
                }
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return RedirectToAction("ReviewManagement");
        }

        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> RejectReview(int id)
        {
            var redirectUrl = Url.Action("ReviewManagement", "Admin");

            if (ModelState.IsValid)
            {
                var review = await _reviewService.GetEntityById(id);

                if (review != null)
                {
                    review.Status = ApproveStatus.Reject;
                    await _reviewService.Update(review);

                    TempData["ToastMessage"] = "Ẩn đánh giá thành công.";
                    TempData["ToastType"] = Constants.Success;
                    return RedirectToAction("ReviewManagement");
                }
            }

            TempData["ToastMessage"] = "Có lỗi xảy ra trong quá trình xử lý.";
            TempData["ToastType"] = Constants.Error;
            return RedirectToAction("ReviewManagement");
        }

        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> ReviewApprove(int? pageIndex)
        {
            var pagingResult = await _bookService.GetPagingReviewBook(ApproveStatus.Approve, pageIndex);

            return View(pagingResult);
        }

        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> ReviewReject(int? pageIndex)
        {
            var pagingResult = await _bookService.GetPagingReviewBook(ApproveStatus.Reject, pageIndex);

            return View(pagingResult);
        }
        #endregion
    }
}
