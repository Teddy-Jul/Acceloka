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
        }
    }
}