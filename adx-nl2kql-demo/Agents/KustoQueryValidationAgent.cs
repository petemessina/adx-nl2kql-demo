using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

namespace adx_nl2kql_demo.Agents
{
    /// <summary>
    /// Agent responsible for validating Kusto Query Language (KQL) syntax.
    /// Inherits from ChatHistoryAgent to integrate with conversation flow.
    /// </summary>
    public class KustoQueryValidationAgent : ChatHistoryAgent
    {
        // Underlying agent that handles the actual chat completion interactions
        private readonly ChatCompletionAgent _agent;

        // Override the Name property from base class to provide a specific agent identifier
        public new string Name => "KustoQueryValidationAgent";

        // Override the Description property from base class to describe this agent's purpose
        public new string Description => "Parses and validates Kusto queries.";

        /// <summary>
        /// Initializes a new instance of the KustoQueryValidationAgent.
        /// </summary>
        /// <param name="kernelBuilder">Builder that provides configuration for the Semantic Kernel</param>
        public KustoQueryValidationAgent(IKernelBuilder kernelBuilder)
        {
            // Build a kernel instance from the provided builder
            Kernel kernel = kernelBuilder.Build();

            // Configure OpenAI settings to automatically invoke kernel functions 
            // when the model detects a tool call in its response
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            // Create arguments with the execution settings
            var arguments = new KernelArguments(executionSettings);

            // Define the agent's instructions - this forms the system message that guides the agent's behavior
            var instructions = "Your goal is to validate kusto queries for any parser related issues. You have a tool to help parse and validte any errors that you must use to double check for issues. If the query has no syntax orparser errors only respond back with the word approve";

            // Initialize the ChatCompletionAgent with all required settings
            _agent = new()
            {
                Name = Name,
                Description = Description,
                Instructions = instructions,
                Kernel = kernel,
                Arguments = arguments
            };

            // Register this class's methods as plugins/tools that the agent can use
            _agent.Kernel.Plugins.AddFromObject(this);
        }

        /// <summary>
        /// Parses and validates a KQL query for syntax errors.
        /// This method is exposed as a kernel function that can be called by the agent.
        /// </summary>
        /// <param name="kustoQuery">The KQL query string to validate</param>
        /// <returns>A string indicating whether the query is valid or detailing any errors found</returns>
        [KernelFunction, Description("Parses and validates the kusto query.")]
        public async Task<string> ParseAndValidateKustoQuery(
            [Description("The kusto query to parse and validate.")] string kustoQuery
        )
        {
            // Use the Kusto Language library to parse the query
            var parsedQuery = Kusto.Language.KustoCode.Parse(kustoQuery);

            // Get any diagnostic issues from the parser
            var queryDiagnostics = parsedQuery.GetDiagnostics();

            // If there are diagnostic issues, report them
            if (queryDiagnostics.Count > 0)
            {
                return string.Join($"The query is invalid and has the below errors{Environment.NewLine}", queryDiagnostics.Select(d => d.ToString()));
            }
            else
            {
                // If no issues, the query is valid
                return "The query is valid.";
            }
        }

        /// <summary>
        /// Invokes the agent with a chat history to generate a non-streaming response.
        /// Delegates to the underlying ChatCompletionAgent.
        /// </summary>
        /// <inheritdoc/>
        public override IAsyncEnumerable<ChatMessageContent> InvokeAsync(ChatHistory history, KernelArguments? arguments = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            return _agent.InvokeAsync(history, arguments, kernel, cancellationToken);
        }

        /// <summary>
        /// Invokes the agent with a chat history to generate a streaming response.
        /// Delegates to the underlying ChatCompletionAgent.
        /// </summary>
        /// <inheritdoc/>
        public override IAsyncEnumerable<StreamingChatMessageContent> InvokeStreamingAsync(ChatHistory history, KernelArguments? arguments = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            return _agent.InvokeStreamingAsync(history, arguments, kernel, cancellationToken);
        }

        /// <summary>
        /// Invokes the agent with a collection of chat messages to generate a non-streaming response.
        /// Delegates to the underlying ChatCompletionAgent.
        /// </summary>
        /// <inheritdoc/>
        public override IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> InvokeAsync(ICollection<ChatMessageContent> messages, AgentThread? thread = null, AgentInvokeOptions? options = null, CancellationToken cancellationToken = default)
        {
            return this._agent.InvokeAsync(messages, thread, options, cancellationToken);
        }

        /// <summary>
        /// Invokes the agent with a collection of chat messages to generate a streaming response.
        /// Delegates to the underlying ChatCompletionAgent.
        /// </summary>
        /// <inheritdoc/>
        public override IAsyncEnumerable<AgentResponseItem<StreamingChatMessageContent>> InvokeStreamingAsync(ICollection<ChatMessageContent> messages, AgentThread? thread = null, AgentInvokeOptions? options = null, CancellationToken cancellationToken = default)
        {
            return this._agent.InvokeStreamingAsync(messages, thread, options, cancellationToken);
        }

        /// <summary>
        /// Restores an agent channel from a serialized state.
        /// This method is required by the base class but not implemented for this agent.
        /// </summary>
        /// <param name="channelState">The serialized channel state</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="NotImplementedException">This method is not implemented</exception>
        protected override Task<AgentChannel> RestoreChannelAsync(string channelState, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}