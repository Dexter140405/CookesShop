-- =============================================
-- Tạo Database
-- =============================================
CREATE DATABASE CookesShopDb;
GO

USE CookesShopDb;
GO

-- =============================================
-- 1. BẢNG IDENTITY (Tài khoản và Phân quyền)
-- =============================================
CREATE TABLE Roles (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL
);

CREATE TABLE Users (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    UserName NVARCHAR(256) NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL,
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(MAX) NULL,
    PhoneNumberConfirmed BIT NOT NULL,
    TwoFactorEnabled BIT NOT NULL,
    LockoutEnd DATETIMEOFFSET NULL,
    LockoutEnabled BIT NOT NULL,
    AccessFailedCount INT NOT NULL,
    
    -- Custom fields (Các trường mở rộng theo yêu cầu)
    FullName NVARCHAR(256) NULL,
    Address NVARCHAR(500) NULL,
    Points INT NOT NULL DEFAULT 0,
    Tier NVARCHAR(50) NOT NULL DEFAULT 'Guest' -- Guest, Silver, Gold
);

CREATE TABLE UserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);

-- (Lưu ý: ASP.NET Core Identity còn một số bảng như AspNetUserClaims, AspNetRoleClaims, AspNetUserLogins, AspNetUserTokens nhưng tôi đã rút gọn lại để tập trung vào các bảng chính).

-- =============================================
-- 2. BẢNG CATEGORY (Danh mục)
-- =============================================
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    Slug NVARCHAR(256) NOT NULL UNIQUE,
    Description NVARCHAR(MAX) NULL
);

-- =============================================
-- 3. BẢNG PRODUCT (Sản phẩm)
-- =============================================
CREATE TABLE Products (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CategoryId INT NOT NULL,
    Name NVARCHAR(256) NOT NULL,
    Slug NVARCHAR(256) NOT NULL UNIQUE,
    Description NVARCHAR(MAX) NULL,
    Ingredients NVARCHAR(MAX) NULL,
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    IsHot BIT NOT NULL DEFAULT 0,
    ImageUrl NVARCHAR(500) NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE
);

-- =============================================
-- 4. BẢNG CART & CARTITEM (Giỏ hàng)
-- =============================================
CREATE TABLE Carts (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(450) NULL, -- NULL nếu là khách (guest)
    SessionId NVARCHAR(256) NULL, -- Dùng để lưu giỏ hàng cho khách chưa đăng nhập (Session)
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);

CREATE TABLE CartItems (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CartId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    FOREIGN KEY (CartId) REFERENCES Carts(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

-- =============================================
-- 5. BẢNG PROMOTION (Khuyến mãi)
-- =============================================
CREATE TABLE Promotions (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    DiscountPercentage DECIMAL(5,2) NOT NULL,
    MaxDiscountAmount DECIMAL(18,2) NULL,
    ExpiryDate DATETIME2 NOT NULL
);

-- =============================================
-- 6. BẢNG ORDER & ORDERITEM (Đơn hàng)
-- =============================================
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(450) NULL,
    OrderDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Delivering, Completed, Cancelled
    PaymentMethod NVARCHAR(50) NOT NULL, -- COD, Online
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Unpaid', -- Unpaid, Paid, Refunded
    TotalAmount DECIMAL(18,2) NOT NULL,
    PromotionId INT NULL,
    
    -- Thông tin giao hàng lưu trực tiếp trên đơn
    ShippingAddress NVARCHAR(500) NOT NULL,
    CustomerName NVARCHAR(256) NOT NULL,
    PhoneNumber NVARCHAR(50) NOT NULL,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (PromotionId) REFERENCES Promotions(Id) ON DELETE SET NULL
);

CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL, -- Lưu giá lúc mua
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

-- =============================================
-- 7. BẢNG REVIEW (Đánh giá)
-- =============================================
CREATE TABLE Reviews (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId NVARCHAR(450) NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment NVARCHAR(MAX) NULL,
    IsApproved BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- =============================================
-- 8. BẢNG SUPPORT TICKETS & Q&A (Hỗ trợ và Hỏi đáp)
-- =============================================
CREATE TABLE SupportTickets (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(450) NULL,
    Subject NVARCHAR(256) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Open', -- Open, InProgress, Closed
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);

CREATE TABLE QuestionsAndAnswers (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId NVARCHAR(450) NULL,
    Question NVARCHAR(MAX) NOT NULL,
    Answer NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    AnsweredAt DATETIME2 NULL,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);