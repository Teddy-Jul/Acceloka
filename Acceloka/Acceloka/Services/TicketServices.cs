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
            var skip = (page - 1) * pageSize;
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

        public async Task<BookTicketResponse> BookTicket(BookTicketRequest request)
        {
            var bookingDate = DateTimeOffset.Now;

            // Ambil semua kode tiket yang akan dibooking (distinct untuk menghindari duplikat)
            var ticketCodes = request.Items.Select(i => i.TicketCode).Distinct().ToList();

            // Ambil data tiket dari database
            var tickets = await _db.Tickets
                .Include(t => t.Category)
                .Where(t => ticketCodes.Contains(t.TicketCode))
                .ToListAsync();

            //  Validasi Kode Tiket tidak ada
            var notFoundCodes = ticketCodes.Except(tickets.Select(t => t.TicketCode)).ToList();
            if (notFoundCodes.Any())
            {
                throw new KeyNotFoundException($"Kode tiket tidak terdaftar: {string.Join(", ", notFoundCodes)}");
            }

            // Validasi quota habis , quantity melebihi quota, dan tanggal event
            foreach (var item in request.Items)
            {
                var ticket = tickets.First(t => t.TicketCode == item.TicketCode);

                // Validasi quota ticket habis
                if (ticket.RemainingQuota == 0)
                {
                    throw new InvalidOperationException($"Kode tiket quotanya habis: {item.TicketCode}");
                }

                // Validasi Quantity melebihi sisa quota
                if (item.Quantity > ticket.RemainingQuota)
                {
                    throw new InvalidOperationException(
                        $"Quantity tiket yang dibooking melebihi sisa quota untuk {item.TicketCode}. " +
                        $"Sisa quota: {ticket.RemainingQuota}, diminta: {item.Quantity}");
                }

                // validasi Tanggal event tidak boleh sebelum tanggal booking
                if (ticket.EventDate <= bookingDate)
                {
                    throw new InvalidOperationException(
                        $"Tanggal event untuk tiket {item.TicketCode} tidak boleh kurang dari atau sama dengan tanggal booking");
                }
            }


            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Buat entitas BookedTicket
                var bookedTicket = new BookedTicket
                {
                    BookingDate = bookingDate,
                    CreatedAt = bookingDate
                };

                _db.BookedTickets.Add(bookedTicket);
                await _db.SaveChangesAsync();

                // Buat BookedTicketDetails dan update kuota tiket
                foreach (var item in request.Items)
                {
                    var ticket = tickets.First(t => t.TicketCode == item.TicketCode);

                    //Simpan properti Price di BookedTicketDetail
                    var detail = new BookedTicketDetail
                    {
                        BookedTicketId = bookedTicket.BookedTicketId,
                        TicketId = ticket.TicketId,
                        Quantity = item.Quantity,
                        Price = ticket.Price * item.Quantity,  // Total harga (Price * Quantity)
                        CreatedAt = bookingDate
                    };

                    _db.BookedTicketDetails.Add(detail);

                    // Update remaining quota
                    ticket.RemainingQuota -= item.Quantity;
                    ticket.UpdatedAt = bookingDate;
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Build response: Group by category
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

                // Total keseluruhan
                response.TotalPrice = response.TicketsPerCategory.Sum(c => c.SubtotalPrice);

                return response;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<GetBookedTicketResponse> GetBookedTicketDetail(int bookedTicketId)
        {
            // Ambil data BookedTicket beserta detailnya
            var bookedTicket = await _db.BookedTickets
                .Include(bt => bt.BookedTicketDetails)
                    .ThenInclude(btd => btd.Ticket)
                        .ThenInclude(t => t.Category)
                .FirstOrDefaultAsync(bt => bt.BookedTicketId == bookedTicketId);

            // Validasi apakah BookedTicketId ada
            if (bookedTicket == null)
            {
                throw new KeyNotFoundException($"BookedTicketId {bookedTicketId} tidak terdaftar");
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
                            TicketAmmount = btd.Quantity

                        }).ToList()
                    }).ToList()
            };

            return response;
        }
        public async Task<GetAllBookedTicketsResponse> GetAllBookedTickets()
        {
            var bookedTickets = await _db.BookedTickets
                .Include(bt => bt.BookedTicketDetails)
                .OrderByDescending(bt => bt.BookingDate)
                .Select(bt => new BookedTicketSummary
                {
                    BookedTicketId = bt.BookedTicketId,
                    BookingDate = bt.BookingDate,
                    TotalTickets = bt.BookedTicketDetails.Sum(btd => btd.Quantity)
                }).ToListAsync();
            return new GetAllBookedTicketsResponse
            {
                BookedTickets = bookedTickets
            };

        }
        public async Task<RevokeTicketResponse> RevokeTicket(int bookedTicketId, string ticketCode, int quantity)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Validasi adanya BookedTicketId
                var bookedTicket = await _db.BookedTickets
                    .Include(bt => bt.BookedTicketDetails)
                        .ThenInclude(btd => btd.Ticket)
                            .ThenInclude(t => t.Category)
                    .FirstOrDefaultAsync(bt => bt.BookedTicketId == bookedTicketId);

                if (bookedTicket == null)
                {
                    throw new KeyNotFoundException($"BookedTicketId {bookedTicketId} tidak terdaftar");
                }

                // Validasi Kode Ticket dalam Booking
                var bookedDetail = bookedTicket.BookedTicketDetails
                    .FirstOrDefault(btd => btd.Ticket.TicketCode == ticketCode);

                if (bookedDetail == null)
                {
                    throw new KeyNotFoundException($"Kode tiket {ticketCode} tidak terdaftar dalam BookedTicketId {bookedTicketId}");
                }

                // Validasi Quantity harus <= quantity yang sudah dibooking
                if (quantity > bookedDetail.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Quantity yang akan direvoke ({quantity}) melebihi quantity yang dibooking ({bookedDetail.Quantity}) untuk tiket {ticketCode}");
                }

                // Update RemainingQuota di tabel Tickets (mengembalikan quota)
                var ticket = bookedDetail.Ticket;
                ticket.RemainingQuota += quantity;
                ticket.UpdatedAt = DateTimeOffset.Now;

                // Update atau hapus BookedTicketDetail
                if (quantity == bookedDetail.Quantity)
                {
                    // Hapus row jika quantity menjadi 0
                    _db.BookedTicketDetails.Remove(bookedDetail);
                }
                else
                {
                    // Update quantity jika masih ada sisa
                    bookedDetail.Quantity -= quantity;
                    bookedDetail.Price = ticket.Price * bookedDetail.Quantity;
                    bookedDetail.UpdatedAt = DateTimeOffset.Now;
                }

                await _db.SaveChangesAsync();

                // Cek apakah masih ada detail lain di BookedTicket ini
                var remainingDetails = await _db.BookedTicketDetails
                    .Include(btd => btd.Ticket)
                        .ThenInclude(t => t.Category)
                    .Where(btd => btd.BookedTicketId == bookedTicketId)
                    .ToListAsync();

                // Jika tidak ada detail lagi, hapus BookedTicket
                if (!remainingDetails.Any())
                {
                    _db.BookedTickets.Remove(bookedTicket);
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Response : tampilkan sisa tiket yang masih dibooking
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
                await transaction.RollbackAsync();
                throw;
            }

        }
        public async Task<EditBookedTicketResponse> EditBookedTicket(int bookedTicketId, EditBookedTicketRequest request)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Validasi adanya BookedTicketId
                var bookedTicket = await _db.BookedTickets
                    .Include(bt => bt.BookedTicketDetails)
                    .ThenInclude(btd => btd.Ticket)
                    .ThenInclude(t => t.Category)
                    .FirstOrDefaultAsync(bt => bt.BookedTicketId == bookedTicketId);
                if(bookedTicket == null)
                {
                    throw new KeyNotFoundException($"BookedTicketId {bookedTicketId} tidak terdaftar");
                }
                var editedTickets = new List<EditedTicketInfo>();
                foreach (var item in request.Items)
                {
                    // Validasi Kode Ticket dalam Booking
                    var bookedDetail = bookedTicket.BookedTicketDetails
                        .FirstOrDefault(btd => btd.Ticket.TicketCode == item.TicketCode);
                    if (bookedDetail == null)
                    {
                        throw new KeyNotFoundException($"Kode tiket {item.TicketCode} tidak terdaftar dalam BookedTicketId {bookedTicketId}");
                    }

                    // Validasi Quantity tidak boleh kurang dari 1
                    if (item.Quantity < 1)
                    {
                        throw new InvalidOperationException($"Quantity tidak boleh kurang dari 0 untuk tiket {item.TicketCode}");
                    }

                    // Max Ticket Quantity yang boleh di booking
                    var ticket = bookedDetail.Ticket;
                    var maxAllowedQuantity = bookedDetail.Quantity + ticket.RemainingQuota;

                    // Validasi Quantitiy tidak boleh melebihi sisa quota
                    if (item.Quantity > maxAllowedQuantity)
                    {
                        throw new InvalidOperationException($"Quantity untuk {item.TicketCode} melebihi sisa quota. Maksimal yang diperbolehkan: {maxAllowedQuantity}, diminta: {item.Quantity}");
                    }

                    // perubahan quantity
                    var quantityDif = item.Quantity - bookedDetail.Quantity;

                    // update RemainingQuota
                    // Jika quantity naik, quota berkurang, vice versa
                    ticket.RemainingQuota -= quantityDif;
                    ticket.UpdatedAt = DateTimeOffset.Now;

                    // Update BookedTicketDetail
                    bookedDetail.Quantity = item.Quantity;
                    bookedDetail.Price = ticket.Price * bookedDetail.Quantity;
                    bookedDetail.UpdatedAt = DateTimeOffset.Now;

                    // Tambahkan ke list untuk response
                    editedTickets.Add(new EditedTicketInfo
                    {
                        TicketCode = ticket.TicketCode,
                        TicketName = ticket.TicketName,
                        CategoryName = ticket.Category.CategoryName,
                        RemainingQuantity = ticket.RemainingQuota
                    });
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new EditBookedTicketResponse
                {
                    EditedTickets = editedTickets
                };

            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}