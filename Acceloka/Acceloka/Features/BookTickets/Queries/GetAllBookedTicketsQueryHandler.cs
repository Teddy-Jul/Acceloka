using Acceloka.Entities;
using Acceloka.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Acceloka.Features.BookedTickets.Queries
{
    public class GetAllBookedTicketsQueryHandler : IRequestHandler<GetAllBookedTicketsQuery, GetAllBookedTicketsResponse>
    {
        private readonly AccelokaContext _db;

        public GetAllBookedTicketsQueryHandler(AccelokaContext db)
        {
            _db = db;
        }

        public async Task<GetAllBookedTicketsResponse> Handle(GetAllBookedTicketsQuery request, CancellationToken cancellationToken)
        {
            // ambil semua booked tickets beserta detailnya
            var bookedTickets = await _db.BookedTickets
                .Include(bt => bt.BookedTicketDetails)
                .OrderByDescending(bt => bt.BookingDate)
                .Select(bt => new BookedTicketSummary
                {
                    BookedTicketId = bt.BookedTicketId,
                    BookingDate = bt.BookingDate,
                    TotalTickets = bt.BookedTicketDetails.Sum(btd => btd.Quantity)
                }).ToListAsync(cancellationToken);

            return new GetAllBookedTicketsResponse
            {
                BookedTickets = bookedTickets
            };
        }
    }
}