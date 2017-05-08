using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IISCI.Web.Controllers;
using MailKit.Net.Smtp;
using System.IO;
using MimeKit;

namespace IISCI.Web.Services
{
    public class SmtpService
    {

        public static SmtpService Instance = new SmtpService();

        internal void Send(SettingsModel settings, string subject, string body, List<string> recipients)
        {
            using (var ms = new MemoryStream())
            {
                try
                {
                    using (var logger = new MailKit.ProtocolLogger(ms))
                    {
                        using (SmtpClient client = new SmtpClient(logger))
                        {
                            int port = settings.SMTPPort == 0 ? (settings.SSL ? 465 : 25) : settings.SMTPPort;
                            client.Connect(settings.SMTPHost, port, settings.SSL);
                            client.Authenticate(settings.Username, settings.Password);

                            MimeMessage msg = new MimeMessage();

                            msg.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail ?? settings.Username));

                            foreach (var r in recipients) {
                                MailboxAddress er = null;
                                if (MailboxAddress.TryParse(r, out er)) {
                                    msg.To.Add(er);
                                }
                            }

                            msg.Subject = subject;
                            BodyBuilder bb = new BodyBuilder()
                            {
                                HtmlBody = body
                            };
                            msg.Body = bb.ToMessageBody();


                            client.Send(msg);
                        }
                    }
                }
                catch (Exception ex) {
                    string error = ex.ToString() + "\r\n";
                    error += System.Text.Encoding.UTF8.GetString(ms.ToArray());

                    throw new InvalidOperationException(error, ex);
                }
            }
        }
    }
}