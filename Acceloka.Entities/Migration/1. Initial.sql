create database acceloka
go

use acceloka
go

drop table BookedTicketDetails
drop table BookedTickets
drop table Categories
drop table Tickets
drop table Users

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

create table Users(
	UserId int identity(1,1) primary key not null,
	Username varchar(255) not null,
	Email varchar(255) not null,
	Password varchar(255) not null,
	CreatedAt datetimeoffset not null default sysdatetimeoffset(),
	UpdatedAt datetimeoffset null,

	constraint UQ_Users_Username unique(Username)
)


alter table BookedTickets
	add UserId int null
		constraint FK_BookedTickets_Users foreign key references Users(UserId)


INSERT INTO Categories (CategoryName) VALUES
('Bioskop'),   -- 1
('Konser'),    -- 2
('Kereta'),    -- 3
('Kapal Laut'),-- 4
('Pesawat'),   -- 5
('Hotel');     -- 6
GO

INSERT INTO Tickets (CategoryId, TicketCode, TicketName, EventDate, Price, Quota, RemainingQuota) VALUES

-- Bioskop (CategoryId=1)
(1, 'BIO001', 'Avatar 3 - IMAX',          '2026-06-15T19:00:00+07:00', 75000.00,  100, 100),  -- AVAILABLE
(1, 'BIO002', 'Spiderman 5 - Regular',    '2026-06-20T14:00:00+07:00', 50000.00,  150, 150),  -- AVAILABLE
(1, 'BIO003', 'Avengers 6 - Premiere',    '2026-07-04T21:00:00+07:00', 120000.00, 50,  2),    -- LOW STOCK (only 2 left)
(1, 'BIO004', 'Dune 3 - 4DX',             '2026-05-10T18:00:00+07:00', 95000.00,  80,  0),    -- SOLD OUT
(1, 'BIO005', 'Deadpool 4 - Regular',     '2025-12-25T20:00:00+07:00', 50000.00,  200, 200),  -- PAST EVENT

-- Konser (CategoryId=2)
(2, 'KON001', 'Rock Festival Jakarta',    '2026-08-15T19:00:00+07:00', 500000.00, 5000, 5000),-- AVAILABLE
(2, 'KON002', 'Jazz Night Bandung',       '2026-09-20T20:00:00+07:00', 350000.00, 1000, 1000),-- AVAILABLE
(2, 'KON003', 'K-Pop Night Jakarta',      '2026-07-30T18:00:00+07:00', 750000.00, 2000, 3),   -- LOW STOCK (only 3 left)
(2, 'KON004', 'EDM Midnight Bali',        '2026-10-31T22:00:00+07:00', 600000.00, 3000, 0),   -- SOLD OUT
(2, 'KON005', 'Coldplay World Tour',      '2025-11-01T19:00:00+07:00', 1500000.00,10000,10000),-- PAST EVENT

-- Kereta (CategoryId=3)
(3, 'KRL001', 'Jakarta - Bandung Argo',   '2026-04-10T06:00:00+07:00', 150000.00, 300,  300), -- AVAILABLE
(3, 'KRL002', 'Jakarta - Surabaya Bima',  '2026-04-11T12:00:00+07:00', 250000.00, 250,  250), -- AVAILABLE
(3, 'KRL003', 'Yogyakarta - Solo Prameks','2026-04-12T08:00:00+07:00', 25000.00,  400,  1),   -- LOW STOCK (only 1 left)
(3, 'KRL004', 'Jakarta - Semarang Tawang','2026-04-13T07:00:00+07:00', 180000.00, 200,  0),   -- SOLD OUT
(3, 'KRL005', 'Bandung - Jakarta Parahyangan','2026-01-15T09:00:00+07:00',100000.00,350,350), -- PAST EVENT

-- Kapal Laut (CategoryId=4)
(4, 'KPL001', 'Jakarta - Batam Ferry',    '2026-05-01T08:00:00+07:00', 400000.00, 500,  500), -- AVAILABLE
(4, 'KPL002', 'Surabaya - Makassar PELNI','2026-05-15T22:00:00+07:00', 650000.00, 800,  800), -- AVAILABLE
(4, 'KPL003', 'Merak - Bakauheni Ferry',  '2026-04-05T06:00:00+07:00', 50000.00,  1000, 4),   -- LOW STOCK (only 4 left)
(4, 'KPL004', 'Jakarta - Belitung Ferry', '2026-04-20T10:00:00+07:00', 350000.00, 200,  0),   -- SOLD OUT

-- Pesawat (CategoryId=5)
(5, 'PSW001', 'Jakarta - Bali (Garuda)',  '2026-05-20T07:00:00+07:00', 1500000.00,180,  180), -- AVAILABLE
(5, 'PSW002', 'Jakarta - Medan (Lion)',   '2026-05-21T09:00:00+07:00', 1200000.00,200,  200), -- AVAILABLE
(5, 'PSW003', 'Jakarta - Lombok (Citilink)','2026-06-01T06:00:00+07:00',900000.00,160,  2),   -- LOW STOCK (only 2 left)
(5, 'PSW004', 'Bali - Labuan Bajo',       '2026-06-10T08:00:00+07:00', 750000.00, 100,  0),   -- SOLD OUT
(5, 'PSW005', 'Jakarta - Yogyakarta',     '2026-01-10T10:00:00+07:00', 800000.00, 180,  180), -- PAST EVENT

-- Hotel (CategoryId=6)
(6, 'HTL001', 'Hotel Mulia Jakarta (1 Malam)',  '2026-05-25T14:00:00+07:00', 2000000.00, 50, 50), -- AVAILABLE
(6, 'HTL002', 'Hotel Santika Bali (1 Malam)',   '2026-06-10T14:00:00+07:00', 800000.00,  100,100), -- AVAILABLE
(6, 'HTL003', 'The Ritz-Carlton Bali (1 Malam)','2026-07-01T14:00:00+07:00', 5000000.00, 20, 1),  -- LOW STOCK (only 1 left)
(6, 'HTL004', 'Hotel Tentrem Yogyakarta',       '2026-06-20T14:00:00+07:00', 1200000.00, 60, 0),  -- SOLD OUT
(6, 'HTL005', 'Hotel Indonesia Heritage',       '2026-01-20T14:00:00+07:00', 1500000.00, 80, 80); -- PAST EVENT
GO

-- ============================================================
-- USERS
-- All passwords = "qwerty"
-- test1 / test2 → normal users to test isolation
-- ============================================================
INSERT INTO Users (Username, Email, Password) VALUES
('admin',   'admin@acceloka.com',   'qwerty'),  -- UserId=1
('test1',   'test1@acceloka.com',   'qwerty'),  -- UserId=2
('test2',   'test2@acceloka.com',   'qwerty');  -- UserId=3
GO

select * from Categories
select * from Tickets
select * from BookedTickets
select * from BookedTicketDetails
select * from Users