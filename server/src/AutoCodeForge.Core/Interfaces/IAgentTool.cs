namespace AutoCodeForge.Core.Interfaces;

/// <summary>
/// Defines one callable agent tool.
/// </summary>
public interface IAgentTool
{
    /// <summary>
    /// Gets the tool name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the tool description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the tool using provided input arguments.
    /// </summary>
    /// <param name="input">The tool input arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tool output as text.</returns>
    Task<string> ExecuteAsync(
        IReadOnlyDictionary<string, string> input,
        CancellationToken cancellationToken = default);
}
