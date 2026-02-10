namespace Acceloka.Model
{
    public class BookTicketRequest
    {
        public List<BookTicketItem> Items { get; set; } = new List<BookTicketItem>();
    }
    public class BookTicketItem
    {
        public string TicketCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
    public class  BookTicketResponse
    {
        public decimal TotalPrice { get; set; } // Total keseluruhan
        public List<TicketPerCategory> TicketsPerCategory { get; set; } = new List<TicketPerCategory>();
    }
    public class TicketPerCategory
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal SubtotalPrice { get; set; } // Total per kategori
        public List<BookedTicketInfo> Tickets { get; set; } = new List<BookedTicketInfo>();
    }
    public class BookedTicketInfo
    {
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public decimal Price { get; set; } // Harga * Quantity
    }
}
