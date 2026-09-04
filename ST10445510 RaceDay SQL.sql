/*
    RaceDay Database Script
    Programming 2B (PROG6212) - PoE Part 1
    Student: Mduduzi Ditlhokwa
    Student Number: ST10445510
    Course: Diploma in IT in Software Development
    Course Code: DISD0601
*/

-- Create the database
IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END;
GO

USE RaceDayDB;
GO

-- Drop tables if they already exist so the script can be re-run
DROP TABLE IF EXISTS Result;
DROP TABLE IF EXISTS Enrolment;
DROP TABLE IF EXISTS Weather;
DROP TABLE IF EXISTS Route;
DROP TABLE IF EXISTS Category;
DROP TABLE IF EXISTS Event;
DROP TABLE IF EXISTS [User];
GO

-- =========================
-- USER
-- Stores Organisers and Participants
-- =========================
CREATE TABLE [User] (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    [Password] NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    [Role] NVARCHAR(20) NOT NULL
        CONSTRAINT CK_User_Role CHECK ([Role] IN ('Organiser', 'Participant'))
);
GO

-- =========================
-- EVENT
-- An Organiser can create many Events
-- =========================
CREATE TABLE Event (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName NVARCHAR(150) NOT NULL,
    EventDate DATE NOT NULL,
    Location NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL,

    CONSTRAINT FK_Event_User
        FOREIGN KEY (OrganiserID) REFERENCES [User](UserID)
);
GO

-- =========================
-- CATEGORY
-- An Event can have many Categories
-- =========================
CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    Distance DECIMAL(6,2) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    MaximumParticipants INT NOT NULL DEFAULT 100,

    CONSTRAINT FK_Category_Event
        FOREIGN KEY (EventID) REFERENCES Event(EventID),

    CONSTRAINT UQ_Category_Event_Name
        UNIQUE (EventID, CategoryName),

    CONSTRAINT CK_Category_Distance
        CHECK (Distance > 0),

    CONSTRAINT CK_Category_EntryFee
        CHECK (EntryFee >= 0),

    CONSTRAINT CK_Category_MaxParticipants
        CHECK (MaximumParticipants > 0)
);
GO

-- =========================
-- ENROLMENT
-- Links Participants to Categories
-- =========================
CREATE TABLE Enrolment (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Confirmed',

    CONSTRAINT FK_Enrolment_Participant
        FOREIGN KEY (ParticipantID) REFERENCES [User](UserID),

    CONSTRAINT FK_Enrolment_Category
        FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID),

    CONSTRAINT UQ_Enrolment_Participant_Category
        UNIQUE (ParticipantID, CategoryID),

    CONSTRAINT CK_Enrolment_Status
        CHECK ([Status] IN ('Pending', 'Confirmed', 'Cancelled'))
);
GO

-- =========================
-- RESULT
-- An Enrolment can have zero or one Result
-- =========================
CREATE TABLE Result (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME(0) NULL,
    FinishingPosition INT NULL,

    CONSTRAINT FK_Result_Enrolment
        FOREIGN KEY (EnrolmentID) REFERENCES Enrolment(EnrolmentID),

    CONSTRAINT CK_Result_Position
        CHECK (FinishingPosition IS NULL OR FinishingPosition > 0)
);
GO

-- =========================
-- ROUTE
-- Stores route information for an Event
-- =========================
CREATE TABLE Route (
    RouteID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL UNIQUE,
    RouteName NVARCHAR(150) NOT NULL,
    Distance DECIMAL(6,2) NOT NULL,
    Description NVARCHAR(500) NULL,
    MapURL NVARCHAR(500) NULL,

    CONSTRAINT FK_Route_Event
        FOREIGN KEY (EventID) REFERENCES Event(EventID),

    CONSTRAINT CK_Route_Distance
        CHECK (Distance > 0)
);
GO

-- =========================
-- WEATHER
-- Stores weather records for an Event
-- =========================
CREATE TABLE Weather (
    WeatherID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    WeatherDate DATE NOT NULL,
    Temperature DECIMAL(5,2) NULL,
    [Condition] NVARCHAR(100) NOT NULL,
    WindSpeed DECIMAL(6,2) NULL,

    CONSTRAINT FK_Weather_Event
        FOREIGN KEY (EventID) REFERENCES Event(EventID),

    CONSTRAINT CK_Weather_WindSpeed
        CHECK (WindSpeed IS NULL OR WindSpeed >= 0)
);
GO

-- =========================================================
-- SEED DATA
-- Minimum required:
-- 2 Organisers
-- 2 Participants
-- 3 Events
-- Categories for every Event
-- Sample Enrolments
-- =========================================================

-- Users
INSERT INTO [User] (FirstName, LastName, Email, [Password], PhoneNumber, [Role])
VALUES
('Thabo', 'Mokoena', 'thabo.mokoena@raceday.co.za', 'Organiser@123', '0711111111', 'Organiser'),
('Lerato', 'Mahlangu', 'lerato.mahlangu@raceday.co.za', 'Organiser@456', '0722222222', 'Organiser'),
('Sipho', 'Dlamini', 'sipho.dlamini@example.com', 'Participant@123', '0733333333', 'Participant'),
('Naledi', 'Molefe', 'naledi.molefe@example.com', 'Participant@456', '0744444444', 'Participant');
GO

-- Events
INSERT INTO Event (OrganiserID, EventName, EventDate, Location, Description)
VALUES
(1, 'Pretoria City Run', '2026-10-10', 'Pretoria, Gauteng',
 'A road running event with multiple distance categories.'),
(1, 'Mabopane Community Walk', '2026-11-07', 'Mabopane, Gauteng',
 'A community walking event suitable for different fitness levels.'),
(2, 'Johannesburg Cycle Challenge', '2026-11-21', 'Johannesburg, Gauteng',
 'A cycling event with multiple distance options.');
GO

-- Categories for Event 1
INSERT INTO Category (EventID, CategoryName, Distance, EntryFee, MaximumParticipants)
VALUES
(1, '10km Run', 10.00, 150.00, 500),
(1, '21km Half Marathon', 21.10, 250.00, 400),
(1, '42km Marathon', 42.20, 350.00, 300);

-- Categories for Event 2
INSERT INTO Category (EventID, CategoryName, Distance, EntryFee, MaximumParticipants)
VALUES
(2, '5km Community Walk', 5.00, 80.00, 600),
(2, '10km Walk', 10.00, 120.00, 400);

-- Categories for Event 3
INSERT INTO Category (EventID, CategoryName, Distance, EntryFee, MaximumParticipants)
VALUES
(3, '30km Cycle', 30.00, 200.00, 300),
(3, '60km Cycle', 60.00, 300.00, 250);
GO

-- Routes
INSERT INTO Route (EventID, RouteName, Distance, Description, MapURL)
VALUES
(1, 'Pretoria City 10/21/42 Route', 42.20,
 'Main city road route covering the event distances.', 'https://example.com/routes/pretoria-city'),
(2, 'Mabopane Community Route', 10.00,
 'Community route through Mabopane.', 'https://example.com/routes/mabopane'),
(3, 'Johannesburg Cycle Route', 60.00,
 'Road cycling route through selected Johannesburg roads.', 'https://example.com/routes/johannesburg-cycle');
GO

-- Weather
INSERT INTO Weather (EventID, WeatherDate, Temperature, [Condition], WindSpeed)
VALUES
(1, '2026-10-10', 22.50, 'Partly Cloudy', 12.00),
(2, '2026-11-07', 24.00, 'Sunny', 8.00),
(3, '2026-11-21', 20.50, 'Clear', 15.00);
GO

-- Sample Enrolments
INSERT INTO Enrolment (ParticipantID, CategoryID, EnrolmentDate, [Status])
VALUES
(3, 1, '2026-09-01', 'Confirmed'),
(3, 4, '2026-09-02', 'Confirmed'),
(4, 2, '2026-09-03', 'Confirmed'),
(4, 6, '2026-09-03', 'Confirmed');
GO

-- Sample Results
INSERT INTO Result (EnrolmentID, FinishTime, FinishingPosition)
VALUES
(1, '00:52:35', 24),
(2, '00:48:12', 12);
GO

-- =========================
-- Verification queries
-- =========================
SELECT * FROM [User];
SELECT * FROM Event;
SELECT * FROM Category;
SELECT * FROM Enrolment;
SELECT * FROM Result;
SELECT * FROM Route;
SELECT * FROM Weather;
GO
