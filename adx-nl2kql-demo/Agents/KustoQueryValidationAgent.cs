using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

namespace adx_nl2kql_demo.Agents
{
    public class KustoQueryValidationAgent : ChatHistoryAgent
    {
        private readonly ChatCompletionAgent _agent;

        public new string Name => "KustoQueryValidationAgent";

        public new string Description => "Parses and validates Kusto queries.";

        public KustoQueryValidationAgent(IKernelBuilder kernelBuilder)
        {
            Kernel kernel = kernelBuilder.Build();
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var arguments = new KernelArguments(executionSettings);
            var instructions = "Your goal is to validate kusto queries for any parser related issues. You have a tool to help parse and validte any errors that you must use to double check for issues. If the query has no syntax orparser errors only respond back with the word approve";

            _agent = new()
            {
                Name = Name,
                Description = Description,
                Instructions = instructions,
                Kernel = kernel,
                Arguments = arguments
            };

            _agent.Kernel.Plugins.AddFromObject(this);
        }

        [KernelFunction, Description("Parses and validates the kusto query.")]
        public async Task<string> ParseAndValidateKustoQuery(
            [Description("The kusto query to parse and validate.")] string kustoQuery
        ) {
            var parsedQuery = Kusto.Language.KustoCode.Parse(kustoQuery);
            var queryDiagnostics = parsedQuery.GetDiagnostics();

            if(queryDiagnostics.Count > 0)
            {
                return string.Join($"The query is invalid and has the below errors{Environment.NewLine}", queryDiagnostics.Select(d => d.ToString()));
            }
            else
            {
                return "The query is valid.";
            }
        }

        public override IAsyncEnumerable<ChatMessageContent> InvokeAsync(ChatHistory history, KernelArguments? arguments = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            return _agent.InvokeAsync(history, arguments, kernel, cancellationToken);
        }

        public override IAsyncEnumerable<StreamingChatMessageContent> InvokeStreamingAsync(ChatHistory history, KernelArguments? arguments = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            return _agent.InvokeStreamingAsync(history, arguments, kernel, cancellationToken);
        }

        public override IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> InvokeAsync(ICollection<ChatMessageContent> messages, AgentThread? thread = null, AgentInvokeOptions? options = null, CancellationToken cancellationToken = default)
        {
            return this._agent.InvokeAsync(messages, thread, options, cancellationToken);
        }

        public override IAsyncEnumerable<AgentResponseItem<StreamingChatMessageContent>> InvokeStreamingAsync(ICollection<ChatMessageContent> messages, AgentThread? thread = null, AgentInvokeOptions? options = null, CancellationToken cancellationToken = default)
        {
            return this._agent.InvokeStreamingAsync(messages, thread, options, cancellationToken);
        }

        protected override Task<AgentChannel> RestoreChannelAsync(string channelState, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
