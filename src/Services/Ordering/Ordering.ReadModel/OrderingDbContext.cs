using Microsoft.EntityFrameworkCore;
using Ordering.ReadModel.Model;

namespace Ordering.ReadModel
{
    public class OrderingDbContext : DbContext
    {
        public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
        {
        }

        public DbSet<OrderReadModel> Orders { get; set; }
        public DbSet<OrderItemReadModel> OrderItems { get; set; }
        public DbSet<OrderSummaryReadModel> OrderSummaries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<OrderReadModel>()
                .Property(e => e.OrderNumber)
                .ValueGeneratedOnAdd();
            
            modelBuilder.Entity<OrderReadModel>().HasKey(e => e.OrderId);
        }
    }
}
