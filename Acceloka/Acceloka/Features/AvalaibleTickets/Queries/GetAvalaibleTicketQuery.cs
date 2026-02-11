using Acceloka.Model;
using MediatR;

namespace Acceloka.Features.AvalaibleTickets.Queries
{
    public class GetAvailableTicketsQuery : IRequest<PagedResult<AvailableTicketResponse>>
    {
        public string? CategoryName { get; set; }
        public string? TicketCode { get; set; }
        public string? TicketName { get; set; }
        public decimal? Price { get; set; }
        public DateTimeOffset? MinEventDate { get; set; }
        public DateTimeOffset? MaxEventDate { get; set; }
        public string OrderBy { get; set; } = "TicketCode";
        public string OrderState { get; set; } = "ascending";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
