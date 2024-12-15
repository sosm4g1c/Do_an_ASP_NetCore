using BookManagement.Models.Entity;

namespace BookManagement.Models.Model
{
    public class CartModel
    {
        public List<CartItemModel> CartItems { get; set; } = new List<CartItemModel>();
        public int? VoucherId{ get; set; }
        public string? VoucherCode { get; set; }
        /// <summary>
        /// Hình thức vận chuyển
        /// </summary>
        public int? DeliveryId { get; set; }
        public int ShipCost { get; set; }
        public int Discount { get; set; }
        public int GrossMoney
        {
            get
            {
                return CartItems.Sum(x => x.TotalMoney);
            }
            set { }
        }
        public int TotalMoney
        {
            get
            {
                var money = GrossMoney + ShipCost - Discount;
                return money < 0 ? 0 : money;
            }
            set { }
        }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string CustomerAddress { get; set; }
        public string OrderNote { get; set; }
    }

    public class CartItemModel : Cart
    {
        public string BookName { get; set; }
        public string BookImage { get; set; }
        public int Price { get; set; }
        public int? PriceOriginal { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; }
        public int TotalMoney
        {
            get
            {
                return Quantity * Price;
            }
            set { }
        }
        public string? ErrorMessage { get; set; }
    }
}
