﻿using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using SemanticKernelDemo.PluginsAndFunctions;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelDemo.Common;

var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? string.Empty;
var key = Environment.GetEnvironmentVariable("OPENAI_KEY") ?? string.Empty;

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion("gpt-4o", endpoint, key);
builder.Plugins.AddFromType<GeneralPlugin>();
var kernel = builder.Build();

var executionSettings = new OpenAIPromptExecutionSettings
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

ChatHistory history = [];

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("User > ");
    Console.ForegroundColor = ConsoleColor.White;
    var request = Console.ReadLine();
    history.AddUserMessage(request!);

    var result = chatCompletionService.GetStreamingChatMessageContentsAsync(history, executionSettings, kernel);

    string fullMessage = "";
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Assistant > ");
    await foreach (var content in result)
    {
        Console.Write(content.Content);
        fullMessage += content.Content;
    }
    Console.WriteLine();

    history.AddAssistantMessage(fullMessage);
    history.Debug();
}