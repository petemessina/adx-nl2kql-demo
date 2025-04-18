using adx_nl2kql_demo.Agents;
using adx_nl2kql_demo.TerminationStrategies;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System;

public class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        IKernelBuilder kernelBuilder = BuildKernel(config);
        ChatHistory chatHistory = new();
        KustoQueryAgent kustoQueryAgent = new(kernelBuilder);
        KustoQueryValidationAgent kustoQueryValidationAgent = new(kernelBuilder);
        AgentGroupChat agentGroupChat = new(kustoQueryAgent, kustoQueryValidationAgent)
        {
            ExecutionSettings = new()
            {
                // Here a TerminationStrategy subclass is used that will terminate when
                // an assistant message contains the term "approve".
                TerminationStrategy =
                    new ApprovalTerminationStrategy()
                    {
                        // Only the kustoQueryValidationAgent may approve.
                        Agents = [kustoQueryValidationAgent],
                        // Limit total number of turns
                        MaximumIterations = 10,
                    }
            }
        };

        ChatMessageContent input = new(AuthorRole.User, "Where are all the sql servers that have a cpu percentage over 2?");
        agentGroupChat.AddChatMessage(input);

        await foreach (ChatMessageContent response in agentGroupChat.InvokeAsync())
        {
            Console.WriteLine(response);
        }

        Console.WriteLine("DONE");
        Console.ReadLine();
    }

    static IKernelBuilder BuildKernel(IConfigurationRoot config)
    {
        var builder = Kernel.CreateBuilder();

        builder.Services.AddAzureOpenAIChatCompletion(
            deploymentName: config["AzureOpenAIDeploymentName"],
            endpoint: config["AzureOpenAIEndpoint"],
            apiKey: config["AzureOpenAIAPIKey"] ?? ""
        );

        return builder;
    }
}