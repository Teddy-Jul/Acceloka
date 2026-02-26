using System;
using System.Collections.Generic;

namespace Acceloka.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string Email { get; set; } = null!;

    public virtual ICollection<BookedTicket> BookedTickets { get; set; } = new List<BookedTicket>();
}
