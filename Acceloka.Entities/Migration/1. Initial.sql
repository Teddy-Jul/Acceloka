create database acceloka
go

use acceloka
go

create table Categories(
	CategoryId int identity(1,1) primary key not null,
	CategoryName varchar(255) not null,
	CreatedAt Datetimeoffset not null default sysdatetimeoffset(), 
	UpdatedAt Datetimeoffset null

	constraint UQ_Categories_CategoryName unique(CategoryName)
)

create table Tickets(
	TicketId int identity(1,1) primary key not null,
	CategoryId int not null
	constraint FK_Tickets_Categories foreign key references Categories(CategoryId),
	TicketCode varchar(255) not null, 
	TicketName varchar(255) not null,
	EventDate datetimeoffset not null,
	Price decimal(18,2) not null,
	Quota int not null,
	RemainingQuota int not null,
	CreatedAt Datetimeoffset not null default sysdatetimeoffset(), 
	UpdatedAt datetimeoffset null,

	constraint UQ_Tickets_TicketCode unique(TicketCode),
	constraint CK_Tickets_Price check(price >= 0),
	constraint CK_Tickets_Quota check(Quota >= 0),
	constraint CK_Tickets_RemainingQuota check(RemainingQuota >= 0),
	constraint CK_Tickets_RemainingQuota_LTE_Quota check(RemainingQuota <= Quota)
)

create table BookedTickets(
	BookedTicketId int identity(1,1) primary key not null,
	BookingDate datetimeoffset not null default sysdatetimeoffset(),
	CreatedAt datetimeoffset not null default sysdatetimeoffset(),
	UpdatedAt datetimeoffset null
)

create table BookedTicketDetails(
	BookedTicketDetailId int identity(1,1) not null primary key,
	BookedTicketId int not null
	constraint FK_BookedTicketDetails_BookedTickets foreign key references BookedTickets(BookedTicketId) on delete cascade,
	TicketId int not null
	constraint FK_BookedTicketDetails_Tickets foreign key references Tickets(TicketId),
	Quantity int not null,
	Price decimal(18,2) not null,
	CreatedAt datetimeoffset not null default sysdatetimeoffset(),
	UpdatedAt datetimeoffset null,

	constraint UQ_BookedTicketDetails_BookedTicket_Ticket unique(BookedTicketId,TicketId),
	constraint CK_BookedTicketDetails_Quantity check(Quantity > 0),
	constraint CK_BookedTicketDetails_Price check(price >= 0)
)

select * from Categories
select * from Tickets
select * from BookedTickets
select * from BookedTicketDetails

INSERT INTO Categories (CategoryName) VALUES 
('Bioskop'),
('Konser'),
('Kereta'),
('Kapal Laut'),
('Pesawat'),
('Hotel');

-- Insert Sample Tickets
INSERT INTO Tickets (CategoryId, TicketCode, TicketName, EventDate, Price, Quota, RemainingQuota) VALUES 
-- Bioskop
(1, 'BIO001', 'Avatar 3 - IMAX', '2026-03-15T19:00:00+07:00', 75000.00, 100, 100),
(1, 'BIO002', 'Spiderman 5 - Regular', '2026-03-20T14:00:00+07:00', 50000.00, 150, 150),

-- Konser
(2, 'KON001', 'Rock Festival Jakarta', '2026-06-15T19:00:00+07:00', 500000.00, 5000, 5000),
(2, 'KON002', 'Jazz Night Bandung', '2026-07-20T20:00:00+07:00', 350000.00, 1000, 1000),

-- Kereta
(3, 'KRL001', 'Jakarta - Bandung Express', '2026-02-15T06:00:00+07:00', 150000.00, 300, 300),
(3, 'KRL002', 'Jakarta - Surabaya', '2026-02-16T12:00:00+07:00', 250000.00, 250, 250),

-- Kapal Laut
(4, 'KPL001', 'Jakarta - Batam Ferry', '2026-03-01T08:00:00+07:00', 400000.00, 500, 500),

-- Pesawat
(5, 'PSW001', 'Jakarta - Bali (Garuda)', '2026-02-20T07:00:00+07:00', 1500000.00, 180, 180),
(5, 'PSW002', 'Jakarta - Medan (Lion)', '2026-02-21T09:00:00+07:00', 1200000.00, 200, 200),

-- Hotel
(6, 'HTL001', 'Hotel Mulia Jakarta', '2026-02-25T14:00:00+07:00', 2000000.00, 50, 50),
(6, 'HTL002', 'Hotel Santika Bali', '2026-03-10T14:00:00+07:00', 800000.00, 100, 100);
GO