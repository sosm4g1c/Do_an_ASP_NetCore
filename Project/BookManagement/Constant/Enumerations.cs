namespace BookManagement.Constant
{
    public class Enumerations
    {
        public enum ToastType : int
        {
            None = 0,
            Success = 1,
            Error = 2,
            Warning = 3,
        }

        public enum OrderStatus : int
        {
            Waiting = 1,
            Shipping = 2,
            Complete = 3,
            Cancel = 4
        }

        public enum EditMode : int
        {
            Add = 1,
            Edit = 2,
            Delete = 3,
        }

        public enum SortType : int
        {
            New = 1,
            Sell = 2,
            Cheap = 3,
            Expensive = 4,
        }

        public enum ReasonCancel : int
        {
            ChangeInfo = 1, // Thay đổi thông tin giao hàng
            NotBuy = 2, // Đổi ý, không muốn mua nữa
            WrongOrder = 3, // Đặt nhầm sản phẩm 
            NotVoucher = 4, // Chưa áp mã giảm giá
            Other = 5, // Lý do khác
        }

        public enum ReasonCancelShop : int
        {
            SoldOut = 1, // Hết hàng
            NoContact = 2, // Không liên hệ được khách hàng
            Other = 3, // Lý do khác
        }

        public enum DashboardViewType : int
        {
            Week = 1, // Tuần
            Month = 2, // Tháng
            Quarter = 3, // Quý
        }

        public enum GenderEnum : int
        {
            None = 0, // Chọn giới tính
            Male = 1, // Nam
            Female = 2, // Nữ
            Other = 3, // Khác
        }
        public enum PaymentType : int
        {
            Cod = 1, // Thanh toán khi nhận hàng
            Online = 2, // Thanh toán trực tuyến
        }
        public enum ApproveStatus : int
        {
            None = 0, // Chưa xử lý
            Approve = 1, // Duyệt
            Reject = 2, // Không duyệt
        }
    }
}
