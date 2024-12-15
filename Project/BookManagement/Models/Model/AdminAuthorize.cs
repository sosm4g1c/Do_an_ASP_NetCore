using BookManagement.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Security.Claims;

namespace BookManagement.Models.Model
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AdminAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check _userConfig
            var userConfigStr = context.HttpContext.User.FindFirst(ClaimTypes.UserData)?.Value;

            if (!string.IsNullOrEmpty(userConfigStr))
            {
                var userConfig = JsonConvert.DeserializeObject<User>(userConfigStr);
                if (userConfig != null)
                {
                    // Nếu có rồi thì check xem có phải admin không thì mới cho zô
                    if (userConfig.RoleType == Constant.RoleEnum.Admin || userConfig.RoleType == Constant.RoleEnum.Staff)
                    {
                        return;
                    }
                    else
                    {
                        context.Result = new RedirectToRouteResult(
                            new RouteValueDictionary(new
                            {
                                controller = "Account",
                                action = "AccessDenied",
                            })
                        );
                    }
                }
                else
                {
                    //Nếu chưa có thì đăng nhập
                    context.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new
                        {
                            controller = "Account",
                            action = "Login",
                        })
                    );
                }
            }
            else
            {
                //Nếu chưa có thì đăng nhập
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Account",
                        action = "Login",
                    })
                );
            }
        }
    }
}
