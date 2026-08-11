using CrashDataApp.Models;
using FluentValidation;

namespace CrashDataApp.Validators;

public class PaginationQueryValidator : AbstractValidator<PaginationQuery>
{
    public PaginationQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 500).WithMessage("PageSize must be between 1 and 500.");
    }
}
