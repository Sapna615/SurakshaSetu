-- Run this query to view tables and records in SurakshaSetuDb
USE SurakshaSetuDb;
GO

-- 1. All Tables in the Database
SELECT TABLE_NAME, TABLE_TYPE 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

-- 2. Registered Users (AspNetUsers)
SELECT Id, FullName, Email, PhoneNumber, EmailConfirmed 
FROM AspNetUsers;
GO

-- 3. Safety Reports & SOS Alerts
SELECT TOP 10 ReportId, IncidentCategory, IncidentLocation, Severity, Status, CreatedAt 
FROM SafetyReports 
ORDER BY CreatedAt DESC;
GO

-- 4. Campus Locations
SELECT LocationId, Name, LocationType, Latitude, Longitude 
FROM CampusLocations;
GO

-- 5. Emergency Contacts
SELECT EmergencyContactId, Name, Category, PhoneNumber, Is24x7 
FROM EmergencyContacts;
GO
