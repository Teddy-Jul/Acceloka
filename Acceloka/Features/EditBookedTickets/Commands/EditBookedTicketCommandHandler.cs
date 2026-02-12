using Acceloka.Entities;
using Acceloka.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Acceloka.Features.EditBookedTicket.Commands
{
    public class EditBookedTicketCommandHandler : IRequestHandler<EditBookedTicketCommand, EditBookedTicketResponse>
    {
        private readonly AccelokaContext _db;

        public EditBookedTicketCommandHandler(AccelokaContext db)
        {
            _db = db;
        }

        public async Task<EditBookedTicketResponse> Handle(EditBookedTicketCommand request, CancellationToken cancellationToken)
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

                var editedTickets = new List<EditedTicketInfo>();

                foreach (var item in request.Items)
                {
                    // Validasi Kode Ticket dalam Booking
                    var bookedDetail = bookedTicket.BookedTicketDetails
                        .FirstOrDefault(btd => btd.Ticket.TicketCode == item.TicketCode);

                    if (bookedDetail == null)
                    {
                        throw new KeyNotFoundException($"Ticket code {item.TicketCode} not found in BookedTicketId {request.BookedTicketId}");
                    }

                    // Validasi Quantity tidak boleh kurang dari 1
                    if (item.Quantity < 1)
                    {
                        throw new InvalidOperationException($"Quantity must be at least 1 for ticket {item.TicketCode}");
                    }

                    // Max Ticket Quantity yang boleh di booking
                    var ticket = bookedDetail.Ticket;
                    var maxAllowedQuantity = bookedDetail.Quantity + ticket.RemainingQuota;

                    // Validasi Quantity tidak boleh melebihi sisa quota
                    if (item.Quantity > maxAllowedQuantity)
                    {
                        throw new InvalidOperationException($"Quantity for {item.TicketCode} exceeds remaining quota. Maximum allowed: {maxAllowedQuantity}, requested: {item.Quantity}");
                    }

                    // perubahan quantity
                    var quantityDif = item.Quantity - bookedDetail.Quantity;

                    // update RemainingQuota
                    ticket.RemainingQuota -= quantityDif;
                    ticket.UpdatedAt = DateTimeOffset.Now;

                    // Update BookedTicketDetail
                    bookedDetail.Quantity = item.Quantity;
                    bookedDetail.Price = ticket.Price * bookedDetail.Quantity;
                    bookedDetail.UpdatedAt = DateTimeOffset.Now;

                    //  response untuk Detail Tickets (Bukan milik user)
                    
                    //editedTickets.Add(new EditedTicketInfo
                    //{
                    //    TicketCode = ticket.TicketCode,
                    //    TicketName = ticket.TicketName,
                    //    CategoryName = ticket.Category.CategoryName,
                    //    RemainingQuantity = ticket.RemainingQuota
                    //});
                    
                }

                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // response untuk Detail Tickets (Milik user)
                var remainingTickets = await _db.BookedTicketDetails
                    .Include(btd => btd.Ticket)
                    .ThenInclude(t => t.Category)
                    .Where(btd => btd.BookedTicketId == request.BookedTicketId)
                    .Select(btd => new EditedTicketInfo
                    {
                        TicketCode = btd.Ticket.TicketCode,
                        TicketName = btd.Ticket.TicketName,
                        CategoryName = btd.Ticket.Category.CategoryName,
                        RemainingQuantity = btd.Quantity
                    }).ToListAsync(cancellationToken);

                return new EditBookedTicketResponse
                {
                    // response untuk Detail Tickets (Bukan milik user)
                    //EditedTickets = editedTickets

                    // response untuk Detail Tickets (Milik user)
                    EditedTickets = remainingTickets
                };
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}