using Acceloka.Entities;
using Acceloka.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Acceloka.Features.BookTicket.Commands
{
    public class BookTicketCommandHandler : IRequestHandler<BookTicketCommand, BookTicketResponse>
    {
        private readonly AccelokaContext _db;

        public BookTicketCommandHandler(AccelokaContext db)
        {
            _db = db;
        }

        public async Task<BookTicketResponse> Handle(BookTicketCommand request, CancellationToken cancellationToken)
        {
            var bookingDate = DateTimeOffset.Now;

            // Ambil semua kode tiket yang akan dibooking (distinct untuk menghindari duplikat)
            var ticketCodes = request.Items.Select(i => i.TicketCode).Distinct().ToList();

            // Ambil data tiket dari database
            var tickets = await _db.Tickets
                .Include(t => t.Category)
                .Where(t => ticketCodes.Contains(t.TicketCode))
                .ToListAsync(cancellationToken);

            // Validasi Kode Tiket tidak ada
            var notFoundCodes = ticketCodes.Except(tickets.Select(t => t.TicketCode)).ToList();
            if (notFoundCodes.Any())
            {
                throw new KeyNotFoundException($"Ticket code not found: {string.Join(", ", notFoundCodes)}");
            }

            // Validasi quota habis, quantity melebihi quota, dan tanggal event
            foreach (var item in request.Items)
            {
                var ticket = tickets.First(t => t.TicketCode == item.TicketCode);

                // Validasi quota ticket habis
                if (ticket.RemainingQuota == 0)
                {
                    throw new InvalidOperationException($"Ticket quota is sold out: {item.TicketCode}");
                }

                // Validasi Quantity melebihi sisa quota
                if (item.Quantity > ticket.RemainingQuota)
                {
                    throw new InvalidOperationException(
                        $"Quantity exceeds remaining quota for {item.TicketCode}. " +
                        $"Remaining quota: {ticket.RemainingQuota}, requested: {item.Quantity}");
                }

                // validasi Tanggal event tidak boleh sebelum tanggal booking
                if (ticket.EventDate <= bookingDate)
                {
                    throw new InvalidOperationException(
                        $"Event date for ticket {item.TicketCode} must be greater than booking date");
                }
            }

            using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Buat object BookedTicket
                var bookedTicket = new BookedTicket
                {
                    BookingDate = bookingDate,
                    CreatedAt = bookingDate,
                    UserId = request.UserId
                };

                _db.BookedTickets.Add(bookedTicket);
                await _db.SaveChangesAsync(cancellationToken);

                // Buat BookedTicketDetails dan update kuota tiket
                foreach (var item in request.Items)
                {
                    var ticket = tickets.First(t => t.TicketCode == item.TicketCode);

                    // Simpan properti Price di BookedTicketDetail
                    var detail = new BookedTicketDetail
                    {
                        BookedTicketId = bookedTicket.BookedTicketId,
                        TicketId = ticket.TicketId,
                        Quantity = item.Quantity,
                        Price = ticket.Price * item.Quantity,
                        CreatedAt = bookingDate
                    };

                    _db.BookedTicketDetails.Add(detail);

                    // Update sisa kuota tiket
                    ticket.RemainingQuota -= item.Quantity;
                    ticket.UpdatedAt = bookingDate;
                }

                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Response Group by category
                var response = new BookTicketResponse
                {
                    TicketsPerCategory = tickets
                        .GroupBy(t => t.Category.CategoryName)
                        .Select(categoryGroup => new TicketPerCategory
                        {
                            CategoryName = categoryGroup.Key,
                            Tickets = categoryGroup.Select(ticket =>
                            {
                                var requestItem = request.Items.First(i => i.TicketCode == ticket.TicketCode);
                                return new BookedTicketInfo
                                {
                                    TicketCode = ticket.TicketCode,
                                    TicketName = ticket.TicketName,
                                    Price = ticket.Price * requestItem.Quantity
                                };
                            }).ToList(),
                            SubtotalPrice = categoryGroup.Sum(ticket =>
                            {
                                var requestItem = request.Items.First(i => i.TicketCode == ticket.TicketCode);
                                return ticket.Price * requestItem.Quantity;
                            })
                        }).ToList()
                };

                response.TotalPrice = response.TicketsPerCategory.Sum(c => c.SubtotalPrice);

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