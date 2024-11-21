using System.Collections.Generic;

public class Category
{
    public string Id { get; set; }
    public string Name { get; set; }

    public ICollection<FinancialData> FinancialData { get; set; }
}
