namespace BookManagement.Models.Entity
{
    /// <summary>
    /// Mã khuyến mại
    /// </summary>
    public class Voucher : BaseEntity
    {
        /// <summary>
        /// Mã giảm giá
        /// </summary>
        public string VoucherCode { get; set; }
        /// <summary>
        /// Tên mã giảm giá
        /// </summary>
        public string VoucherName { get; set; }
        /// <summary>
        /// Số lượng mã
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Số lượng mã đã dùng
        /// </summary>
        public int UsedNumber { get; set; }
        /// <summary>
        /// Giảm giá bao nhiêu
        /// </summary>
        public int Discount { get; set; }
        /// <summary>
        /// Đơn tối thiểu bao nhiêu
        /// </summary>
        public int MinAmount { get; set; }
        /// <summary>
        /// Trạng thái
        /// </summary>
        public bool IsActive { get; set; }
    }
}
