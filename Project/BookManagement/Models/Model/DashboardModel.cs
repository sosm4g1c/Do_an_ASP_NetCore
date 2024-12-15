namespace BookManagement.Models.Model
{
    public class DashboardModel
    {
        public int TotalBook { get; set; }
        public int TotalCategory { get; set; }
        public int TotalVoucher { get; set; }
        public int OrderWaiting { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public OrderOverview TotalOrder { get; set; } = new OrderOverview();
        public OrderOverview TotalMoney { get; set; } = new OrderOverview();
        public List<BookBestSeller> BestSeller { get; set; } = new List<BookBestSeller>();
        public int ViewType { get; set; }
    }

    public class OrderOverview
    {
        public int Waiting { get; set; }
        public int Delivery { get; set; }
        public int Complete { get; set; }
        public int Cancel { get; set; }
    }

    public class BookBestSeller
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public int TotalSold { get; set; }
    }
}
