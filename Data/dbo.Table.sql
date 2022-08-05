CREATE TABLE [dbo].[Deals]
(
	[Id] INT NOT NULL IDENTITY PRIMARY KEY, 
    [Number] NVARCHAR(50) NOT NULL, 
    [Date] NVARCHAR(10) NOT NULL, 
    [VolumeBuyer] FLOAT NOT NULL , 
    [VolumeSeller] FLOAT NOT NULL, 
    [BuyerId] INT NOT NULL, 
    [SellerId] INT NOT NULL, 
    CONSTRAINT [FK_Deal_Buyer] FOREIGN KEY ([BuyerId]) REFERENCES [Сontractors]([Id]), 
	CONSTRAINT [FK_Deal_Seller] FOREIGN KEY ([SellerId]) REFERENCES [Сontractors]([Id]) 
)
