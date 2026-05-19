using SqlSugar;

namespace __ProjectName__.Entities;

[SugarTable("WikiPages")]
public class WikiPageEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public Guid? RepositoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string? MetaJson { get; set; }

    public Guid AuthorId { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
