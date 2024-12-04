using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogTriggers : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TRIGGER after_user_insert
                AFTER INSERT ON AspNetUsers
                FOR EACH ROW
                BEGIN
                    INSERT INTO AuditLogs (UserID, Category, Action, ActionSuccess, Timestamp, AdditionalDetails)
                    VALUES (NEW.Id, 0, 0, TRUE, UTC_TIMESTAMP(), CONCAT('User created: ', NEW.UserName));
                END;

                CREATE TRIGGER after_user_update
                AFTER UPDATE ON AspNetUsers
                FOR EACH ROW
                BEGIN
                    INSERT INTO AuditLogs (UserID, Category, Action, ActionSuccess, Timestamp, AdditionalDetails)
                    VALUES (NEW.Id, 0, 2, TRUE, UTC_TIMESTAMP(), CONCAT('User updated: ', NEW.UserName));
                END;

                CREATE TRIGGER after_user_delete
                AFTER DELETE ON AspNetUsers
                FOR EACH ROW
                BEGIN
                    INSERT INTO AuditLogs (UserID, Category, Action, ActionSuccess, Timestamp, AdditionalDetails)
                    VALUES (OLD.Id, 0, 1, TRUE, UTC_TIMESTAMP(), CONCAT('User deleted: ', OLD.UserName));
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS after_user_insert;
                DROP TRIGGER IF EXISTS after_user_update;
                DROP TRIGGER IF EXISTS after_user_delete;
            ");
        }
    }
}
