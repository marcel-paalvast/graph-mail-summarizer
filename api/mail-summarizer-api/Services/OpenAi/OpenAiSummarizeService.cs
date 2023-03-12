using Microsoft.Extensions.Options;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mail_summarizer_api.Settings;
using OpenAI.GPT3.ObjectModels.RequestModels;

namespace mail_summarizer_api.Services.OpenAi;
internal class OpenAiSummarizeService : ISummarizeService
{
    private readonly OpenAIService _client;

    public OpenAiSummarizeService(HttpClient httpClient, IOptions<OpenAiSettings> options)
    {
        _client = new OpenAIService(new OpenAiOptions()
        {
            ApiKey = options.Value.ApiKey,
        }, httpClient);
    }

    public async Task<string> SummarizeAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "";
        }

        if (message.Length > 10_000)
        {
            message = message[..10_000];
        }

        var response = await _client.CreateCompletion(
            new ChatCompletionCreateRequest()
            {
                Messages = new List<ChatMessage>()
                {
                    ChatMessage.FromUser("Summarize in <128 chars"),
                    ChatMessage.FromUser(message),
                },
                Model = OpenAI.GPT3.ObjectModels.Models.ChatGpt3_5Turbo,
                MaxTokens = 256,
                Temperature = 0.1f,
                FrequencyPenalty = 1.5f,
            });

        if (response.Successful)
        {
            return response.Choices.First().Message.Content;
        }
        else
        {
            throw new Exception(response.Error?.Message);
        }
    }

    public async Task<string> SummarizeAsync(IEnumerable<string> messages)
    {
        var chat = new List<ChatMessage>()
        {
            ChatMessage.FromUser("Make a single summary of the following messages and filter out information that isn't priority in <512 chars without bullet points:")
        };

        var length = 0;
        foreach (var message in messages)
        {
            length += message.Length;

            if (length <= 10_000)
            {
                chat.Add(ChatMessage.FromUser(message));
            }
            else
            {
                chat.Add(ChatMessage.FromUser(message[..^(length - 3000)]));
                break;
            }

        }

        var response = await _client.CreateCompletion(
        new ChatCompletionCreateRequest()
        {
            Messages = chat,
            Model = OpenAI.GPT3.ObjectModels.Models.ChatGpt3_5Turbo,
            MaxTokens = 512,
            Temperature = 0.1f,
            FrequencyPenalty = 1.5f,
        });

        if (response.Successful)
        {
            return response.Choices.First().Message.Content;
        }
        else
        {
            throw new Exception(response.Error?.Message);
        }
    }
}
