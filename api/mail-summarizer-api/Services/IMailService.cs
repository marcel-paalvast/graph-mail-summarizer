using mail_summarizer_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Services;
public interface IMailService
{
    Task GetMailAsync(GetMailOptions options);

    Task SendMailAsync(Mail mail);
}
