using BookManagement.Constant;
using System.ComponentModel.DataAnnotations;
using static BookManagement.Constant.Enumerations;

namespace BookManagement.Models.Entity
{
    /// <summary>
    /// Người dùng
    /// </summary>
    public class User : BaseEntity
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Tên đăng nhập không được để trống")]
        public string UserName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Họ không được để trống")]
        public string FirstName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Tên không được để trống")]
        public string LastName { get; set; }
        [DataType(DataType.EmailAddress)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email không được để trống")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                            @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                            ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Address { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDay { get; set; }
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Số điện thoại không đúng định dạng")]
        public string? PhoneNumber { get; set; }
        public GenderEnum? Gender { get; set; }
        public string? Infomation { get; set; }
        public bool IsDelete { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public RoleEnum RoleType { get; set; } = RoleEnum.User;
        public string? RoleName
        {
            get
            {
                switch (RoleType)
                {
                    case RoleEnum.User:
                        return "Người dùng";
                    case RoleEnum.Admin:
                        return "Admin";
                    case RoleEnum.Staff:
                        return "Nhân viên";
                    default:
                        return "";
                }
            }
            set { }
        }
    }
}
