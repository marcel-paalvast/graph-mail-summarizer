using Microsoft.Extensions.Options;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mail_summarizer_api.Settings;
using System.Net.Http;
using OpenAI.GPT3.ObjectModels.RequestModels;

namespace mail_summarizer_api.Services;
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
        var response = await _client.CreateCompletion(
            new ChatCompletionCreateRequest()
            {
                Messages = new List<ChatMessage>()
                {
                    ChatMessage.FromUser($"Summarize in <128 chars"),
                    ChatMessage.FromUser(message),
                },
                Model = OpenAI.GPT3.ObjectModels.Models.ChatGpt3_5Turbo,
                MaxTokens = 250,
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
