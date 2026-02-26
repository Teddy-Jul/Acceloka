using Acceloka.Model;
using MediatR;

namespace Acceloka.Features.BookedTickets.Queries
{
    public class GetBookedTicketDetailQuery : IRequest<GetBookedTicketResponse>
    {
        public int BookedTicketId { get; set; }
        public int UserId { get; set; }
    }
}