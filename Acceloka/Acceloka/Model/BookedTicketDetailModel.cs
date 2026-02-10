namespace Acceloka.Model
{
    public class GetBookedTicketResponse
    {
        public List<BookedTicketCategoryGroup> Categories { get; set; } = new List<BookedTicketCategoryGroup>();
    }

    public class BookedTicketCategoryGroup
    {
        public int QtyPerCategory { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<BookedTicketDetailInfo> Tickets { get; set; } = new List<BookedTicketDetailInfo>();
    }

    public class BookedTicketDetailInfo
    {
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public DateTimeOffset EventDate { get; set; }
        public int TicketAmmount { get; set; }
    }

    public class GetAllBookedTicketsResponse
    {
        public List<BookedTicketSummary> BookedTickets { get; set; } = new List<BookedTicketSummary>();
    }
    public class BookedTicketSummary
    {
        public int BookedTicketId { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public int TotalTickets { get; set; }
        public int TotalQuantity { get; set; }
    }
}
