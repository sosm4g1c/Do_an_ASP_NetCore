using BookManagement.Models.Entity;
using System.ComponentModel.DataAnnotations;
using static BookManagement.Constant.Enumerations;

namespace BookManagement.Models.Model
{
    public class CartConfirmModel
    {
        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// Hình thức vận chuyển
        /// </summary>
        public int DeliveryId { get; set; }
        public int? VoucherId{ get; set; }
        public int ShipCost { get; set; }
        public int Discount { get; set; }
        public int TotalMoney { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Người nhận không được để trống")]
        public string CustomerName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Số điện thoại không được để trống")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Số điện thoại không đúng")]
        public string PhoneNumber { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Địa chỉ không được để trống")]
        public string CustomerAddress { get; set; }
        public string? OrderNote { get; set; }
        /// <summary>
        /// Hình thức thanh toán
        /// </summary>
        public PaymentType PaymentType { get; set; }
    }
}
