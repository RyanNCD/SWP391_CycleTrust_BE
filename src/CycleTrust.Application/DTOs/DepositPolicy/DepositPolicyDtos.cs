namespace CycleTrust.Application.DTOs.DepositPolicy;

public class DepositPolicyDto
{
    public long Id { get; set; }
    public bool IsActive { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty; // PERCENT or FIXED
    public decimal? PercentValue { get; set; }
    public long? FixedAmount { get; set; }
    public long MinAmount { get; set; }
    public long? MaxAmount { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDepositPolicyRequest
{
    public string PolicyName { get; set; } = string.Empty;
    public string Mode { get; set; } = "PERCENT"; // PERCENT or FIXED
    public decimal? PercentValue { get; set; }
    public long? FixedAmount { get; set; }
    public long MinAmount { get; set; } = 0;
    public long? MaxAmount { get; set; }
    public string? Note { get; set; }
}

public class UpdateDepositPolicyRequest
{
    public string PolicyName { get; set; } = string.Empty;
    public string Mode { get; set; } = "PERCENT";
    public decimal? PercentValue { get; set; }
    public long? FixedAmount { get; set; }
    public long MinAmount { get; set; } = 0;
    public long? MaxAmount { get; set; }
    public string? Note { get; set; }
}
