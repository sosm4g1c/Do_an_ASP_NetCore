namespace BookManagement.Models.Entity
{
    /// <summary>
    /// Chi tiết sản phẩm trong Đơn hàng
    /// </summary>
    public class OrderDetail : BaseEntity
    {
        /// <summary>
        /// ID người mua
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// ID đơn hàng
        /// </summary>
        public int OrderId { get; set; }
        /// <summary>
        /// ID sách
        /// </summary>
        public int BookId { get; set; }
        /// <summary>
        /// Tên sách
        /// </summary>
        public string BookName { get; set; }
        /// <summary>
        /// Số lượng mua
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Tổng giá tiền
        /// </summary>
        public int PriceBuy { get; set; }
        /// <summary>
        /// Ảnh bìa sách
        /// </summary>
        public string BookImage { get; set; }
    }
}
