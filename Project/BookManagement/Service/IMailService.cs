using BookManagement.Models.Model;

namespace BookManagement.Service
{
    public interface IMailService
    {
        bool SendMailResetPassword(string email, int otp);
    }
}
