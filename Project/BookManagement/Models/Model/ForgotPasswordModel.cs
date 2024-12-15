using System.ComponentModel.DataAnnotations;

namespace BookManagement.Models.Model
{
    public class EmailModel
    {
        [DataType(DataType.EmailAddress)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email không được để trống")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                            @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                            ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }
    }
    public class ConfirmOtpModel
    {
        public string Key { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public int OTP { get; set; }
    }


    public class ConfirmOtpBindingModel
    {
        public string Key { get; set; }
        public int OTP { get; set; }
    }

    public class ConfirmPasswordModel
    {
        public string Key { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Không tìm thấy tài khoản!")]
        public int? UserId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu chứa tối thiểu 8 ký tự")]
        [RegularExpression(@"^(?=.*[A-z])(?=.*[0-9])(?=.*?[!@#$%\^&*\(\)\-_+=;:'""\/\[\]{},.<>|`]).{8,32}$",
                            ErrorMessage = "Mật khẩu chứa tối thiểu 1 chữ số và 1 ký tự đặc biệt")]
        public string NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage = "Mật khẩu không khớp")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Mật khẩu không được để trống")]
        public string ConfirmPassword { get; set; }
    }
}
