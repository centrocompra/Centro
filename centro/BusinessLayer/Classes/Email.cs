using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace BusinessLayer.Classes
{
    public class Email
    {
        public static void SendEmail(string from, string[] to, string[] CC, string[] BCC, string subject, string body)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(from);

            foreach (string t in to)
            {
                message.To.Add(new MailAddress(t));
            }

            if (CC != null)
            {
                foreach (string c in CC)
                {
                    message.CC.Add(new MailAddress(c));
                }
            }

            if (BCC != null)
            {
                foreach (string b in BCC)
                {
                    message.Bcc.Add(new MailAddress(b));
                }
            }

            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();

            client.Send(message);
        }
    }
}
