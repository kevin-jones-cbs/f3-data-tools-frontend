using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace F3Lambda
{
    public static class EmailPeacock
    {
        public static void Send(string subject, string body)
        {
            var me = new MailAddress("kjmp32@gmail.com", "Kevin Jones");
            var fromAddress = me;
            var toAddress = me;

            // set password in environment variable
            string fromPassword = Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD");

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
