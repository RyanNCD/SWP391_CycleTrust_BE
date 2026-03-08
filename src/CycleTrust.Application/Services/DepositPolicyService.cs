using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.DepositPolicy;

namespace CycleTrust.Application.Services;

public interface IDepositPolicyService
{
    Task<DepositPolicyDto?> GetActivePolicyAsync();
    Task<List<DepositPolicyDto>> GetAllPoliciesAsync();
    Task<DepositPolicyDto> CreatePolicyAsync(CreateDepositPolicyRequest request);
    Task<DepositPolicyDto> UpdatePolicyAsync(long id, UpdateDepositPolicyRequest request);
    Task<DepositPolicyDto> SetActiveAsync(long id, bool isActive);
}

public class DepositPolicyService : IDepositPolicyService
{
    private readonly CycleTrustDbContext _context;
    private readonly IMapper _mapper;

    public DepositPolicyService(CycleTrustDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DepositPolicyDto?> GetActivePolicyAsync()
    {
        var policy = await _context.DepositPolicies
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        return policy == null ? null : _mapper.Map<DepositPolicyDto>(policy);
    }

    public async Task<List<DepositPolicyDto>> GetAllPoliciesAsync()
    {
        var policies = await _context.DepositPolicies
            .OrderByDescending(p => p.IsActive)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<DepositPolicyDto>>(policies);
    }

    public async Task<DepositPolicyDto> CreatePolicyAsync(CreateDepositPolicyRequest request)
    {
        // Validate mode
        if (!Enum.TryParse<DepositMode>(request.Mode, true, out var mode))
            throw new Exception("Mode không hợp lệ. Chỉ chấp nhận PERCENT hoặc FIXED");

        // Validate based on mode
        if (mode == DepositMode.PERCENT && (!request.PercentValue.HasValue || request.PercentValue <= 0 || request.PercentValue > 100))
            throw new Exception("PercentValue phải từ 0-100 khi Mode là PERCENT");

        if (mode == DepositMode.FIXED && (!request.FixedAmount.HasValue || request.FixedAmount <= 0))
            throw new Exception("FixedAmount phải > 0 khi Mode là FIXED");

        // Create policy (initially active)
        var policy = new DepositPolicy
        {
            IsActive = true,
            PolicyName = request.PolicyName,
            Mode = mode,
            PercentValue = request.PercentValue,
            FixedAmount = request.FixedAmount,
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            Note = request.Note
        };

        // Deactivate other policies if this is set to active
        var otherPolicies = await _context.DepositPolicies
            .Where(p => p.IsActive)
            .ToListAsync();

        foreach (var p in otherPolicies)
        {
            p.IsActive = false;
        }

        _context.DepositPolicies.Add(policy);
        await _context.SaveChangesAsync();

        return _mapper.Map<DepositPolicyDto>(policy);
    }

    public async Task<DepositPolicyDto> UpdatePolicyAsync(long id, UpdateDepositPolicyRequest request)
    {
        var policy = await _context.DepositPolicies.FindAsync(id);

        if (policy == null)
            throw new Exception("Policy không tồn tại");

        // Validate mode
        if (!Enum.TryParse<DepositMode>(request.Mode, true, out var mode))
            throw new Exception("Mode không hợp lệ. Chỉ chấp nhận PERCENT hoặc FIXED");

        // Validate based on mode
        if (mode == DepositMode.PERCENT && (!request.PercentValue.HasValue || request.PercentValue <= 0 || request.PercentValue > 100))
            throw new Exception("PercentValue phải từ 0-100 khi Mode là PERCENT");

        if (mode == DepositMode.FIXED && (!request.FixedAmount.HasValue || request.FixedAmount <= 0))
            throw new Exception("FixedAmount phải > 0 khi Mode là FIXED");

        policy.PolicyName = request.PolicyName;
        policy.Mode = mode;
        policy.PercentValue = request.PercentValue;
        policy.FixedAmount = request.FixedAmount;
        policy.MinAmount = request.MinAmount;
        policy.MaxAmount = request.MaxAmount;
        policy.Note = request.Note;

        await _context.SaveChangesAsync();

        return _mapper.Map<DepositPolicyDto>(policy);
    }

    public async Task<DepositPolicyDto> SetActiveAsync(long id, bool isActive)
    {
        var policy = await _context.DepositPolicies.FindAsync(id);

        if (policy == null)
            throw new Exception("Policy không tồn tại");

        if (isActive)
        {
            // Deactivate all other policies
            var otherPolicies = await _context.DepositPolicies
                .Where(p => p.IsActive && p.Id != id)
                .ToListAsync();

            foreach (var p in otherPolicies)
            {
                p.IsActive = false;
            }
        }

        policy.IsActive = isActive;
        await _context.SaveChangesAsync();

        return _mapper.Map<DepositPolicyDto>(policy);
    }
}
