using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Acceloka.Entities;

public partial class AccelokaContext : DbContext
{
    public AccelokaContext()
    {
    }

    public AccelokaContext(DbContextOptions<AccelokaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BookedTicket> BookedTickets { get; set; }

    public virtual DbSet<BookedTicketDetail> BookedTicketDetails { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookedTicket>(entity =>
        {
            entity.HasKey(e => e.BookedTicketId).HasName("PK__BookedTi__9110472FD5942B25");

            entity.Property(e => e.BookingDate).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
        });

        modelBuilder.Entity<BookedTicketDetail>(entity =>
        {
            entity.HasKey(e => e.BookedTicketDetailId).HasName("PK__BookedTi__3C836D93707089B9");

            entity.HasIndex(e => new { e.BookedTicketId, e.TicketId }, "UQ_BookedTicketDetails_BookedTicket_Ticket").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.BookedTicket).WithMany(p => p.BookedTicketDetails)
                .HasForeignKey(d => d.BookedTicketId)
                .HasConstraintName("FK_BookedTicketDetails_BookedTickets");

            entity.HasOne(d => d.Ticket).WithMany(p => p.BookedTicketDetails)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookedTicketDetails_Tickets");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B6E3E0A4E");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Tickets__712CC607929D3E9A");

            entity.HasIndex(e => e.TicketCode, "UQ__Tickets__23DC808FFD0F6FBB").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TicketCode).HasMaxLength(50);
            entity.Property(e => e.TicketName).HasMaxLength(200);

            entity.HasOne(d => d.Category).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_Categories");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
