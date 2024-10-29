using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialSeedingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: ["Id", "Name", "NormalizedName", "ConcurrencyStamp"],
                values: new object[,]
                {
                    { 1, "Admin", "ADMIN", Guid.NewGuid().ToString() },
                    { 2, "Sponsor", "SPONSOR", Guid.NewGuid().ToString() },
                    { 3, "Driver", "DRIVER", Guid.NewGuid().ToString() },
                    { 4, "Guest", "GUEST", Guid.NewGuid().ToString() }
                }
            );

            migrationBuilder.InsertData(
            table: "About",
            columns: ["Release", "Team", "Version", "Product", "Description"],
            values: new object[,]
                {
                    { 
                        DateTime.Now.Date,  
                        16,                          
                        1,                           
                        "GitGud Drivers",            
                        "Our Program, GitGud Drivers, aims to transform the trucking industry with our innovative web application designed to incentivize safe and efficient driving. Our platform allows companies to reward truck drivers for positive driving, offering points that can be redeemed for a variety of products produced by the sponsor company."
                    }
                }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
            table: "About",
            keyColumn: "Release",
            keyValue: DateTime.Now.Date);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValues: [1, 2, 3, 4]);
        }
    }
}
