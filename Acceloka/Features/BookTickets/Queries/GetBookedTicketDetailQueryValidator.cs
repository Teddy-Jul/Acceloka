using Acceloka.Features.BookedTickets.Queries;
using FluentValidation;

namespace Acceloka.Features.BookTickets.Queries
{
    public class GetBookedTicketDetailQueryValidator : AbstractValidator<GetBookedTicketDetailQuery>
    {
        // Validasi untuk GetBookedTicketDetailQuery
        public GetBookedTicketDetailQueryValidator()
        {
            RuleFor(x => x.BookedTicketId)
                .GreaterThan(0)
                .WithMessage("BookedTicketId must be greater than 0");

            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("UserId must be greater than 0");
        }
    }
}