using AutoMapper;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;

namespace BookManagement.Service
{
    public class AuthService : IAuthService
    {
        private readonly IMapper _mapper;
        private readonly IBaseService<User> _userService;

        public AuthService(IMapper mapper, IBaseService<User> userService)
        {
            _mapper = mapper;
            _userService = userService;
        }

        /// <summary>
        /// Insert user mới đăng ký
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertUser(RegisterModel model)
        {
            var user = _mapper.Map<User>(model);

            user.Password = await HashPassword(model.Password);
            user.RoleType = Constant.RoleEnum.User;
            user.IsDelete = false;
            user.IsActive = true;

            await _userService.Insert(user);
        }

        public async Task<User> AuthenticationUser(UserModel model)
        {
            var user = await _userService.GetList(x => x.UserName.ToLower().Trim().Equals(model.UserName.ToLower().Trim()) || x.Email.ToLower().Trim().Equals(model.UserName.ToLower().Trim()));

            if (user != null && user.Any())
            {
                for (var i = 0; i < user.Count; i++)
                {
                    var thisUser = user.ElementAt(i);
                    if (await ValidateHashPassword(model.Password, thisUser.Password))
                    {
                        return thisUser;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Decode password
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<string> HashPassword(string value)
        {
            return Task.FromResult(BCrypt.Net.BCrypt.HashPassword(value, BCrypt.Net.BCrypt.GenerateSalt(12)));
        }

        /// <summary>
        /// Verify password
        /// </summary>
        /// <param name="value">Password người dùng nhập</param>
        /// <param name="hash">Password đã decode</param>
        /// <returns></returns>
        public Task<bool> ValidateHashPassword(string value, string hash)
        {
            return Task.FromResult(BCrypt.Net.BCrypt.Verify(value, hash));
        }

    }
}
