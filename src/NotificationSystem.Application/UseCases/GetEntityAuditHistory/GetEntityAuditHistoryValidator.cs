using FluentValidation;

namespace NotificationSystem.Application.UseCases.GetEntityAuditHistory;

/// <summary>
/// Validator for GetEntityAuditHistoryQuery.
/// Validates required parameters and pagination options.
/// </summary>
public class GetEntityAuditHistoryValidator : AbstractValidator<GetEntityAuditHistoryQuery>
{
    public GetEntityAuditHistoryValidator()
    {
        // Entity name is required
        RuleFor(x => x.EntityName)
            .NotEmpty()
            .WithMessage("Entity name is required");

        // Entity ID is required
        RuleFor(x => x.EntityId)
            .NotEmpty()
            .WithMessage("Entity ID is required");

        // Validate pagination
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must not exceed 100");
    }
}
