using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for agent tool invocation audit records.
/// </summary>
public class AgentToolInvocationRepository : BaseRepository<AgentToolInvocationEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentToolInvocationRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public AgentToolInvocationRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }
}
