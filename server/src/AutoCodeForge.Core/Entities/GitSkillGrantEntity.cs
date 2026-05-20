using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Stores user-repository level grants for agent Git skills.
/// </summary>
[SugarTable("GitSkillGrants")]
public class GitSkillGrantEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the grant identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets target repository identifier.
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets permission level text: ReadOnly, Write, Dangerous.
    /// </summary>
    [SugarColumn(Length = 32, IsNullable = false)]
    public string Level { get; set; } = "ReadOnly";
}
