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

-- 6. Permanently Delete a User by Email (Allows Re-Registration)
-- Replace 'user@example.com' with the email to delete:
/*
DECLARE @TargetEmail NVARCHAR(256) = 'user@example.com';
DECLARE @TargetUserId NVARCHAR(450);
SELECT @TargetUserId = Id FROM AspNetUsers WHERE Email = @TargetEmail;

IF @TargetUserId IS NOT NULL
BEGIN
    BEGIN TRANSACTION;
    DELETE FROM SOSAlerts WHERE UserId = @TargetUserId;
    DELETE FROM AuditLogs WHERE UserId = @TargetUserId;
    UPDATE SafetyReports SET UserId = NULL WHERE UserId = @TargetUserId;
    UPDATE SafetyReports SET AssignedTo = NULL WHERE AssignedTo = @TargetUserId;
    DELETE FROM AspNetUserRoles WHERE UserId = @TargetUserId;
    DELETE FROM AspNetUserClaims WHERE UserId = @TargetUserId;
    DELETE FROM AspNetUserLogins WHERE UserId = @TargetUserId;
    DELETE FROM AspNetUserTokens WHERE UserId = @TargetUserId;
    DELETE FROM AspNetUsers WHERE Id = @TargetUserId;
    COMMIT TRANSACTION;
    PRINT 'User ' + @TargetEmail + ' deleted successfully. Email is now available for re-registration.';
END
ELSE
BEGIN
    PRINT 'User ' + @TargetEmail + ' was not found.';
END
GO
*/
