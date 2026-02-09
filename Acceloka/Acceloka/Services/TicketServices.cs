using Acceloka.Entities;
using Acceloka.Model;
using Microsoft.EntityFrameworkCore;

namespace Acceloka.Services
{
    public class TicketServices
    {
        private readonly AccelokaContext _db;

        public TicketServices(AccelokaContext db)
        {
            _db = db;
        }

        public async Task<List<AvailableTicketResponse>> GetAvailableTickets(
            string? namaKategori,
            string? kodeTicket,
            string? namaTicket,
            decimal? harga,
            DateTimeOffset? tanggalEventMinimal,
            DateTimeOffset? tanggalEventMaksimal,
            string orderBy,
            string orderState)
        {
            // Validasi langsung throw ArgumentException
            if (harga.HasValue && harga.Value < 0)
            {
                throw new ArgumentException("Harga harus lebih besar atau sama dengan 0");
            }

            if (tanggalEventMinimal.HasValue && tanggalEventMaksimal.HasValue
                && tanggalEventMinimal.Value > tanggalEventMaksimal.Value)
            {
                throw new ArgumentException("Tanggal event minimal harus lebih kecil atau sama dengan tanggal event maksimal");
            }

            var query = _db.Tickets.Include(t => t.Category).AsQueryable();

            // Filter
            if (!string.IsNullOrWhiteSpace(namaKategori))
            {
                query = query.Where(t => t.Category.CategoryName.Contains(namaKategori));
            }

            if (!string.IsNullOrWhiteSpace(kodeTicket))
            {
                query = query.Where(t => t.TicketCode.Contains(kodeTicket));
            }

            if (!string.IsNullOrWhiteSpace(namaTicket))
            {
                query = query.Where(t => t.TicketName.Contains(namaTicket));
            }

            if (harga.HasValue)
            {
                query = query.Where(t => t.Price <= harga.Value);
            }

            if (tanggalEventMinimal.HasValue)
            {
                query = query.Where(t => t.EventDate >= tanggalEventMinimal.Value);
            }

            if (tanggalEventMaksimal.HasValue)
            {
                query = query.Where(t => t.EventDate <= tanggalEventMaksimal.Value);
            }

            // Ordering
            query = ApplyOrdering(query, orderBy, orderState);

            var data = await query.Select(t => new AvailableTicketResponse
            {
                NamaKategori = t.Category.CategoryName,
                KodeTicket = t.TicketCode,
                NamaTicket = t.TicketName,
                TanggalEvent = t.EventDate,
                Harga = t.Price,
                SisaQuota = t.RemainingQuota
            }).ToListAsync();

            return data;
        }

        public async Task<PagedResult<AvailableTicketResponse>> GetAvailableTickets(
            string? namaKategori,
            string? kodeTicket,
            string? namaTicket,
            decimal? harga,
            DateTimeOffset? tanggalEventMinimal,
            DateTimeOffset? tanggalEventMaksimal,
            string orderBy,
            string orderState,
            int page = 1,
            int pageSize = 10)
        {
            var query = _db.Tickets.Include(t => t.Category).AsQueryable();

            // Filter
            if (!string.IsNullOrWhiteSpace(namaKategori))
            {
                query = query.Where(t => t.Category.CategoryName.Contains(namaKategori));
            }

            if (!string.IsNullOrWhiteSpace(kodeTicket))
            {
                query = query.Where(t => t.TicketCode.Contains(kodeTicket));
            }

            if (!string.IsNullOrWhiteSpace(namaTicket))
            {
                query = query.Where(t => t.TicketName.Contains(namaTicket));
            }

            if (harga.HasValue)
            {
                query = query.Where(t => t.Price <= harga.Value);
            }

            if (tanggalEventMinimal.HasValue)
            {
                query = query.Where(t => t.EventDate >= tanggalEventMinimal.Value);
            }

            if (tanggalEventMaksimal.HasValue)
            {
                query = query.Where(t => t.EventDate <= tanggalEventMaksimal.Value);
            }

            // Get TOTAL COUNT sebelum pagination
            var totalCount = await query.CountAsync();

            // Apply ordering
            query = ApplyOrdering(query, orderBy, orderState);

            // PAGINATION: Skip
            var skip = (page - 1) * pageSize;  // page 1: skip 0, page 2: skip 10, dst
            query = query.Skip(skip).Take(pageSize);

            // Execute query
            var tickets = await query.ToListAsync();

            // Map to response
            var data = tickets.Select(t => new AvailableTicketResponse
            {
                NamaKategori = t.Category.CategoryName,
                KodeTicket = t.TicketCode,
                NamaTicket = t.TicketName,
                TanggalEvent = t.EventDate,
                Harga = t.Price,
                SisaQuota = t.RemainingQuota
            }).ToList();

            // Calculate pagination info
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<AvailableTicketResponse>
            {
                Data = data,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        private IQueryable<Ticket> ApplyOrdering(IQueryable<Ticket> query, string orderBy, string orderState)
        {
            var isDescending = orderState.ToLower() == "descending";

            return orderBy.ToLower() switch
            {
                "namakategori" => isDescending
                    ? query.OrderByDescending(t => t.Category.CategoryName)
                    : query.OrderBy(t => t.Category.CategoryName),
                "kodeticket" => isDescending
                    ? query.OrderByDescending(t => t.TicketCode)
                    : query.OrderBy(t => t.TicketCode),
                "namaticket" => isDescending
                    ? query.OrderByDescending(t => t.TicketName)
                    : query.OrderBy(t => t.TicketName),
                "tanggalevent" => isDescending
                    ? query.OrderByDescending(t => t.EventDate)
                    : query.OrderBy(t => t.EventDate),
                "harga" => isDescending
                    ? query.OrderByDescending(t => t.Price)
                    : query.OrderBy(t => t.Price),
                "sisaquota" => isDescending
                    ? query.OrderByDescending(t => t.RemainingQuota)
                    : query.OrderBy(t => t.RemainingQuota),
                _ => query.OrderBy(t => t.TicketCode)
            };
        }
    }
}
