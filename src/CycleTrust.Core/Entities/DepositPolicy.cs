using CycleTrust.Core.Enums;

namespace CycleTrust.Core.Entities;

public class DepositPolicy : BaseEntity
{
    public bool IsActive { get; set; } = true;
    public string PolicyName { get; set; } = string.Empty;
    public DepositMode Mode { get; set; }
    public decimal? PercentValue { get; set; }
    public long? FixedAmount { get; set; }
    public long MinAmount { get; set; } = 0;
    public long? MaxAmount { get; set; }
    public string? Note { get; set; }
}
