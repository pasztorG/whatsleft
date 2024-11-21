using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Household> Households { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<FinancialData> FinancialData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Household>()
            .HasKey(h => h.Id);

        modelBuilder.Entity<Category>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<FinancialData>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(f => f.Household)
                .WithMany(h => h.FinancialData)
                .HasForeignKey(f => f.HouseholdId);
            entity.HasOne(f => f.Category)
                .WithMany(c => c.FinancialData)
                .HasForeignKey(f => f.CategoryId);
        });
    }
}
