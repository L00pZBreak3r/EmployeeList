using Microsoft.EntityFrameworkCore;

namespace EmployeeListLibrary.EF
{
  using Models;

  public class EmployeeListDBContext : DbContext
  {
#if !USE_INMEMORY_DATABASE
    public readonly string DatabaseName;
#endif

    public DbSet<Employee> Employees { get; set; }
    public DbSet<SalaryPlan> SalaryPlans { get; set; }

#if USE_INMEMORY_DATABASE
    public EmployeeListDBContext(DbContextOptions<EmployeeListDBContext> options)
        : base(options)
    {
    }
#else
    public EmployeeListDBContext(string aDatabaseName)
    {
      DatabaseName = aDatabaseName;
    }
#endif

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<SalaryPlan>()
          .Property(c => c.EmployeeType)
          .HasConversion<int>();

      base.OnModelCreating(modelBuilder);

    }
#if !USE_INMEMORY_DATABASE
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=" + DatabaseName + ";Trusted_Connection=True;");
    }
#endif
  }
}
