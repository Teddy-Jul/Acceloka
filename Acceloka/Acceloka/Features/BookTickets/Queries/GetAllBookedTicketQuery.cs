using Acceloka.Model;
using MediatR;

namespace Acceloka.Features.BookedTickets.Queries
{
    public class GetAllBookedTicketsQuery : IRequest<GetAllBookedTicketsResponse>
    {
    }
}