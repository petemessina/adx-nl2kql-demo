using adx_nl2kql_demo.Agents;
using adx_nl2kql_demo.TerminationStrategies;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Set up the configuration system using multiple sources in order of priority:
        // 1. User secrets (for sensitive data like API keys)
        // 2. appsettings.json file (optional)
        // 3. Environment variables
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Create a kernel builder with Azure OpenAI configuration
        IKernelBuilder kernelBuilder = BuildKernel(config);

        // Initialize a new chat history to track conversation
        ChatHistory chatHistory = new();

        // Create the specialized agents:
        // 1. KustoQueryAgent - Generates KQL queries from natural language
        // 2. KustoQueryValidationAgent - Validates syntax of generated queries
        KustoQueryAgent kustoQueryAgent = new(kernelBuilder);
        KustoQueryValidationAgent kustoQueryValidationAgent = new(kernelBuilder);

        // Set up a group chat with both agents to facilitate collaboration
        AgentGroupChat agentGroupChat = new(kustoQueryAgent, kustoQueryValidationAgent)
        {
            ExecutionSettings = new()
            {
                // Configure the termination strategy to end the conversation
                // when an "approve" message is received
                TerminationStrategy =
                    new ApprovalTerminationStrategy()
                    {
                        // Restrict which agents can approve (only the validation agent)
                        Agents = [kustoQueryValidationAgent],
                        // Set a maximum number of conversation turns to prevent infinite loops
                        MaximumIterations = 10,
                    }
            }
        };

        // Define a natural language query to be converted to KQL
        ChatMessageContent input = new(AuthorRole.User, "Where are all the sql servers that have a cpu percentage over 2?");

        // Add the user's query to the group chat
        agentGroupChat.AddChatMessage(input);

        // Process the conversation and display each response as it's generated
        await foreach (ChatMessageContent response in agentGroupChat.InvokeAsync())
        {
            Console.WriteLine(response);
        }

        // Indicate completion and wait for user input before closing
        Console.WriteLine("DONE");
        Console.ReadLine();
    }

    /// <summary>
    /// Configures and builds a Semantic Kernel with Azure OpenAI integration
    /// </summary>
    /// <param name="config">Application configuration containing OpenAI settings</param>
    /// <returns>A configured kernel builder</returns>
    static IKernelBuilder BuildKernel(IConfigurationRoot config)
    {
        var builder = Kernel.CreateBuilder();

        // Add Azure OpenAI chat completion capability to the kernel
        builder.Services.AddAzureOpenAIChatCompletion(
            deploymentName: config["AzureOpenAIDeploymentName"],
            endpoint: config["AzureOpenAIEndpoint"],
            apiKey: config["AzureOpenAIAPIKey"] ?? ""
        );

        return builder;
    }
}