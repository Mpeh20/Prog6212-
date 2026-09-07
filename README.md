# Prog6212-RaceDay System - Documentation
Project Overview
RaceDay is a comprehensive event management system designed for race organisers and participants. The platform enables organisers to create and manage racing events, while participants can discover events, register for categories, track their results, and manage payments.

System Architecture
Technology Stack
Component	Technology
Database	SQL Server (SSMS)
Backend API	RESTful API (to be implemented in Part 2)
Authentication	JWT (JSON Web Tokens)
Version Control	Git / GitHub
Database Schema
The system uses a relational database with the following core entities:

Users - System users with role-based access (Organiser/Participant)

Events - Racing events created by organisers

Categories - Race categories within events (e.g., 5K, 10K, Marathon)

Enrolments - Participant registrations for event categories

Results - Race results for each enrolment

Payments - Payment records for event enrolments

Entity Relationship Diagram
https://ERD.png

Refer to ERD.png for the complete entity relationship diagram.

Planning Documents
All planning documents are located in the /docs folder:

Document	Description
ERD.png	Entity Relationship Diagram showing all entities, attributes, and relationships
API_Endpoint_Plan.md	Complete RESTful API endpoint specifications
database_schema.sql	SQL Server database creation and seeding script
API Endpoints Overview
Authentication
POST /api/auth/register - Register a new user

POST /api/auth/login - Login and receive JWT token

Users
GET /api/users/me - Get current user profile

PUT /api/users/me - Update current user profile

GET /api/users/{id} - Get user by ID

GET /api/users/me/enrolments - Get current user's enrolments

Events
GET /api/events - List all events (with filters)

GET /api/events/{id} - Get event details

POST /api/events - Create new event (Organiser only)

PUT /api/events/{id} - Update event (Organiser only)

DELETE /api/events/{id} - Delete event (Organiser only)

Categories
GET /api/events/{eventId}/categories - List categories for an event

POST /api/events/{eventId}/categories - Add category (Organiser only)

PUT /api/categories/{id} - Update category (Organiser only)

DELETE /api/categories/{id} - Delete category (Organiser only)

Enrolments
POST /api/enrolments - Register for an event category

GET /api/enrolments/{id} - Get enrolment details

PUT /api/enrolments/{id} - Update enrolment status

DELETE /api/enrolments/{id} - Cancel enrolment

Results
POST /api/enrolments/{enrolmentId}/results - Record result (Organiser only)

GET /api/enrolments/{enrolmentId}/results - Get result by enrolment

PUT /api/results/{id} - Update result (Organiser only)

DELETE /api/results/{id} - Delete result (Organiser only)

GET /api/events/{eventId}/results - Get event results/leaderboard

Payments
POST /api/enrolments/{enrolmentId}/payments - Process payment

GET /api/enrolments/{enrolmentId}/payments - Get enrolment payments

GET /api/payments/{id} - Get payment details

Database Setup Instructions
Prerequisites
SQL Server Management Studio (SSMS)

SQL Server instance (Local or Azure)

Setup Steps
Connect to SQL Server

sql
-- Open SSMS and connect to your SQL Server instance
Create the Database

sql
CREATE DATABASE RaceDay;
GO
USE RaceDay;
GO
Run the Schema Script

Open database_schema.sql in SSMS

Execute the entire script (F5)

The script will:

Drop existing tables (if they exist)

Create all tables with proper constraints

Seed the database with sample data

Verify Installation

sql
-- Check that all tables were created
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';

-- Check sample data
SELECT * FROM dbo.Users;
SELECT * FROM dbo.Events;
Sample Data Included
The seed script includes:

Entity	Count	Description
Users	4	2 Organisers, 2 Participants
Events	3	Various race events
Categories	9	3 categories per event
Enrolments	8	Sample participant registrations
Results	6	Sample race results
Payments	8	Sample payment records
Security & Role-Based Access
User Roles
Role	Permissions
Organiser	Create/update/delete events, manage categories, record results, view all enrolments
Participant	Register for events, view own enrolments, view own results, make payments
Public (None)	View events, view event results/leaderboards, register/login
Authentication Flow
User registers with email, password, name, and role

Password is hashed and stored in the database

User logs in with email/password

Server returns a JWT token

Client includes token in Authorization: Bearer <token> header for protected endpoints

Middleware validates token and checks role-based permissions

Planned Deviations (If Any)
The following section documents any deliberate differences between the planning documents and the final implementation.

None at this stage - The system will be implemented exactly as planned.

Development Guidelines
File Structure
text
RaceDay/
├── docs/
│   ├── ERD.png
│   ├── API_Endpoint_Plan.md
│   ├── database_schema.sql
│   └── README.md
├── src/
│   └── (API code - Part 2)
└── tests/
    └── (Test files)
Naming Conventions
Component	Convention	Example
Tables	Plural, PascalCase	Users, Enrolments
Columns	snake_case	user_id, created_at
API Routes	kebab-case	/api/events/{id}/categories
JSON Fields	camelCase	fullName, startDate
Commit Message Format
text
<type>: <description>

[optional body]

[optional footer]
Types: feat, fix, docs, style, refactor, test, chore

Troubleshooting
Common Issues
Issue	Solution
Cannot connect to SQL Server	Verify server name and credentials in SSMS
Script execution fails	Ensure you're running the script in the correct database context
Foreign key constraint errors	Verify the parent tables exist and have data before referencing them
Duplicate email errors	Sample data uses unique emails; if re-running, drop tables first
Contact
For questions or issues, please refer to the course documentation or contact your instructor.

Version History
Version	Date	Changes
1.0	2026-09-04	Initial planning documents completed
- ERD created
- API endpoints planned
- Database schema designed and seeded
