using Microsoft.EntityFrameworkCore;

namespace FinancePal.Models{
    public class FinancePalDbContext : DbContext{
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<User> Users { get; set; }

        public FinancePalDbContext(DbContextOptions<FinancePalDbContext> options)
            : base(options){
        }
    }
}
