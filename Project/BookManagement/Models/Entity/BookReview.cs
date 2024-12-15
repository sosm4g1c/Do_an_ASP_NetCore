using static BookManagement.Constant.Enumerations;

namespace BookManagement.Models.Entity
{
    /// <summary>
    /// Đánh giá sản phẩm
    /// </summary>
    public class BookReview : BaseEntity
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public ApproveStatus Status { get; set; }
    }
}
