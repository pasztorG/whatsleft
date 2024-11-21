using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace whatsleft_minimal_api.Migrations
{
    public partial class SeedCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             // First, delete all existing categories
        migrationBuilder.Sql("DELETE FROM \"Categories\"");
        
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { Guid.NewGuid().ToString(), "Salary/Wages" },
                    { Guid.NewGuid().ToString(), "Bonuses" },
                    { Guid.NewGuid().ToString(), "Freelance Income" },
                    { Guid.NewGuid().ToString(), "Rental Income" },
                    { Guid.NewGuid().ToString(), "Investment Income" },
                    { Guid.NewGuid().ToString(), "Government Benefits" },
                    { Guid.NewGuid().ToString(), "Gifts" },
                    { Guid.NewGuid().ToString(), "Side Hustles" },
                    { Guid.NewGuid().ToString(), "Housing Costs" },
                    { Guid.NewGuid().ToString(), "Utilities" },
                    { Guid.NewGuid().ToString(), "Groceries" },
                    { Guid.NewGuid().ToString(), "Car Expenses" },
                    { Guid.NewGuid().ToString(), "Public Transport" },
                    { Guid.NewGuid().ToString(), "Insurance" },
                    { Guid.NewGuid().ToString(), "Healthcare" },
                    { Guid.NewGuid().ToString(), "Debt Payments" },
                    { Guid.NewGuid().ToString(), "Education" },
                    { Guid.NewGuid().ToString(), "Entertainment" },
                    { Guid.NewGuid().ToString(), "Savings/Investments" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Name",
                keyValues: new object[]
                {
                    "Salary/Wages",
                    "Bonuses",
                    "Freelance Income",
                    "Rental Income",
                    "Investment Income",
                    "Government Benefits",
                    "Gifts",
                    "Side Hustles",
                    "Housing Costs",
                    "Utilities",
                    "Groceries",
                    "Car Expenses",
                    "Public Transport",
                    "Insurance",
                    "Healthcare",
                    "Debt Payments",
                    "Education",
                    "Entertainment",
                    "Savings/Investments"
                });
        }
    }
}
