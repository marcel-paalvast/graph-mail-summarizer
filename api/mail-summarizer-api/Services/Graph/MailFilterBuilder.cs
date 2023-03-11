using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Services.Graph;
/// <summary>
/// Builds a filter that can be used by <see cref="Microsoft.Graph.Me.MailFolders.Item.Messages.MessagesRequestBuilder.MessagesRequestBuilderGetRequestConfiguration"/>
/// to filter mails to receive based on criteria added by exposed methods of the instance.
/// </summary>
public class MailFilterBuilder
{
    readonly List<string> _filters = new();

    /// <summary>
    /// Filter messages based on the received date.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="dateOperator"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void AddDateFilter(DateTime dateTime, DateOperator dateOperator)
    {
        var filter = dateOperator switch
        {
            DateOperator.GreaterOrEqual => $"ReceivedDateTime ge {dateTime:yyyy-MM-ddTHH:mm:ssZ}",
            DateOperator.LessThan => $"ReceivedDateTime lt {dateTime:yyyy-MM-ddTHH:mm:ssZ}",
            _ => throw new NotImplementedException(),
        };
        _filters.Add(filter);
    }

    /// <summary>
    /// Filters out messages that are read.
    /// </summary>
    public void AddUnreadFilter()
    {
        _filters.Add("isRead eq false");
    }

    /// <summary>
    /// Filters out messages that are not received (send messages).
    /// </summary>
    public void AddReceivedFilter()
    {
        _filters.Add("singleValueExtendedProperties/Any(ep: ep/id eq 'String 0x0040' and ep/value ne null)");
    }

    /// <summary>
    /// Creates the odata filter that can be passed on the Graph Api.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"({string.Join(") and (", _filters)})";
    }
}

public enum DateOperator
{
    GreaterOrEqual,
    LessThan,
}