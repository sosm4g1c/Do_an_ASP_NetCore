namespace BookManagement.Models.Entity
{
    /// <summary>
    /// Hình thức vận chuyển
    /// </summary>
    public class Delivery : BaseEntity
    {
        /// <summary>
        /// Mã 
        /// </summary>
        public string DeliveryCode { get; set; }
        /// <summary>
        /// Tên hình thức vận chuyển
        /// </summary>
        public string DeliveryName { get; set; }
        /// <summary>
        /// Phí vận chuyển
        /// </summary>
        public int Cost { get; set; }
        /// <summary>
        /// Trạng thái
        /// </summary>
        public bool IsActive { get; set; }
    }
}
