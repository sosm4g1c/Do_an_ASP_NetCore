using BookManagement.Models.Entity;

namespace BookManagement.Data
{
    public interface IUserConfig
    {
        User GetUserConfig();
        int GetUserId();
    }
}
