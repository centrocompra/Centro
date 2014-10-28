IF not EXISTS (SELECT * FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[Shop]'))
CREATE FULLTEXT INDEX ON [dbo].[Shop](
[ShopName] LANGUAGE [English])
KEY INDEX [PK_Shop_ShopID]ON ([Cities], FILEGROUP [PRIMARY])
WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM)
GO

IF not EXISTS (SELECT * FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[Users]'))
CREATE FULLTEXT INDEX ON [dbo].[Users](
[UserName] LANGUAGE [English])
KEY INDEX [PK_Users_UserID]ON ([Cities], FILEGROUP [PRIMARY])
WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM)
GO

CREATE FULLTEXT INDEX ON [dbo].[Products](
[Description] LANGUAGE [English], 
[Materials] LANGUAGE [English], 
[Tags] LANGUAGE [English], 
[Title] LANGUAGE [English])
KEY INDEX [PK_Products_ProductID]ON ([Cities], FILEGROUP [PRIMARY])
WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM)
GO

IF not EXISTS (SELECT * FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[StateProvince]'))
CREATE FULLTEXT INDEX ON [dbo].[StateProvince](
[StateName] LANGUAGE [English])
KEY INDEX [PK_StateProvince_StateID]ON ([Cities], FILEGROUP [PRIMARY])
WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM)
GO

IF not EXISTS (SELECT * FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[Country]'))
CREATE FULLTEXT INDEX ON [dbo].[Country](
[CountryName] LANGUAGE [English])
KEY INDEX [PK_Country_CountryID]ON ([Cities], FILEGROUP [PRIMARY])
WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM)
GO

IF not EXISTS (SELECT * FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[Cities]'))
CREATE FULLTEXT INDEX ON [dbo].[Cities](
[CityName] LANGUAGE [English])
KEY INDEX [PK_Cities_CityID]ON ([Cities], FILEGROUP [PRIMARY])
WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM)
GO

Create Table Deals
(
	DealID int primary key identity(1,1),
	Title varchar(250) not null,
	SubTitle varchar(500),
	IsActive bit not null
)

Create table DealItems
(
	DealItemID int primary key identity(1,1),
	DealID int foreign key references Deals(DealID) on delete cascade,
	Title varchar(250) not null,
	CategoryID int foreign key references Categories(CategoryID),
	Picture varchar(200),
	IsActive bit not null
)

Create table DealItemProducts
(
	DealItemProductID int primary key identity(1,1),
	ProductID int foreign key references Products(ProductID) on delete cascade,
	DealID int foreign key references Deals(DealID) on delete cascade,	
	DealItemID int foreign key references DealItems(DealItemID)
)