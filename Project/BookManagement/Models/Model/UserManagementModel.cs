using BookManagement.Models.Entity;

namespace BookManagement.Models.Model
{
    public class UserManagementModel : User
    {
        public int TotalOrder { get; set; }
        public int TotalMoney { get; set; }
    }

    public class UserPagingModel
    {
        public string? Keyword { get; set; }
        public PagingModel<UserManagementModel> Paging { get; set; }
    }
}
