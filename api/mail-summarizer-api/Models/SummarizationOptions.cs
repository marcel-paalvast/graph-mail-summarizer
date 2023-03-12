using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Models;
public class TokenExtensions<T>
{
    public required Token Token { get; set; }
    public required T Value { get; set; }
}
