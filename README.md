# Acceloka - Online Ticket Booking System

Acceloka adalah sistem booking tiket online berbasis ASP.NET Core 9 (C# 13) yang mendukung berbagai jenis tiket seperti bioskop, konser, kereta, kapal laut, pesawat, dan hotel.  
Sistem ini menerapkan MARVEL Pattern (MediatR, FluentValidation), error handling RFC 7807, logging Serilog, dan koneksi database async.

---

## Fitur Utama

- **GET /api/v1/get-available-ticket**  
  Lihat tiket yang masih tersedia, filter, search, order, dan pagination.
- **POST /api/v1/book-ticket**  
  Booking tiket dengan validasi kuota, tanggal, dsb.
- **GET /api/v1/get-booked-ticket/{BookedTicketId}**  
  Lihat detail tiket yang sudah dibooking.
- **DELETE /api/v1/revoke-ticket/{BookedTicketId}/{KodeTicket}/{Qty}**  
  Revoke/cancel sebagian/seluruh tiket yang sudah dibooking.
- **PUT /api/v1/edit-booked-ticket/{BookedTicketId}**  
  Edit quantity tiket yang sudah dibooking.

---

## Arsitektur & Teknologi

- **ASP.NET Core 9 (Web API)**
- **Entity Framework Core 9** (SQL Server)
- **MediatR** (CQRS & MARVEL Pattern)
- **FluentValidation** (Validasi request)
- **Serilog** (Logging ke file `/logs/Log-{tanggal}.txt`)
- **Hellang.Middleware.ProblemDetails** (RFC 7807 error response)
- **Swagger** (API documentation)
