using FluentValidation;

namespace Acceloka.Features.BookTicket.Commands
{
    public class BookTicketCommandValidator : AbstractValidator<BookTicketCommand>
    {
        // Validasi untuk BookTicketCommand
        public BookTicketCommandValidator()
        {
            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Items cannot be empty");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.TicketCode)
                    .NotEmpty()
                    .WithMessage("TicketCode is required");

                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Quantity must be greater than 0");
            });
        }
    }
}