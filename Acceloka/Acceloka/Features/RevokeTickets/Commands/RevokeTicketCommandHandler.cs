using Acceloka.Entities;
using Acceloka.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Acceloka.Features.RevokeTicket.Commands
{
    public class RevokeTicketCommandHandler : IRequestHandler<RevokeTicketCommand, RevokeTicketResponse>
    {
        private readonly AccelokaContext _db;

        public RevokeTicketCommandHandler(AccelokaContext db)
        {
            _db = db;
        }

        public async Task<RevokeTicketResponse> Handle(RevokeTicketCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Validasi adanya BookedTicketId
                var bookedTicket = await _db.BookedTickets
                    .Include(bt => bt.BookedTicketDetails)
                        .ThenInclude(btd => btd.Ticket)
                            .ThenInclude(t => t.Category)
                    .FirstOrDefaultAsync(bt => bt.BookedTicketId == request.BookedTicketId, cancellationToken);

                if (bookedTicket == null)
                {
                    throw new KeyNotFoundException($"BookedTicketId {request.BookedTicketId} not found");
                }

                // Validasi Kode Ticket dalam Booking
                var bookedDetail = bookedTicket.BookedTicketDetails
                    .FirstOrDefault(btd => btd.Ticket.TicketCode == request.TicketCode);

                if (bookedDetail == null)
                {
                    throw new KeyNotFoundException($"Ticket code {request.TicketCode} not found in BookedTicketId {request.BookedTicketId}");
                }

                // Validasi Quantity harus <= quantity yang sudah dibooking
                if (request.Quantity > bookedDetail.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Quantity to revoke ({request.Quantity}) exceeds booked quantity ({bookedDetail.Quantity}) for ticket {request.TicketCode}");
                }

                // Update RemainingQuota di tabel Tickets (mengembalikan quota)
                var ticket = bookedDetail.Ticket;
                ticket.RemainingQuota += request.Quantity;
                ticket.UpdatedAt = DateTimeOffset.Now;

                // Update atau hapus BookedTicketDetail
                if (request.Quantity == bookedDetail.Quantity)
                {
                    _db.BookedTicketDetails.Remove(bookedDetail);
                }
                else
                {
                    bookedDetail.Quantity -= request.Quantity;
                    bookedDetail.Price = ticket.Price * bookedDetail.Quantity;
                    bookedDetail.UpdatedAt = DateTimeOffset.Now;
                }

                await _db.SaveChangesAsync(cancellationToken);

                // Cek apakah masih ada detail lain di BookedTicket ini
                var remainingDetails = await _db.BookedTicketDetails
                    .Include(btd => btd.Ticket)
                        .ThenInclude(t => t.Category)
                    .Where(btd => btd.BookedTicketId == request.BookedTicketId)
                    .ToListAsync(cancellationToken);

                // Jika tidak ada detail lagi, hapus BookedTicket
                if (!remainingDetails.Any())
                {
                    _db.BookedTickets.Remove(bookedTicket);
                    await _db.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                // Response tampilkan sisa tiket yang masih dibooking
                var response = new RevokeTicketResponse
                {
                    RemainingTicket = remainingDetails.Select(btd => new RevokeTicketItem
                    {
                        TicketCode = btd.Ticket.TicketCode,
                        TicketName = btd.Ticket.TicketName,
                        CategoryName = btd.Ticket.Category.CategoryName,
                        RemainingQuantity = btd.Quantity
                    }).ToList()
                };

                return response;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}