using FluentValidation;

namespace NotificationSystem.Application.UseCases.GetAuditLogs;

/// <summary>
/// Validator for GetAuditLogsQuery.
/// Validates pagination parameters and date range logic.
/// </summary>
public class GetAuditLogsValidator : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsValidator()
    {
        // Validate pagination
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must not exceed 100");

        // Validate date range
        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Start date must be before end date");
    }
}
