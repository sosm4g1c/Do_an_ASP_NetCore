using BookManagement.Models.Entity;
using System.ComponentModel.DataAnnotations;

namespace BookManagement.Models.Model
{
    public class OrderViewModel : Order
    {
        public string UserName { get; set; }
        public string DeliveryName { get; set; }
        public string StatusName
        {
            get
            {
                switch (this.Status)
                {
                    case Constant.Enumerations.OrderStatus.Waiting:
                        return "Chờ lấy hàng";
                    case Constant.Enumerations.OrderStatus.Shipping:
                        return "Đang giao hàng";
                    case Constant.Enumerations.OrderStatus.Complete:
                        return "Đơn hàng hoàn thành";
                    case Constant.Enumerations.OrderStatus.Cancel:
                        return "Đơn hàng đã hủy";
                    default:
                        return "";
                };
            }
            set { }
        }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
