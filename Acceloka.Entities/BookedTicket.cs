using System;
using System.Collections.Generic;

namespace Acceloka.Entities;

public partial class BookedTicket
{
    public int BookedTicketId { get; set; }

    public DateTimeOffset BookingDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<BookedTicketDetail> BookedTicketDetails { get; set; } = new List<BookedTicketDetail>();

    public virtual User? User { get; set; }
}
