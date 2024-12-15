using System.ComponentModel.DataAnnotations;

namespace BookManagement.Models.Entity
{
    public class News : BaseEntity
    {
        /// <summary>
        /// Ảnh bìa tin tức
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn ảnh bìa tin tức")]
        public string NewsImage { get; set; }
        /// <summary>
        /// Tiêu đề
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(255, ErrorMessage = "Tiêu đề chứa tối đa 255 ký tự")]
        public string Title { get; set; }
        /// <summary>
        /// Tóm tắt
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Tóm tắt không được để trống")]
        [MaxLength(255, ErrorMessage = "Tóm tắt chứa tối đa 255 ký tự")]
        public string Summary { get; set; }
        /// <summary>
        /// Nội dung
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }
    }
}
