using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a wiki page stored by the system.
/// </summary>
[SugarTable("WikiPages")]
public class WikiPageEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets wiki page identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets page title.
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = false)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets page slug.
    /// </summary>
    [SugarColumn(Length = 260, IsNullable = false)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets markdown content.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string Content { get; set; } = string.Empty;
}
