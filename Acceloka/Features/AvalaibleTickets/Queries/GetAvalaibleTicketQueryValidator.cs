using FluentValidation;

namespace Acceloka.Features.AvalaibleTickets.Queries
{
    public class GetAvailableTicketQueryValidator : AbstractValidator<GetAvailableTicketsQuery>
    {
        // Validasi untuk GetAvailableTickets
        public GetAvailableTicketQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("PageSize maximum is 100");

            RuleFor(x => x.OrderState)
                .Must(x => x.ToLower() == "ascending" || x.ToLower() == "descending")
                .WithMessage("OrderState must be 'ascending' or 'descending'");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Price.HasValue)
                .WithMessage("Price cannot be negative");

            RuleFor(x => x.MaxEventDate)
                .GreaterThanOrEqualTo(x => x.MinEventDate)
                .When(x => x.MinEventDate.HasValue && x.MaxEventDate.HasValue)
                .WithMessage("MaxEventDate must be greater than or equal to MinEventDate");
        }
    }
}
