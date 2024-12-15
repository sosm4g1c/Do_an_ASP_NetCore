using AutoMapper;
using BookManagement.Constant;
using BookManagement.Data;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using BookManagement.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Security.Claims;
using static BookManagement.Constant.Enumerations;

namespace BookManagement.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IBaseService<User> _userService;
        private readonly IBaseService<Cart> _cartService;
        private readonly IConfiguration _configuration;
        private readonly IUserConfig _userConfig;
        private IMemoryCache _cache;
        private IMailService _mailService;

        public AccountController(
            IMapper mapper,
            IAuthService authService,
            IBaseService<User> userService,
            IBaseService<Cart> cartService,
            IUserConfig userConfig,
            IConfiguration configuration,
            IMailService mailService,
            IMemoryCache memoryCache)
        {
            _mapper = mapper;
            _userConfig = userConfig;
            _authService = authService;
            _userService = userService;
            _cartService = cartService;
            _configuration = configuration;
            _cache = memoryCache;
            _mailService = mailService;
        }

        // Get: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.ToastType = Constants.None;

            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            return View();
        }

        // Get: /Account/LoginGoogle
        [HttpGet]
        public async Task LoginGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("SigninGoogle")
                });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserModel model, string? returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _authService.AuthenticationUser(model);
                if (user != null)
                {
                    if (user.IsDelete)
                    {
                        ViewBag.ToastMessage = "Tài khoản không khả dụng hoặc đã bị xóa. Vui lòng kiểm tra lại.";
                        ViewBag.ToastType = Constants.Error;

                        return View(model);
                    }
                    else if (!user.IsActive)
                    {
                        ViewBag.ToastMessage = "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên để được hỗ trợ.";
                        ViewBag.ToastType = Constants.Error;

                        return View(model);
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(user)),
                        new Claim(ClaimTypes.Role, user.RoleType == RoleEnum.Admin ? Role.Admin :
                        (user.RoleType == RoleEnum.Staff ? Role.Staff : Role.User)),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = model.RememberMe,
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ViewBag.ToastMessage = "Tài khoản hoặc mật khẩu không chính xác.";
                    ViewBag.ToastType = Constants.Error;

                    return View(model);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> SigninGoogle()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var claimsData = result.Principal.Identities.FirstOrDefault().Claims.ToList();

            // Đăng nhập google thành công 
            var email = claimsData.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var user = await _userService.Get(x => x.Email.ToLower().Trim().Equals(email.ToLower().Trim()));

            // Nếu chưa có user thì insert user vào database
            if (user is null)
            {
                await _authService.InsertUser(new RegisterModel
                {
                    FirstName = claimsData.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value,
                    LastName = claimsData.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value,
                    Email = email,
                    UserName = email,
                    Password = _configuration.GetSection("Password:Default").Value
                });

                user = await _userService.Get(x => x.UserName.ToLower().Trim().Equals(email.ToLower().Trim()));
            }

            if (user.IsDelete)
            {
                TempData["ToastMessage"] = "Tài khoản không khả dụng hoặc đã bị xóa. Vui lòng kiểm tra lại.";
                TempData["ToastType"] = Constants.Error;

                return RedirectToAction("Login");
            }
            else if (!user.IsActive)
            {
                TempData["ToastMessage"] = "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên để được hỗ trợ.";
                TempData["ToastType"] = Constants.Error;

                return RedirectToAction("Login");
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(user)),
                    new Claim(ClaimTypes.Role, user.RoleType == RoleEnum.Admin ? Role.Admin :
                        (user.RoleType == RoleEnum.Staff ? Role.Staff : Role.User)),
                };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // Get: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var userExist = await _userService.Exist(x => x.UserName.ToLower().Trim().Equals(model.UserName.ToLower().Trim()));
                var emailExist = await _userService.Exist(x => x.Email.ToLower().Trim().Equals(model.Email.ToLower().Trim()));
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
                    await _authService.InsertUser(model);

                    TempData["ToastMessage"] = "Đăng ký tài khoản thành công.";
                    TempData["ToastType"] = Constants.Success;

                    return RedirectToAction("Login");
                }
            }
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
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

            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> ChangePassword(PasswordModel model)
        {
            var userId = _userConfig.GetUserId();
            // Check pass nhập vào xem có đúng không
            var user = await _userService.GetEntityById(userId);
            var validPass = await _authService.ValidateHashPassword(model.OldPassword, user.Password);

            if (!validPass)
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu hiện tại không chính xác");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                user.Password = await _authService.HashPassword(model.NewPassword);
                await _userService.Update(user);

                TempData["ToastMessage"] = "Cập nhật mật khẩu thành công.";
                TempData["ToastType"] = Constants.Success;

                return RedirectToAction("ChangePassword");
            }
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Infomation()
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

            var user = await _userService.GetEntityById(userId);

            var userModel = _mapper.Map<UserInfomationModel>(user);

            // Set vào ViewBag
            ViewBag.GenderList = new SelectList(new List<ItemDropdownModel>()
            {
                new ItemDropdownModel(){ Value = 0, Name = "Chọn giới tính" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Male, Name = "Nam" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Female, Name = "Nữ" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Other, Name = "Khác" },
            }, "Value", "Name");

            return View(userModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Infomation(UserInfomationModel model)
        {
            // Set vào ViewBag
            ViewBag.GenderList = new SelectList(new List<ItemDropdownModel>()
            {
                new ItemDropdownModel(){ Value = 0, Name = "Chọn giới tính" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Male, Name = "Nam" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Female, Name = "Nữ" },
                new ItemDropdownModel(){ Value = (int)GenderEnum.Other, Name = "Khác" },
            }, "Value", "Name");

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
                    var user = await _userService.GetEntityById(model.Id);

                    var userModel = _mapper.Map(model, user);

                    await _userService.Update(userModel);

                    TempData["ToastMessage"] = "Cập nhật thông tin tài khoản thành công.";
                    TempData["ToastType"] = Constants.Success;
                    return RedirectToAction("Infomation");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ViewBag.ToastType = Constants.None;

            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(EmailModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.Get(x => x.Email.ToLower().Trim().Equals(model.Email.ToLower().Trim()));

                if (user != null)
                {
                    if (user.IsDelete)
                    {
                        ModelState.AddModelError("Email", "Email đăng ký cho tài khoản không khả dụng/đã bị xóa");
                        return View(model);
                    }
                    else if (!user.IsActive)
                    {
                        ModelState.AddModelError("Email", "Email đăng ký cho tài khoản đã bị khóa");
                        return View(model);
                    }
                    else
                    {
                        var otp = new Random().Next(100000, 999999);
                        string key = Guid.NewGuid().ToString();

                        // Save cache
                        var cache = new ConfirmOtpModel()
                        {
                            Key = key,
                            UserId = user.Id,
                            Email = user.Email,
                            OTP = otp
                        };
                        _cache.Set<ConfirmOtpModel>(key, cache, new TimeSpan(0, 10, 0));

                        //Xử lý gửi mail
                        var isSend = _mailService.SendMailResetPassword(user.Email, otp);

                        if (isSend)
                        {
                            return RedirectToAction("ConfirmOTP", new { key = key });
                        }
                        else
                        {
                            TempData["ToastMessage"] = "Hệ thống mail gặp sự cố, vui lòng thử lại sau";
                            TempData["ToastType"] = Constants.Error;
                            return RedirectToAction("ForgotPassword");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("Email", "Email chưa được đăng ký");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ConfirmOTP(string key)
        {
            var cacheModel = new ConfirmOtpModel();

            if (string.IsNullOrEmpty(key))
            {
                return RedirectToAction("ForgotPassword");
            }
            else if (!_cache.TryGetValue<ConfirmOtpModel>(key, out cacheModel))
            {
                TempData["ToastMessage"] = "Mã OTP đã hết hiệu lực, vui lòng thử lại";
                TempData["ToastType"] = Constants.Error;
                return RedirectToAction("ForgotPassword");
            }

            _cache.TryGetValue<ConfirmOtpModel>(key, out cacheModel);

            var model = new ConfirmOtpBindingModel()
            {
                Key = key,
            };

            ViewBag.ToastType = Constants.None;

            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult ConfirmOTP(ConfirmOtpBindingModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.OTP > 999999 || model.OTP < 100000)
                {
                    ModelState.AddModelError("OTP", "Nhập mã OTP gồm 6 chữ số");
                    return View(model);
                }
                else
                {
                    var cacheModel = new ConfirmOtpModel();

                    if (string.IsNullOrEmpty(model.Key) || !_cache.TryGetValue<ConfirmOtpModel>(model.Key, out cacheModel))
                    {
                        TempData["ToastMessage"] = "Mã OTP đã hết hiệu lực, vui lòng thử lại";
                        TempData["ToastType"] = Constants.Error;
                        return RedirectToAction("ForgotPassword");
                    }

                    _cache.TryGetValue<ConfirmOtpModel>(model.Key, out cacheModel);

                    if (cacheModel.OTP != model.OTP)
                    {
                        ModelState.AddModelError("OTP", "Mã OTP không chính xác");
                        return View(model);
                    }
                    else
                    {
                        return RedirectToAction("ResetPassword", new { key = model.Key });
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResendOTP(string key)
        {
            var cacheModel = new ConfirmOtpModel();
            _cache.TryGetValue<ConfirmOtpModel>(key, out cacheModel);
            var otp = new Random().Next(100000, 999999);

            // Save cache
            var cache = new ConfirmOtpModel()
            {
                Key = key,
                UserId = cacheModel.UserId,
                Email = cacheModel.Email,
                OTP = otp
            };

            _cache.Remove(key);
            _cache.Set<ConfirmOtpModel>(key, cache, new TimeSpan(0, 10, 0));

            //Xử lý gửi mail
            var isSend = _mailService.SendMailResetPassword(cacheModel.Email, otp);

            if (isSend)
            {
                TempData["ToastMessage"] = "Đã gửi lại mã OTP, vui lòng kiểm tra hòm thư.";
                TempData["ToastType"] = Constants.Success;
                return RedirectToAction("ConfirmOTP", new { key = key });
            }
            else
            {
                TempData["ToastMessage"] = "Hệ thống mail gặp sự cố, vui lòng thử lại sau";
                TempData["ToastType"] = Constants.Error;
                return RedirectToAction("ConfirmOTP");
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string key)
        {
            var cacheModel = new ConfirmOtpModel();

            if (string.IsNullOrEmpty(key))
            {
                return RedirectToAction("ForgotPassword");
            }
            else if (!_cache.TryGetValue<ConfirmOtpModel>(key, out cacheModel))
            {
                TempData["ToastMessage"] = "Mã OTP đã hết hiệu lực, vui lòng thử lại";
                TempData["ToastType"] = Constants.Error;
                return RedirectToAction("ForgotPassword");
            }

            _cache.TryGetValue<ConfirmOtpModel>(key, out cacheModel);

            var model = new ConfirmPasswordModel()
            {
                Key = key,
                UserId = cacheModel.UserId
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ConfirmPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetEntityById(model.UserId ?? 0);

                if (user != null)
                {
                    user.Password = await _authService.HashPassword(model.NewPassword);
                    await _userService.Update(user);

                    // remove cache
                    _cache.Remove(model.Key);

                    TempData["ToastMessage"] = "Cập nhật mật khẩu thành công.";
                    TempData["ToastType"] = Constants.Success;

                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("UserId", "Không tìm thấy tài khoản!");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
