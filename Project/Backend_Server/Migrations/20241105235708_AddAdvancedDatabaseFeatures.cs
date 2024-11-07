using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedDatabaseFeatures : Migration
    {
        /// <inheritdoc />
         protected override void Up(MigrationBuilder migrationBuilder)
        {
            //PMS Triggers
            migrationBuilder.Sql(@"
                -- Trigger to validate point transactions and update driver balance
                CREATE TRIGGER trg_PointTransactionValidate
                BEFORE INSERT ON PointTransactions
                FOR EACH ROW
                BEGIN
                    DECLARE v_current_points INT;
                    DECLARE v_driver_exists INT;
                    
                    -- Verify driver exists and get current points
                    SELECT TotalPoints, 1 INTO v_current_points, v_driver_exists
                    FROM Drivers 
                    WHERE UserID = NEW.UserID
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

                -- Trigger to update driver points and audit
                CREATE TRIGGER trg_PointTransactionAfter
                AFTER INSERT ON PointTransactions
                FOR EACH ROW
                BEGIN
                    -- Update driver points
                    UPDATE Drivers
                    SET TotalPoints = TotalPoints + NEW.PointsChanged
                    WHERE UserID = NEW.UserID;
                    
                    -- Create audit log
                    INSERT INTO AuditLog (UserID, Category, Description, Timestamp)
                    SELECT 
                        NEW.UserID,
                        'Points',
                        JSON_OBJECT(
                            'action', IF(NEW.PointsChanged >= 0, 'CREDIT', 'DEBIT'),
                            'amount', ABS(NEW.PointsChanged),
                            'reason', NEW.Reason,
                            'sponsorId', NEW.SponsorID,
                            'balance', (
                                SELECT TotalPoints 
                                FROM Drivers 
                                WHERE UserID = NEW.UserID
                            )
                        ),
                        NEW.TransactionDate;
                END;
            ");

            //Procedures based on current progress
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_ProcessPurchase(
                    IN p_userId INT,
                    IN p_productId INT,
                    IN p_sponsorId INT,
                    OUT p_purchaseId INT,
                    OUT p_success BIT
                )
                BEGIN
                    DECLARE v_points INT;
                    DECLARE v_price INT;
                    DECLARE v_available BIT;
                    
                    START TRANSACTION;
                    
                    -- Get product info with lock
                    SELECT PriceInPoints, Availability 
                    INTO v_price, v_available
                    FROM Products 
                    WHERE ProductID = p_productId 
                    AND SponsorID = p_sponsorId
                    FOR UPDATE;
                    
                    -- Check product exists and is available
                    IF v_price IS NULL OR v_available = 0 THEN
                        SET p_success = 0;
                        SET p_purchaseId = NULL;
                        ROLLBACK;
                    ELSE
                        -- Get driver points with lock
                        SELECT TotalPoints 
                        INTO v_points
                        FROM Drivers 
                        WHERE UserID = p_userId
                        AND SponsorID = p_sponsorId
                        FOR UPDATE;
                        
                        IF v_points >= v_price THEN
                            -- Create purchase
                            INSERT INTO Purchases (
                                UserID, DriverUserID, ProductID,
                                PointsSpent, Status, PurchaseDate
                            )
                            VALUES (
                                p_userId, p_userId, p_productId,
                                v_price, 'Ordered', CURRENT_TIMESTAMP()
                            );
                            
                            SET p_purchaseId = LAST_INSERT_ID();
                            
                            -- Deduct points via point transaction
                            INSERT INTO PointTransactions (
                                UserID, SponsorID, PointsChanged,
                                Reason, TransactionDate
                            )
                            VALUES (
                                p_userId, p_sponsorId, -v_price,
                                CONCAT('Purchase: ', p_purchaseId),
                                CURRENT_TIMESTAMP()
                            );
                            
                            SET p_success = 1;
                            COMMIT;
                        ELSE
                            SET p_success = 0;
                            SET p_purchaseId = NULL;
                            ROLLBACK;
                        END IF;
                    END IF;
                END;

                -- Procedure for cancelling/refunding purchases
                CREATE PROCEDURE sp_ProcessPurchaseCancel(
                    IN p_purchaseId INT,
                    IN p_userId INT,
                    IN p_refund BIT,
                    OUT p_success BIT
                )
                BEGIN
                    DECLARE v_points INT;
                    DECLARE v_sponsorId INT;
                    DECLARE v_status VARCHAR(50);
                    
                    START TRANSACTION;
                    
                    -- Get purchase info
                    SELECT p.PointsSpent, p.Status, d.SponsorID 
                    INTO v_points, v_status, v_sponsorId
                    FROM Purchases p
                    JOIN Drivers d ON p.DriverUserID = d.UserID
                    WHERE p.PurchaseID = p_purchaseId
                    AND p.UserID = p_userId
                    FOR UPDATE;
                    
                    IF v_status = 'Ordered' THEN
                        -- Update purchase status
                        UPDATE Purchases 
                        SET Status = IF(p_refund, 'Refunded', 'Cancelled')
                        WHERE PurchaseID = p_purchaseId;
                        
                        -- Refund points if requested
                        IF p_refund THEN
                            INSERT INTO PointTransactions (
                                UserID, SponsorID, PointsChanged,
                                Reason, TransactionDate
                            )
                            VALUES (
                                p_userId, v_sponsorId, v_points,
                                CONCAT('Refund for purchase: ', p_purchaseId),
                                CURRENT_TIMESTAMP()
                            );
                        END IF;
                        
                        SET p_success = 1;
                        COMMIT;
                    ELSE
                        SET p_success = 0;
                        ROLLBACK;
                    END IF;
                END;
            ");

            // Maintanence jobs
            migrationBuilder.Sql(@"
                -- Daily product maintenance
                CREATE EVENT evt_DailyProductMaintenance
                ON SCHEDULE EVERY 1 DAY
                STARTS CURRENT_TIMESTAMP
                DO
                BEGIN
                    -- Cleanup old audit logs
                    DELETE FROM AuditLog 
                    WHERE Timestamp < DATE_SUB(CURRENT_TIMESTAMP, INTERVAL 90 DAY)
                    AND Category NOT IN ('Purchase', 'Points');
                    
                    -- Update product availability
                    UPDATE Products 
                    SET Availability = FALSE
                    WHERE LastChecked < DATE_SUB(CURRENT_TIMESTAMP, INTERVAL 24 HOUR);
                    
                    -- Reconcile point balances
                    UPDATE Drivers d
                    SET TotalPoints = (
                        SELECT COALESCE(SUM(PointsChanged), 0)
                        FROM PointTransactions pt
                        WHERE pt.UserID = d.UserID
                    );
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS trg_PointTransactionValidate;
                DROP TRIGGER IF EXISTS trg_PointTransactionAfter;
            ");

            
            migrationBuilder.Sql(@"
                DROP PROCEDURE IF EXISTS sp_ProcessPurchase;
                DROP PROCEDURE IF EXISTS sp_ProcessPurchaseCancel;
            ");

            
            migrationBuilder.Sql(@"
                DROP EVENT IF EXISTS evt_DailyProductMaintenance;
            ");
        }
    }
}
