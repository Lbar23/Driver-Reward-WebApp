using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Server.Migrations
{
    /// <inheritdoc />
    public partial class AuditLoggingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountActivity",
                columns: table => new
                {
                    ActivityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TIMESTAMP", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ActivityType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Details = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountActivity", x => x.ActivityId);
                    table.ForeignKey(
                        name: "FK_AccountActivity_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Authentications",
                columns: table => new
                {
                    AuthID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TIMESTAMP", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    AuthType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Success = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    UserAgent = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Details = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authentications", x => x.AuthID);
                    table.ForeignKey(
                        name: "FK_Authentications_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "FeedbackForms",
                keyColumn: "FeedbackID",
                keyValue: 1,
                column: "SubmissionDate",
                value: new DateTime(2024, 12, 3, 22, 55, 18, 960, DateTimeKind.Utc).AddTicks(851));

            migrationBuilder.UpdateData(
                table: "FeedbackForms",
                keyColumn: "FeedbackID",
                keyValue: 2,
                column: "SubmissionDate",
                value: new DateTime(2024, 12, 3, 22, 55, 18, 960, DateTimeKind.Utc).AddTicks(852));

            migrationBuilder.UpdateData(
                table: "FeedbackForms",
                keyColumn: "FeedbackID",
                keyValue: 3,
                column: "SubmissionDate",
                value: new DateTime(2024, 12, 3, 22, 55, 18, 960, DateTimeKind.Utc).AddTicks(854));

            migrationBuilder.UpdateData(
                table: "FeedbackForms",
                keyColumn: "FeedbackID",
                keyValue: 4,
                column: "SubmissionDate",
                value: new DateTime(2024, 12, 3, 22, 55, 18, 960, DateTimeKind.Utc).AddTicks(855));

            migrationBuilder.CreateIndex(
                name: "IX_AccountActivity_UserId",
                table: "AccountActivity",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Authentications_UserID",
                table: "Authentications",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountActivity");

            migrationBuilder.DropTable(
                name: "Authentications");

            migrationBuilder.UpdateData(
                table: "FeedbackForms",
                keyColumn: "FeedbackID",
                keyValue: 1,
                column: "SubmissionDate",
                value: new DateTime(2024, 12, 2, 1, 55, 4, 262, DateTimeKind.Utc).AddTicks(7186));

            migrationBuilder.UpdateData(
                table: "FeedbackForms",
                keyColumn: "FeedbackID",
                keyValue: 2,
                column: "SubmissionDate",
                value: new DateTime(2024, 12, 2, 1, 55, 4, 262, DateTimeKind.Utc).AddTicks(7188));

            migrationBuilder.UpdateData(
                table: "FeedbackForms",
                keyColumn: "FeedbackID",
                keyValue: 3,
                column: "SubmissionDate",
                value: new DateTime(2024, 12, 2, 1, 55, 4, 262, DateTimeKind.Utc).AddTicks(7191));

            migrationBuilder.UpdateData(
                table: "FeedbackForms",
                keyColumn: "FeedbackID",
                keyValue: 4,
                column: "SubmissionDate",
                value: new DateTime(2024, 12, 2, 1, 55, 4, 262, DateTimeKind.Utc).AddTicks(7193));
        }
    }
}
