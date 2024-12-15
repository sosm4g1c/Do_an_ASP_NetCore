using System.ComponentModel.DataAnnotations;

namespace BookManagement.Models.Model
{
    public class PasswordModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Mật khẩu không được để trống")]
        public string OldPassword { get; set; }
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
