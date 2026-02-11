using Acceloka.Model;
using MediatR;

namespace Acceloka.Features.EditBookedTicket.Commands
{
    public class EditBookedTicketCommand : IRequest<EditBookedTicketResponse>
    {
        public int BookedTicketId { get; set; }
        public List<EditBookedTicketItem> Items { get; set; } = new List<EditBookedTicketItem>();
    }
}