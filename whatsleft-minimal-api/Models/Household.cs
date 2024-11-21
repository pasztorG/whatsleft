public class Household
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }

    public ICollection<FinancialData> FinancialData { get; set; }
}
