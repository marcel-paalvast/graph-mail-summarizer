using mail_summarizer_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Services;
/// <summary>
/// Allows for mail actions.
/// </summary>
public interface IMailService
{
    /// <summary>
    /// Retrieves a set of emails.
    /// </summary>
    /// <param name="options">Allows configuration on what mails to retrieve</param>
    /// <returns></returns>
    Task<IList<Mail>> GetMailAsync(GetMailOptions options);

    /// <summary>
    /// Sends an email.
    /// </summary>
    /// <param name="mail"></param>
    /// <returns></returns>
    Task SendMailAsync(SendMail mail);
}
