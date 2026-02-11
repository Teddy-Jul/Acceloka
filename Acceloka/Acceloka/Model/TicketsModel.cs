namespace Acceloka.Model
{
    // GET /api/v1/get-avalaible-ticket
    public class AvailableTicketResponse
    {
        public string CategoryName { get; set; } = string.Empty;
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public DateTimeOffset EventDate { get; set; }
        public decimal Price { get; set; }
        public int RemainingQuota { get; set; }
    }

    // Response dengan Pagination
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}
