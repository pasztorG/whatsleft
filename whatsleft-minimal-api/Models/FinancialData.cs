public enum FinancialType
{
    Income,
    Expense
}

public class FinancialData
{
    public string Id { get; set; }
    public string HouseholdId { get; set; }
    public bool IsRegular { get; set; }
    public FinancialType Type { get; set; }
    public string Description { get; set; }
    public int Amount { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CategoryId { get; set; }

    public Household Household { get; set; }
    public Category Category { get; set; }
}
