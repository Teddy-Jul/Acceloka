using Acceloka.Entities;
using Acceloka.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Acceloka.Features.BookedTickets.Queries
{
    public class GetBookedTicketDetailQueryHandler : IRequestHandler<GetBookedTicketDetailQuery, GetBookedTicketResponse>
    {
        private readonly AccelokaContext _db;

        public GetBookedTicketDetailQueryHandler(AccelokaContext db)
        {
            _db = db;
        }

        public async Task<GetBookedTicketResponse> Handle(GetBookedTicketDetailQuery request, CancellationToken cancellationToken)
        {
            // Ambil data BookedTicket beserta detailnya
            var bookedTicket = await _db.BookedTickets
                .Include(bt => bt.BookedTicketDetails)
                    .ThenInclude(btd => btd.Ticket)
                        .ThenInclude(t => t.Category)
                .FirstOrDefaultAsync(bt => bt.BookedTicketId == request.BookedTicketId, cancellationToken);

            // Validasi apakah BookedTicketId ada
            if (bookedTicket == null)
            {
                throw new KeyNotFoundException($"BookedTicketId {request.BookedTicketId} not found");
            }

            // Validasi apakah booking milik user yang request
            if (bookedTicket.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this booking.");
            }

            // Group by Category dan hitung total quantity per kategori
            var response = new GetBookedTicketResponse
            {
                Categories = bookedTicket.BookedTicketDetails
                    .GroupBy(btd => btd.Ticket.Category.CategoryName)
                    .Select(categoryGroup => new BookedTicketCategoryGroup
                    {
                        QtyPerCategory = categoryGroup.Sum(btd => btd.Quantity),
                        CategoryName = categoryGroup.Key,
                        Tickets = categoryGroup.Select(btd => new BookedTicketDetailInfo
                        {
                            TicketCode = btd.Ticket.TicketCode,
                            TicketName = btd.Ticket.TicketName,
                            EventDate = btd.Ticket.EventDate,
                            TicketAmount = btd.Quantity
                        }).ToList()
                    }).ToList()
            };

            return response;
        }
    }
}