using System;
using Microsoft.EntityFrameworkCore;
using EventFlow.EntityFramework;

namespace Ordering.ReadModel
{
    public class DbContextProvider : IDbContextProvider<OrderingDbContext>, IDisposable
    {
        private readonly DbContextOptions<OrderingDbContext> _options;

        public DbContextProvider(string msSqlConnectionString)
        {
            _options = new DbContextOptionsBuilder<OrderingDbContext>()
                .UseSqlServer(@"Server=sql.data;Initial Catalog=CapacitacionMicroservicios.OrderingDb;User Id=sa;Password=Pass@word")
                .Options;
        }

        public OrderingDbContext CreateContext()
        {
            var context = new OrderingDbContext(_options);
            return context;
        }

        public void Dispose()
        {
        }
    }
}
