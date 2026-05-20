using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for LLM model configurations.
/// </summary>
public class LLMModelConfigRepository : BaseRepository<LLMModelConfigEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LLMModelConfigRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public LLMModelConfigRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets one model config by provider and name.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="modelName">The model name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The model config or null.</returns>
    public async Task<LLMModelConfigEntity?> GetByProviderAndModelAsync(
        LLMProvider provider,
        string modelName,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable.FirstAsync(config => config.Provider == provider && config.ModelName == modelName);
    }
}
