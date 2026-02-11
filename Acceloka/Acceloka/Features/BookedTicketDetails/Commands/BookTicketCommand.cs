using Acceloka.Model;
using MediatR;

namespace Acceloka.Features.BookTicket.Commands
{
    public class BookTicketCommand : IRequest<BookTicketResponse>
    {
        public List<BookTicketItem> Items { get; set; } = new List<BookTicketItem>();
    }
}
