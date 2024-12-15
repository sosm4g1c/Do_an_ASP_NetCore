using static BookManagement.Constant.Enumerations;

namespace BookManagement.Models.Entity
{
    /// <summary>
    /// Đơn hàng
    /// </summary>
    public class Order : BaseEntity
    {
        /// <summary>
        /// ID người mua
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// Hình thức vận chuyển
        /// </summary>
        public int DeliveryId { get; set; }
        /// <summary>
        /// Phí vận chuyển
        /// </summary>
        public int ShipCost { get; set; }
        /// <summary>
        /// Mã giảm giá (nếu có)
        /// </summary>
        public int? VoucherId { get; set; }
        /// <summary>
        /// Khuyến mại
        /// </summary>
        public int Discount { get; set; }
        /// <summary>
        /// Tổng tiền
        /// </summary>
        public int TotalMoney { get; set; }
        /// <summary>
        /// Trạng thái
        /// </summary>
        public OrderStatus Status { get; set; }
        /// <summary>
        /// Tên người nhận
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// SĐT người nhận
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Địa chỉ nhận hàng
        /// </summary>
        public string CustomerAddress { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? OrderNote { get; set; }
        /// <summary>
        /// Hình thức thanh toán
        /// </summary>
        public PaymentType PaymentType { get; set; }
        public string PaymentName
        {
            get
            {
                switch (PaymentType)
                {
                    case PaymentType.Cod:
                        return "Thanh toán khi nhận hàng";
                    case PaymentType.Online:
                        return "Thanh toán Online";
                    default:
                        return "";
                }
            }
            set { }
        }
        /// <summary>
        /// Lý do hủy đơn hàng
        /// </summary>
        public string? CancelReason { get; set; }
    }
}
