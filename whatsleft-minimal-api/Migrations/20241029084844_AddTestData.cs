using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace whatsleft_minimal_api.Migrations
{
    public partial class AddTestData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var currentUtc = DateTime.UtcNow;
            
            // Create a test household
            migrationBuilder.InsertData(
                table: "Households",
                columns: new[] { "Id", "Name", "Password" },
                values: new object[] { "test-household", "Test Family", "hashed_password_here" }
            );

            // Add sample financial data
            migrationBuilder.InsertData(
                table: "FinancialData",
                columns: new[] { "Id", "HouseholdId", "IsRegular", "Type", "Description", "Amount", "Date", "CreatedAt", "CategoryId" },
                values: new object[,]
                {
                    { "fd1", "test-household", true, 0, "Monthly Salary", 350000, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), currentUtc, "1" },
                    { "fd2", "test-household", true, 1, "Rent Payment", 120000, new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc), currentUtc, "9" },
                    { "fd3", "test-household", true, 1, "Groceries", 45000, new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc), currentUtc, "11" },
                    { "fd4", "test-household", false, 0, "Year-end Bonus", 100000, new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc), currentUtc, "2" },
                    { "fd5", "test-household", true, 1, "Car Insurance", 25000, new DateTime(2024, 1, 20, 0, 0, 0, DateTimeKind.Utc), currentUtc, "14" },
                    { "fd6", "test-household", true, 1, "Internet Bill", 5000, new DateTime(2024, 1, 25, 0, 0, 0, DateTimeKind.Utc), currentUtc, "10" },
                    { "fd7", "test-household", false, 1, "Entertainment", 15000, new DateTime(2024, 1, 28, 0, 0, 0, DateTimeKind.Utc), currentUtc, "18" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"FinancialData\" WHERE \"HouseholdId\" = 'test-household'");
            migrationBuilder.Sql("DELETE FROM \"Households\" WHERE \"Id\" = 'test-household'");
        }
    }
}