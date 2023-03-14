using mail_summarizer_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Services;
/// <summary>
/// Used for creating a html template for emails.
/// </summary>
public interface IMailGeneratorService
{
    /// <summary>
    /// Creates the html body of an email./>
    /// </summary>
    /// <param name="summaries">The contents of the mail.</param>
    /// <returns></returns>
    public string Create(MailSummaries summaries);
}
