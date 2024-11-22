using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Server.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseFixChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Trigger to validate point transactions
                CREATE TRIGGER trg_PointTransactionValidate
                BEFORE INSERT ON PointTransactions
                FOR EACH ROW
                BEGIN
                    DECLARE v_current_points INT;
                    DECLARE v_driver_exists INT;
                    
                    -- Verify driver exists and get current points from SponsorDrivers
                    SELECT Points, 1 INTO v_current_points, v_driver_exists
                    FROM SponsorDrivers 
                    WHERE DriverID = NEW.UserID
                    AND SponsorID = NEW.SponsorID;
                    
                    IF v_driver_exists IS NULL THEN
                        SIGNAL SQLSTATE '45000'
                        SET MESSAGE_TEXT = 'Driver not found or not associated with sponsor';
                    END IF;
                    
                    -- Prevent negative balance
                    IF NEW.PointsChanged < 0 AND (v_current_points + NEW.PointsChanged) < 0 THEN
                        SIGNAL SQLSTATE '45000'
                        SET MESSAGE_TEXT = 'Insufficient points for deduction';
                    END IF;
                END;

                -- Trigger to update points and audit
                CREATE TRIGGER trg_PointTransactionAfter
                AFTER INSERT ON PointTransactions
                FOR EACH ROW
                BEGIN
                    -- Update points in SponsorDrivers
                    UPDATE SponsorDrivers
                    SET Points = Points + NEW.PointsChanged
                    WHERE DriverID = NEW.UserID
                    AND SponsorID = NEW.SponsorID;
                    
                    -- Create audit log
                    INSERT INTO AuditLogs (UserID, Category, Description, Timestamp)
                    SELECT 
                        NEW.UserID,
                        'Points',
                        JSON_OBJECT(
                            'action', IF(NEW.PointsChanged >= 0, 'CREDIT', 'DEBIT'),
                            'amount', ABS(NEW.PointsChanged),
                            'reason', NEW.Reason,
                            'sponsorId', NEW.SponsorID,
                            'balance', (
                                SELECT Points 
                                FROM SponsorDrivers 
                                WHERE DriverID = NEW.UserID
                                AND SponsorID = NEW.SponsorID
                            )
                        ),
                        NEW.TransactionDate;
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS trg_PointTransactionValidate;
                DROP TRIGGER IF EXISTS trg_PointTransactionAfter;
            ");
        }
    }
}