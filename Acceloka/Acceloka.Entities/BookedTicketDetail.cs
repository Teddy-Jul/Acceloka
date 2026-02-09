using System;
using System.Collections.Generic;

namespace Acceloka.Entities;

public partial class BookedTicketDetail
{
    public int BookedTicketDetailId { get; set; }

    public int BookedTicketId { get; set; }

    public int TicketId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual BookedTicket BookedTicket { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}
