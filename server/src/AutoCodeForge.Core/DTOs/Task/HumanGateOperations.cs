namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// Represents a request to approve a human gate.
/// </summary>
public class ApproveRequest
{
    public string? Comment { get; set; }
}

/// <summary>
/// Represents a request to reject a human gate.
/// </summary>
public class RejectRequest
{
    public string? Reason { get; set; }
}

/// <summary>
/// Represents a request to approve with modifications.
/// </summary>
public class ModifyApproveRequest
{
    public GateModifications? Modifications { get; set; }
}

/// <summary>
/// Represents modifications made during gate approval.
/// </summary>
public class GateModifications
{
    public string? Input { get; set; }
    public string? Instructions { get; set; }
}

/// <summary>
/// Represents a request to update task requirement.
/// </summary>
public class UpdateRequirementRequest
{
    public string NewInput { get; set; } = string.Empty;
    public int? RestartFromStep { get; set; }
}