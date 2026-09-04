-- ============================================================
-- RaceDay Database Schema
-- SQL Server Management Studio (SSMS)
-- ============================================================

-- Drop tables if they exist (for clean recreation)
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL DROP TABLE dbo.Enrolments;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;

-- ============================================================
-- 1. Users Table
-- ============================================================
CREATE TABLE dbo.Users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    email NVARCHAR(255) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    full_name NVARCHAR(100) NOT NULL,
    role NVARCHAR(20) NOT NULL CHECK (role IN ('Organiser', 'Participant')),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);

-- ============================================================
-- 2. Events Table
-- ============================================================
CREATE TABLE dbo.Events (
    event_id INT IDENTITY(1,1) PRIMARY KEY,
    organiser_id INT NOT NULL,
    name NVARCHAR(200) NOT NULL,
    description NVARCHAR(MAX),
    start_date DATETIME NOT NULL,
    end_date DATETIME NOT NULL,
    location NVARCHAR(200) NOT NULL,
    status NVARCHAR(20) DEFAULT 'Draft' CHECK (status IN ('Draft', 'Open', 'Closed', 'Completed', 'Cancelled')),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Events_Users FOREIGN KEY (organiser_id) REFERENCES dbo.Users(user_id)
);

-- ============================================================
-- 3. Categories Table
-- ============================================================
CREATE TABLE dbo.Categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    event_id INT NOT NULL,
    name NVARCHAR(100) NOT NULL,
    distance_km DECIMAL(5,2) NOT NULL,
    start_time DATETIME,
    max_participants INT DEFAULT 100,
    fee_amount DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    created_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Categories_Events FOREIGN KEY (event_id) REFERENCES dbo.Events(event_id) ON DELETE CASCADE
);

-- ============================================================
-- 4. Enrolments Table
-- ============================================================
CREATE TABLE dbo.Enrolments (
    enrolment_id INT IDENTITY(1,1) PRIMARY KEY,
    participant_id INT NOT NULL,
    category_id INT NOT NULL,
    registration_date DATETIME DEFAULT GETDATE(),
    status NVARCHAR(20) DEFAULT 'Registered' CHECK (status IN ('Registered', 'Confirmed', 'Cancelled', 'Withdrawn')),
    bib_number INT NULL UNIQUE,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (participant_id) REFERENCES dbo.Users(user_id),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (category_id) REFERENCES dbo.Categories(category_id) ON DELETE CASCADE,
    CONSTRAINT UQ_Enrolments_Participant_Category UNIQUE (participant_id, category_id)
);

-- ============================================================
-- 5. Results Table
-- ============================================================
CREATE TABLE dbo.Results (
    result_id INT IDENTITY(1,1) PRIMARY KEY,
    enrolment_id INT NOT NULL UNIQUE,
    finish_time TIME(3) NULL,
    overall_position INT NULL,
    category_position INT NULL,
    chip_time TIME(3) NULL,
    gun_time TIME(3) NULL,
    status NVARCHAR(20) DEFAULT 'Pending' CHECK (status IN ('Pending', 'DNS', 'DNF', 'Finished', 'Disqualified')),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (enrolment_id) REFERENCES dbo.Enrolments(enrolment_id) ON DELETE CASCADE
);

-- ============================================================
-- 6. Payments Table
-- ============================================================
CREATE TABLE dbo.Payments (
    payment_id INT IDENTITY(1,1) PRIMARY KEY,
    enrolment_id INT NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    payment_date DATETIME DEFAULT GETDATE(),
    method NVARCHAR(50) NOT NULL CHECK (method IN ('Credit Card', 'Debit Card', 'Bank Transfer', 'Cash', 'Mobile Payment')),
    status NVARCHAR(20) DEFAULT 'Pending' CHECK (status IN ('Pending', 'Completed', 'Failed', 'Refunded')),
    transaction_id NVARCHAR(100) NULL,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Payments_Enrolments FOREIGN KEY (enrolment_id) REFERENCES dbo.Enrolments(enrolment_id) ON DELETE CASCADE
);

-- ============================================================
-- ============================================================
-- Sample Data - Seed Script
-- ============================================================
-- ============================================================

-- ------------------------------------------------------------
-- 1. Users (2 Organisers, 2 Participants)
-- ------------------------------------------------------------
INSERT INTO dbo.Users (email, password_hash, full_name, role, created_at) VALUES
('sarah.organiser@raceday.com', 'hashed_password_1', 'Sarah Johnson', 'Organiser', GETDATE()),
('mike.organiser@raceday.com', 'hashed_password_2', 'Mike Williams', 'Organiser', GETDATE()),
('emma.runner@email.com', 'hashed_password_3', 'Emma Thompson', 'Participant', GETDATE()),
('james.athlete@email.com', 'hashed_password_4', 'James Rodriguez', 'Participant', GETDATE());

-- ------------------------------------------------------------
-- 2. Events
-- ------------------------------------------------------------
INSERT INTO dbo.Events (organiser_id, name, description, start_date, end_date, location, status, created_at) VALUES
(1, 'City Marathon 2026', 'Annual city marathon featuring full and half marathon categories.', 
 '2026-11-15 06:00:00', '2026-11-15 18:00:00', 'Central Park, New York', 'Open', GETDATE()),
(2, 'Trail Running Series', 'Mountain trail running event with multiple distance options.', 
 '2026-10-20 07:00:00', '2026-10-20 17:00:00', 'Rocky Mountain National Park', 'Open', GETDATE()),
(1, 'Sunset 10K Classic', 'Evening 10K run along the waterfront.', 
 '2026-09-25 17:30:00', '2026-09-25 20:00:00', 'Bayfront Park, Miami', 'Draft', GETDATE());

-- ------------------------------------------------------------
-- 3. Categories (3 Categories per Event)
-- ------------------------------------------------------------
-- Event 1: City Marathon 2026
INSERT INTO dbo.Categories (event_id, name, distance_km, start_time, max_participants, fee_amount, created_at) VALUES
(1, 'Full Marathon', 42.20, '2026-11-15 06:00:00', 500, 120.00, GETDATE()),
(1, 'Half Marathon', 21.10, '2026-11-15 07:30:00', 800, 80.00, GETDATE()),
(1, '10K Run', 10.00, '2026-11-15 09:00:00', 600, 50.00, GETDATE());

-- Event 2: Trail Running Series
INSERT INTO dbo.Categories (event_id, name, distance_km, start_time, max_participants, fee_amount, created_at) VALUES
(2, '50K Ultra Trail', 50.00, '2026-10-20 05:00:00', 200, 150.00, GETDATE()),
(2, '25K Trail Run', 25.00, '2026-10-20 07:30:00', 350, 90.00, GETDATE()),
(2, '10K Trail Fun Run', 10.00, '2026-10-20 09:30:00', 400, 45.00, GETDATE());

-- Event 3: Sunset 10K Classic
INSERT INTO dbo.Categories (event_id, name, distance_km, start_time, max_participants, fee_amount, created_at) VALUES
(3, '10K Run', 10.00, '2026-09-25 17:30:00', 300, 55.00, GETDATE()),
(3, '5K Fun Run', 5.00, '2026-09-25 18:00:00', 200, 30.00, GETDATE()),
(3, 'Kids 1K Dash', 1.00, '2026-09-25 19:00:00', 100, 15.00, GETDATE());

-- ------------------------------------------------------------
-- 4. Enrolments (Sample enrolments for Participants)
-- ------------------------------------------------------------
-- Participant 1 (Emma) enrolments
INSERT INTO dbo.Enrolments (participant_id, category_id, registration_date, status, bib_number, created_at) VALUES
(3, 2, DATEADD(DAY, -30, GETDATE()), 'Confirmed', 101, GETDATE()),   -- Half Marathon
(3, 5, DATEADD(DAY, -15, GETDATE()), 'Registered', 205, GETDATE()), -- 25K Trail Run
(3, 7, DATEADD(DAY, -5, GETDATE()), 'Registered', 301, GETDATE());  -- 10K Run (Sunset)

-- Participant 2 (James) enrolments
INSERT INTO dbo.Enrolments (participant_id, category_id, registration_date, status, bib_number, created_at) VALUES
(4, 1, DATEADD(DAY, -45, GETDATE()), 'Confirmed', 102, GETDATE()),  -- Full Marathon
(4, 4, DATEADD(DAY, -20, GETDATE()), 'Confirmed', 206, GETDATE()), -- 50K Ultra Trail
(4, 8, DATEADD(DAY, -10, GETDATE()), 'Registered', 302, GETDATE()); -- 5K Fun Run

-- Additional enrolments to make events look realistic
-- More participants for Event 1
INSERT INTO dbo.Enrolments (participant_id, category_id, registration_date, status, bib_number, created_at) VALUES
(3, 1, DATEADD(DAY, -40, GETDATE()), 'Confirmed', 103, GETDATE()),  -- Full Marathon
(4, 2, DATEADD(DAY, -35, GETDATE()), 'Confirmed', 104, GETDATE()); -- Half Marathon

-- ------------------------------------------------------------
-- 5. Results (Sample results for completed/recent events)
-- ------------------------------------------------------------
-- Results for Event 1 (City Marathon) - some participants have finished
INSERT INTO dbo.Results (enrolment_id, finish_time, overall_position, category_position, chip_time, gun_time, status, created_at) VALUES
(1, '03:45:22.000', 45, 12, '03:44:50.000', '03:45:22.000', 'Finished', GETDATE()),
(6, '04:12:08.000', 78, 24, '04:11:30.000', '04:12:08.000', 'Finished', GETDATE()),
(2, '01:52:15.000', 15, 3, '01:52:00.000', '01:52:15.000', 'Finished', GETDATE()),
(7, '02:05:30.000', 32, 8, '02:05:10.000', '02:05:30.000', 'Finished', GETDATE());

-- Results for Event 2 (Trail Running) - some pending
INSERT INTO dbo.Results (enrolment_id, status, created_at) VALUES
(3, 'Pending', GETDATE()),  -- Emma - 25K Trail (not yet run)
(5, 'Pending', GETDATE()),  -- James - 50K Ultra (not yet run)
(4, 'DNS', GETDATE());      -- Emma - 10K Trail (DNS)

-- ------------------------------------------------------------
-- 6. Payments (Sample payment records)
-- ------------------------------------------------------------
-- Payments for Emma's enrolments
INSERT INTO dbo.Payments (enrolment_id, amount, payment_date, method, status, transaction_id, created_at) VALUES
(1, 80.00, DATEADD(DAY, -30, GETDATE()), 'Credit Card', 'Completed', 'TXN-2026-001', GETDATE()),
(2, 90.00, DATEADD(DAY, -15, GETDATE()), 'Credit Card', 'Completed', 'TXN-2026-002', GETDATE()),
(3, 55.00, DATEADD(DAY, -5, GETDATE()), 'Credit Card', 'Completed', 'TXN-2026-003', GETDATE());

-- Payments for James' enrolments
INSERT INTO dbo.Payments (enrolment_id, amount, payment_date, method, status, transaction_id, created_at) VALUES
(4, 120.00, DATEADD(DAY, -45, GETDATE()), 'Debit Card', 'Completed', 'TXN-2026-004', GETDATE()),
(5, 150.00, DATEADD(DAY, -20, GETDATE()), 'Bank Transfer', 'Completed', 'TXN-2026-005', GETDATE()),
(6, 30.00, DATEADD(DAY, -10, GETDATE()), 'Credit Card', 'Completed', 'TXN-2026-006', GETDATE());

-- Payments for additional enrolments
INSERT INTO dbo.Payments (enrolment_id, amount, payment_date, method, status, transaction_id, created_at) VALUES
(7, 120.00, DATEADD(DAY, -40, GETDATE()), 'Credit Card', 'Completed', 'TXN-2026-007', GETDATE()),
(8, 80.00, DATEADD(DAY, -35, GETDATE()), 'Credit Card', 'Completed', 'TXN-2026-008', GETDATE());

-- ============================================================
-- Verification Queries (Optional - comment out in production)
-- ============================================================
-- SELECT 'Users' AS TableName, COUNT(*) AS RowCount FROM dbo.Users
-- UNION SELECT 'Events', COUNT(*) FROM dbo.Events
-- UNION SELECT 'Categories', COUNT(*) FROM dbo.Categories
-- UNION SELECT 'Enrolments', COUNT(*) FROM dbo.Enrolments
-- UNION SELECT 'Results', COUNT(*) FROM dbo.Results
-- UNION SELECT 'Payments', COUNT(*) FROM dbo.Payments;

-- ============================================================
-- End of Script
-- ============================================================