using Acceloka.Entities;
using Acceloka.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Acceloka.Features.AvalaibleTickets.Queries
{
    public class GetAvailableTicketQueryHandler : IRequestHandler<GetAvailableTicketsQuery, PagedResult<AvailableTicketResponse>>
    {
        private readonly AccelokaContext _db;

        public GetAvailableTicketQueryHandler(AccelokaContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<AvailableTicketResponse>> Handle(GetAvailableTicketsQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Tickets.Include(t => t.Category).AsQueryable();
            query = query.Where(t => t.RemainingQuota > 0);
            // Filter
            if (!string.IsNullOrWhiteSpace(request.CategoryName))
            {
                query = query.Where(t => t.Category.CategoryName.Contains(request.CategoryName));
            }

            if (!string.IsNullOrWhiteSpace(request.TicketCode))
            {
                query = query.Where(t => t.TicketCode.Contains(request.TicketCode));
            }

            if (!string.IsNullOrWhiteSpace(request.TicketName))
            {
                query = query.Where(t => t.TicketName.Contains(request.TicketName));
            }

            if (request.Price.HasValue)
            {
                query = query.Where(t => t.Price <= request.Price.Value);
            }

            if (request.MinEventDate.HasValue)
            {
                query = query.Where(t => t.EventDate >= request.MinEventDate.Value);
            }

            if (request.MaxEventDate.HasValue)
            {
                query = query.Where(t => t.EventDate <= request.MaxEventDate.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            if(totalCount == 0)
            {
                throw new KeyNotFoundException("No tickets found");
            }

            // Apply ordering
            query = ApplyOrdering(query, request.OrderBy, request.OrderState);

            // pagination skip page 1 1-10 , page 2 11-20
            var skip = (request.Page - 1) * request.PageSize;
            query = query.Skip(skip).Take(request.PageSize);

            // Execute query
            var tickets = await query.ToListAsync(cancellationToken);

            // response data tickets
            var data = tickets.Select(t => new AvailableTicketResponse
            {
                CategoryName = t.Category.CategoryName,
                TicketCode = t.TicketCode,
                TicketName = t.TicketName,
                EventDate = t.EventDate,
                Price = t.Price,
                RemainingQuota = t.RemainingQuota
            }).ToList();

            // menghitung total pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new PagedResult<AvailableTicketResponse>
            {
                Data = data,
                CurrentPage = request.Page,
                TotalPages = totalPages,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        private IQueryable<Ticket> ApplyOrdering(IQueryable<Ticket> query, string orderBy, string orderState)
        {
            var isDescending = orderState.ToLower() == "descending";

            return orderBy.ToLower() switch
            {
                "categoryname" => isDescending
                    ? query.OrderByDescending(t => t.Category.CategoryName)
                    : query.OrderBy(t => t.Category.CategoryName),
                "ticketcode" => isDescending
                    ? query.OrderByDescending(t => t.TicketCode)
                    : query.OrderBy(t => t.TicketCode),
                "ticketname" => isDescending
                    ? query.OrderByDescending(t => t.TicketName)
                    : query.OrderBy(t => t.TicketName),
                "eventdate" => isDescending
                    ? query.OrderByDescending(t => t.EventDate)
                    : query.OrderBy(t => t.EventDate),
                "price" => isDescending
                    ? query.OrderByDescending(t => t.Price)
                    : query.OrderBy(t => t.Price),
                "remainingquota" => isDescending
                    ? query.OrderByDescending(t => t.RemainingQuota)
                    : query.OrderBy(t => t.RemainingQuota),
                _ => query.OrderBy(t => t.TicketCode)
            };
        }
    }
}