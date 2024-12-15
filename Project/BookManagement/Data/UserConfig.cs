using BookManagement.Models.Entity;
using Newtonsoft.Json;
using System.Security.Claims;

namespace BookManagement.Data
{
    public class UserConfig : IUserConfig
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserConfig(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public User GetUserConfig()
        {
            var userConfig = new User();
            var userConfigStr = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.UserData)?.Value;
            if (!string.IsNullOrEmpty(userConfigStr))
            {
                return JsonConvert.DeserializeObject<User>(userConfigStr);
            }
            return userConfig;
        }
        public int GetUserId()
        {
            var userConfig = new User();
            var userConfigStr = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.UserData)?.Value;
            if (!string.IsNullOrEmpty(userConfigStr))
            {
                userConfig = JsonConvert.DeserializeObject<User>(userConfigStr);
            }

            return userConfig?.Id ?? 0;
        }
    }
}
