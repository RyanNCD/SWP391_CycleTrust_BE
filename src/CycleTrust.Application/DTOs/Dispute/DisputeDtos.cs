namespace CycleTrust.Application.DTOs.Dispute;

public class DisputeDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long OpenedBy { get; set; }
    public string OpenedByName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long? AssignedInspectorId { get; set; }
    public string? AssignedInspectorName { get; set; }
    public long? AssignedAdminId { get; set; }
    public string? AssignedAdminName { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<DisputeEventDto> Events { get; set; } = new();
}

public class DisputeEventDto
{
    public long Id { get; set; }
    public long? ActorId { get; set; }
    public string? ActorName { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateDisputeRequest
{
    public long OrderId { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public class AssignDisputeRequest
{
    public long? InspectorId { get; set; }
    public long? AdminId { get; set; }
}

public class ResolveDisputeRequest
{
    public string Resolution { get; set; } = string.Empty;
}

public class AddDisputeEventRequest
{
    public string Message { get; set; } = string.Empty;
}
