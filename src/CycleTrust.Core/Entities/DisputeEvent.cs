namespace CycleTrust.Core.Entities;

public class DisputeEvent : BaseEntity
{
    public long DisputeId { get; set; }
    public long? ActorId { get; set; }
    public string Message { get; set; } = string.Empty;
    
    public Dispute Dispute { get; set; } = null!;
    public User? Actor { get; set; }
}
