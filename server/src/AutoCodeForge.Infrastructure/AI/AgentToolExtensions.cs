using AutoCodeForge.Core.Interfaces;
using Microsoft.Extensions.AI;

namespace AutoCodeForge.Infrastructure.AI;

/// <summary>
/// Extension methods for converting custom IAgentTool to Microsoft Agent Framework AIFunction.
/// </summary>
public static class AgentToolExtensions
{
    /// <summary>
    /// Converts a custom IAgentTool to Microsoft Agent Framework AIFunction.
    /// </summary>
    /// <param name="tool">The custom agent tool.</param>
    /// <returns>An AIFunction instance.</returns>
    public static AIFunction ToAgentTool(this IAgentTool tool)
    {
        // Create a wrapper function that handles the type conversion
        Func<Dictionary<string, object?>, CancellationToken, Task<object?>> wrapper = async (args, cancellationToken) =>
        {
            var input = new Dictionary<string, string>();
            foreach (var kvp in args)
            {
                input[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
            }
            
            var result = await tool.ExecuteAsync(input, cancellationToken);
            return (object?)result;
        };
        
        return AIFunctionFactory.Create(wrapper, tool.Name, tool.Description);
    }
}