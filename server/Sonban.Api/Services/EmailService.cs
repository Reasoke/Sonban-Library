using Sonban.Api.Classes;
using System.Net;
using System.Net.Mail;

namespace Sonban.Api.Services {

  public interface IEmailService {

    Task SendMail(string mailTo, string subject, string message, bool htmlBody = true, string fromEmail = null, string fromName = null, string[] attachedFiles = null, string ccMailto = null);
  }

  public class EmailService : IEmailService {
    private readonly ILogger logger;

    private readonly string host;
    private readonly int port;
    private readonly bool enableSsl;
    private readonly string login;
    private readonly string password;
    private readonly string mailFromName;
    private readonly string mailFromEmail;

    public EmailService(ILogger logger, ISettingsProvider settingsProvider) {

      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

      this.logger = logger;

      host = settingsProvider.GetValue<string>("SmtpServer");
      port = settingsProvider.GetValue<int>("SmtpPort");
      enableSsl = settingsProvider.GetValue<bool>("SmtpEnableSsl");
      login = settingsProvider.GetValue<string>("SmtpLogin");
      password = settingsProvider.GetValue<string>("SmtpPassword");
      var from = settingsProvider.GetValue<string>("SmtpMailFrom");

      var splitted = from.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      mailFromEmail = splitted[0];
      if (splitted.Length > 1)
        mailFromName = splitted[1];
    }

    public async Task SendMail(string mailTo, string subject, string message, bool htmlBody = true, string fromEmail = null, string fromName = null, string[] attachedFiles = null, string ccMailto = null) {
      try {
        using (var mail = new MailMessage()) {

          mail.From = new MailAddress(mailFromEmail, "Sonban");
          var replyTo = string.IsNullOrWhiteSpace(fromEmail)
            ? new MailAddress(mailFromEmail, mailFromName)
            : string.IsNullOrWhiteSpace(fromName) ? new MailAddress(fromEmail) : new MailAddress(fromEmail, fromName);
          mail.ReplyToList.Add(replyTo);

          var mailToList = mailTo.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
          foreach (var mailToItem in mailToList)
            mail.To.Add(new MailAddress(mailToItem));

          if (!string.IsNullOrEmpty(ccMailto)) {
            foreach (var ccMailToItem in ccMailto.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
              mail.CC.Add(new MailAddress(ccMailToItem));
          }

          mail.Subject = subject;
          mail.Body = message;
          mail.IsBodyHtml = htmlBody;

          if (attachedFiles != null && attachedFiles.Length > 0) {
            foreach (var attachFile in attachedFiles) {
              if (!string.IsNullOrWhiteSpace(attachFile)) {
                //todo: check file exists?
                mail.Attachments.Add(new Attachment(attachFile));
              }
            }
          }

          using (var client = new SmtpClient {
              Host = host,
              Port = port,
              EnableSsl = enableSsl,
              Credentials = new NetworkCredential(login, password),
              DeliveryMethod = SmtpDeliveryMethod.Network
          }) {
            await client.SendMailAsync(mail).ConfigureAwait(false);
          }
        }
        logger.LogInformation($"'{subject}' sent to: {mailTo}");
      }
      catch (Exception ex) {
        logger.LogError(ex, $"Error sending '{subject}' to: {mailTo}");
        throw;
      }
    }
  }
}
