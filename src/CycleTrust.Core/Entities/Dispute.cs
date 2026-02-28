using CycleTrust.Core.Enums;

namespace CycleTrust.Core.Entities;

public class Dispute : BaseEntity
{
    public long OrderId { get; set; }
    public long OpenedBy { get; set; }
    public DisputeStatus Status { get; set; } = DisputeStatus.OPEN;
    public long? AssignedInspectorId { get; set; }
    public long? AssignedAdminId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public Order Order { get; set; } = null!;
    public User OpenedByUser { get; set; } = null!;
    public User? AssignedInspector { get; set; }
    public User? AssignedAdmin { get; set; }
    public ICollection<DisputeEvent> Events { get; set; } = new List<DisputeEvent>();
}
