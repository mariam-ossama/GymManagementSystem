using GymManagement.Configurations;
using GymManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.DbContexts
{
    public class GymDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;Database=GymManagementDB;Trusted_Connection=true;TrustServerCertificate=true;");
        }

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration<Plan>(new PlanConfiguration());
        }
        public DbSet<Plan> Plans { get; set; }
    }
}
