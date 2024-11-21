using Microsoft.EntityFrameworkCore.Migrations;

namespace whatsleft_minimal_api.Migrations
{
    public partial class UpdateCategoryIds : Migration
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
                    { "1", "Salary/Wages" },
                    { "2", "Bonuses" },
                    { "3", "Freelance Income" },
                    { "4", "Rental Income" },
                    { "5", "Investment Income" },
                    { "6", "Government Benefits" },
                    { "7", "Gifts" },
                    { "8", "Side Hustles" },
                    { "9", "Housing Costs" },
                    { "10", "Utilities" },
                    { "11", "Groceries" },
                    { "12", "Car Expenses" },
                    { "13", "Public Transport" },
                    { "14", "Insurance" },
                    { "15", "Healthcare" },
                    { "16", "Debt Payments" },
                    { "17", "Education" },
                    { "18", "Entertainment" },
                    { "19", "Savings/Investments" }
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