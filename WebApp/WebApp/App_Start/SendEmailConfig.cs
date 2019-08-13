using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace WebApp.App_Start
{
    public class SendEmailConfig
    {
        public static void Execute(string email, string emailSubject, string emailText)
        {
            try
            {
                var path = System.Web.Hosting.HostingEnvironment.MapPath("~/emailApiKey.txt");
                var apiKey = File.ReadAllText(path);
                string mailBodyhtml = emailText;
                var msg = new MailMessage("vinkoklocnaprevoz@example.com", email, emailSubject, mailBodyhtml);
                msg.IsBodyHtml = true;
                var smtpClient = new SmtpClient("smtp.sendgrid.net", 587);
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new NetworkCredential("apikey", apiKey);
                smtpClient.EnableSsl = true;
                smtpClient.Send(msg);
            }
            catch
            {
                return;
            }
        }
    }
}