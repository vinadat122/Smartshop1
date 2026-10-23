
CREATE DATABASE IF NOT EXISTS SMARTSHOP
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE SMARTSHOP;

CREATE TABLE IF NOT EXISTS Product (
    ProductId INT NOT NULL AUTO_INCREMENT,
    ProductName VARCHAR(128) NOT NULL,
    Category VARCHAR(64) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Features VARCHAR(512) NOT NULL,
    Description LONGTEXT NULL,
    PRIMARY KEY (ProductId)
);

CREATE TABLE IF NOT EXISTS Review (
    ReviewId INT NOT NULL AUTO_INCREMENT,
    ProductId INT NOT NULL,
    CustomerName VARCHAR(64) NOT NULL,
    Content VARCHAR(1000) NOT NULL,
    Rating INT NOT NULL,
    Sentiment VARCHAR(16) NOT NULL,
    Score DOUBLE NOT NULL,
    Summary VARCHAR(256) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    PRIMARY KEY (ReviewId),
    CONSTRAINT FK_Review_Product
        FOREIGN KEY (ProductId)
        REFERENCES Product(ProductId)
        ON DELETE CASCADE,
    CONSTRAINT CK_Review_Rating
        CHECK (Rating BETWEEN 1 AND 5)
);

CREATE TABLE IF NOT EXISTS ChatMessage (
    Id INT NOT NULL AUTO_INCREMENT,
    SessionId VARCHAR(100) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    Content LONGTEXT NOT NULL,
    CreatedAt DATETIME NOT NULL,
    PRIMARY KEY (Id),
    INDEX IX_ChatMessage_SessionId_CreatedAt (SessionId, CreatedAt)
);

-- Seed sample products only when the product name does not already exist.
INSERT INTO Product (ProductName, Category, Price, Features)
SELECT 'Samsung Galaxy S25', 'Smartphone', 799.00,
       'Smartphone; Galaxy S25 series'
WHERE NOT EXISTS (
    SELECT 1 FROM Product WHERE ProductName = 'Samsung Galaxy S25'
);

INSERT INTO Product (ProductName, Category, Price, Features)
SELECT 'MacBook Air M4', 'Laptop', 999.00,
       'Apple M4 chip; lightweight laptop'
WHERE NOT EXISTS (
    SELECT 1 FROM Product WHERE ProductName = 'MacBook Air M4'
);

INSERT INTO Product (ProductName, Category, Price, Features)
SELECT 'AirPods Pro 3', 'Audio', 249.00,
       'Wireless earbuds; noise cancellation'
WHERE NOT EXISTS (
    SELECT 1 FROM Product WHERE ProductName = 'AirPods Pro 3'
);

INSERT INTO Product (ProductName, Category, Price, Features)
SELECT 'Xiaomi Redmi Note 15', 'Smartphone', 299.00,
       'Redmi Note smartphone series'
WHERE NOT EXISTS (
    SELECT 1 FROM Product WHERE ProductName = 'Xiaomi Redmi Note 15'
);
