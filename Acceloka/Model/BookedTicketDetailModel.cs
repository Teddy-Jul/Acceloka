namespace Acceloka.Model
{
    // GET/api/v1/get-booked-ticket/bookedticketid
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
        public int TicketAmount { get; set; }
    }
    // Get /api/v1/get-all-booked-tickets
    public class GetAllBookedTicketsResponse
    {
        public List<BookedTicketSummary> BookedTickets { get; set; } = new List<BookedTicketSummary>();
    }
    public class BookedTicketSummary
    {
        public int BookedTicketId { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public int TotalTickets { get; set; }
    }

    // Delete /api/v1/revoke-booked-ticket/bookedticketid
    public class RevokeTicketResponse 
    {
        public List<RevokeTicketItem> RemainingTicket { get; set; } = new List<RevokeTicketItem>();
    }

    public class RevokeTicketItem
    {
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int RemainingQuantity { get; set; }
    }

    // Put /api/v1/edit-booked-ticket/bookedticketid
    public class EditBookedTicketRequest
    {
        public List<EditBookedTicketItem> Items { get; set; } = new List<EditBookedTicketItem>();
    }
    public class EditBookedTicketItem
    {
        public string TicketCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
    public class EditBookedTicketResponse
    {
        public List<EditedTicketInfo> EditedTickets { get; set; } = new List<EditedTicketInfo>();
    }
    public class EditedTicketInfo
    {
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int RemainingQuantity { get; set; }
    }
}
