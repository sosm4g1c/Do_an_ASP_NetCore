using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BookManagement.Models.Model;
using System.Net;
using System.Net.Mail;


namespace BookManagement.Service
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool SendMail(SendMailModel email)
        {
            var emailMessage = new MailMessage();
            var from = string.IsNullOrEmpty(email.From) ? _configuration["MailSettings:UserName"] : email.From;

            emailMessage.From = new MailAddress(from);
            emailMessage.To.Add(email.To);

            if (email.Cc != null && email.Cc.Count > 0)
            {
                foreach (var cc in email.Cc)
                {
                    emailMessage.To.Add(cc);
                }
            }

            emailMessage.Subject = email.Subject;
            emailMessage.Body = email.EmailContent;
            emailMessage.IsBodyHtml = true;

            var port = 0;
            int.TryParse(_configuration["MailSettings:Port"], out port);

            using (SmtpClient smtp = new SmtpClient(_configuration["MailSettings:Server"], port))
            {
                smtp.Credentials = new NetworkCredential(_configuration["MailSettings:UserName"], _configuration["MailSettings:Password"]);
                smtp.EnableSsl = true;
                smtp.Send(emailMessage);
            }

            return true;
        }

        public bool SendMailResetPassword(string email, int otp)
        {
            string contentEmail = string.Format(@"
                <table style=""vertical-align:top"" role=""presentation"" border=""0"" width=""100%"" cellspacing=""0"" cellpadding=""0"">
                    <tbody>
                        <tr>
                            <td style=""font-size:0px;padding:5px 5px 10px 5px;word-break:break-word"" align=""left"">
                                <div style=""font-family:BinancePlex,Arial,PingFangSC-Regular,'Microsoft YaHei',sans-serif;font-size:20px;font-weight:900;line-height:25px;text-align:left;color:#000000"">[BookStore] Mã xác minh</div>
                            </td>
                        </tr>
                        <tr>
                            <td style=""font-size:0px;padding:5px 5px 5px 5px;word-break:break-word"" align=""left"">
                                <div style=""font-family:BinancePlex,Arial,PingFangSC-Regular,'Microsoft YaHei',sans-serif;font-size:14px;line-height:20px;text-align:left;color:#000000"">Mã xác minh của bạn:</div>
                            </td>
                        </tr>
                        <tr>
                            <td style=""background:#ffffff;font-size:0px;padding:5px 5px 5px 5px;word-break:break-word"" align=""left"">
                                <div style=""font-family:BinancePlex,Arial,PingFangSC-Regular,'Microsoft YaHei',sans-serif;font-size:18px;line-height:30px;text-align:left;color:#f0b90b"">
                                    <div><span><strong>{0}</strong></span></div>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td style=""font-size:0px;padding:5px 5px 5px 5px;word-break:break-word"" align=""left"">
                                <div style=""font-family:BinancePlex,Arial,PingFangSC-Regular,'Microsoft YaHei',sans-serif;font-size:14px;line-height:20px;text-align:left;color:#000000"">
                                    <span>Mã xác minh sẽ có hiệu lực trong 10 phút. Vui lòng không chia sẻ mã này với người khác.</span>
                                    <div>&nbsp;</div>
                                    <div><em>Đây là tin nhắn tự động, vui lòng không trả lời. </em></div>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            ", otp);

            var model = new SendMailModel()
            {
                To = email,
                EmailContent = contentEmail,
                Subject = "Xác minh khôi phục mật khẩu BookStore"
            };

            return SendMail(model);
        }
    }
}
