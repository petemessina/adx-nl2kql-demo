using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;

namespace adx_nl2kql_demo.TerminationStrategies
{
    /// <summary>
    /// A termination strategy that ends agent conversations when an approval message is detected.
    /// This strategy inherits from the base TerminationStrategy class provided by Semantic Kernel.
    /// </summary>
    public class ApprovalTerminationStrategy : TerminationStrategy
    {
        /// <summary>
        /// Determines whether the agent conversation should terminate based on message content.
        /// </summary>
        /// <param name="agent">The agent that is participating in the conversation</param>
        /// <param name="history">The complete conversation history up to the current point</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>
        /// A task that resolves to true if the conversation should terminate (because 
        /// the most recent message contains the word "approve"), or false otherwise
        /// </returns>
        /// <remarks>
        /// This implementation checks if the last message in the history contains the word "approve"
        /// using a case-insensitive comparison. If it does, the conversation will terminate.
        /// The null-conditional operator (?.) and null-coalescing operator (??) are used to handle 
        /// potential null content in the message safely.
        /// </remarks>
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
            => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
    }
}