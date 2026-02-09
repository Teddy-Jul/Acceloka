namespace Acceloka.Model
{
    public class AvailableTicketResponse
    {
        public string NamaKategori { get; set; } = string.Empty;
        public string KodeTicket { get; set; } = string.Empty;
        public string NamaTicket { get; set; } = string.Empty;
        public DateTimeOffset TanggalEvent { get; set; }
        public decimal Harga { get; set; }
        public int SisaQuota { get; set; }
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
