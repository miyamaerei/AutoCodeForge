using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.AI.GitTools;

/// <summary>
/// Request to set Git skill permission level for one repository.
/// </summary>
public class UpdateGitSkillGrantRequest
{
    /// <summary>
    /// Gets or sets level text: ReadOnly, Write, Dangerous.
    /// </summary>
    [Required]
    [MaxLength(32)]
    public string Level { get; set; } = "ReadOnly";
}
