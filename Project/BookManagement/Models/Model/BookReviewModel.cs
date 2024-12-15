using BookManagement.Models.Entity;
using System.ComponentModel.DataAnnotations;

namespace BookManagement.Models.Model
{
    public class BookReviewModel
    {
        public int BookId { get; set; }
        [Range(1, 5, ErrorMessage = "Vui lòng đánh giá sản phẩm")]
        public int Rating { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Vui lòng nhập đánh giá sản phẩm")]
        public string Comment { get; set; }
    }

    public class ApproveReviewModel : BookReview
    {
        /// <summary>
        /// Tên sách
        /// </summary>
        public string BookName { get; set; }
        /// <summary>
        /// Ảnh bìa sách
        /// </summary>
        public string BookImage { get; set; }
    }
}
