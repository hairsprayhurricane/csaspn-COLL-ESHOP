-- Create the database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'OnlineStore')
BEGIN
    CREATE DATABASE OnlineStore;
END
GO

USE OnlineStore;
GO

-- Create Categories table
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL
);

-- Create Products table
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    Price DECIMAL(18,2) NOT NULL,
    ImageUrl NVARCHAR(255),
    CategoryId INT,
    Stock INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- Create CartItems table
CREATE TABLE CartItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    SessionId NVARCHAR(255),
    UserId NVARCHAR(450),
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Create Orders table
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(450),
    OrderDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending'
);

-- Create OrderItems table
CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Insert sample categories
INSERT INTO Categories ([Name]) VALUES 
('Electronics'),
('Clothing'),
('Books'),
('Home & Garden');

-- Insert sample products
INSERT INTO Products ([Name], [Description], Price, ImageUrl, CategoryId, Stock) VALUES 
('Smartphone X', 'Latest smartphone with great camera', 999.99, '/images/smartphone-x.jpg', 1, 50),
('Laptop Pro', 'Powerful laptop for professionals', 1499.99, '/images/laptop-pro.jpg', 1, 30),
('Wireless Earbuds', 'Noise cancelling wireless earbuds', 199.99, '/images/earbuds.jpg', 1, 100),
('Cotton T-Shirt', 'Comfortable cotton t-shirt', 29.99, '/images/tshirt.jpg', 2, 200),
('Programming Book', 'Learn C# in 30 days', 39.99, '/images/programming-book.jpg', 3, 75),
('Garden Chair', 'Comfortable outdoor chair', 59.99, '/images/garden-chair.jpg', 4, 45);

-- Create indexes for better performance
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_CartItems_UserId ON CartItems(UserId);
CREATE INDEX IX_CartItems_SessionId ON CartItems(SessionId);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);