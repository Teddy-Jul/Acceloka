using FluentValidation;

namespace Acceloka.Features.EditBookedTicket.Commands
{
    public class EditBookedTicketCommandValidator : AbstractValidator<EditBookedTicketCommand>
    {
        // Validasi untuk EditBookedTicketCommand
        public EditBookedTicketCommandValidator()
        {
            RuleFor(x => x.BookedTicketId)
                .GreaterThan(0)
                .WithMessage("BookedTicketId must be greater than 0");

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