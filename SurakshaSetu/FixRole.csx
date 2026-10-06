using Microsoft.EntityFrameworkCore;
using SurakshaSetu.Data;
using Microsoft.AspNetCore.Identity;
using SurakshaSetu.Models;
using System;
using System.Linq;

// A simple manual sql to update the role mapping
var connectionString = "Server=localhost;Database=SurakshaSetuDb;User Id=sa;Password=Your_Password123;Encrypt=False;";

using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
{
    connection.Open();
    
    // 1. Get Security Role ID
    var command1 = new Microsoft.Data.SqlClient.SqlCommand("SELECT Id FROM AspNetRoles WHERE Name = 'SecurityOfficer'", connection);
    var securityRoleId = (string)command1.ExecuteScalar();
    
    // 2. Get User ID
    var command2 = new Microsoft.Data.SqlClient.SqlCommand("SELECT Id FROM AspNetUsers WHERE Email = 'security@surakshasetu.edu'", connection);
    var userId = (string)command2.ExecuteScalar();
    
    if (securityRoleId != null && userId != null) {
        // 3. Delete old roles for this user
        var command3 = new Microsoft.Data.SqlClient.SqlCommand("DELETE FROM AspNetUserRoles WHERE UserId = '" + userId + "'", connection);
        command3.ExecuteNonQuery();
        
        // 4. Insert correct role
        var command4 = new Microsoft.Data.SqlClient.SqlCommand("INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('" + userId + "', '" + securityRoleId + "')", connection);
        command4.ExecuteNonQuery();
        
        Console.WriteLine("SUCCESS: User security@surakshasetu.edu now has the SecurityOfficer role!");
    } else {
        Console.WriteLine("Could not find user or role.");
    }
}
