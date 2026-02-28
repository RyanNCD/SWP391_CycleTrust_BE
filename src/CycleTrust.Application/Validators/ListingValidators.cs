using FluentValidation;
using CycleTrust.Application.DTOs.Listing;

namespace CycleTrust.Application.Validators;

public class CreateListingRequestValidator : AbstractValidator<CreateListingRequest>
{
    public CreateListingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.PriceAmount).GreaterThan(0);
    }
}
