using EventFlow;
using EventFlow.Configuration;
using EventFlow.EntityFramework;
using EventFlow.EntityFramework.Extensions;
using EventFlow.Extensions;
using EventFlow.Queries;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using System.Threading;
using Ordering.ReadModel.Model;
using Ordering.ReadModel.Queries;
using Ordering.ReadModel.QueryHandler;

namespace Ordering.ReadModel
{
    public static class ReadModelConfiguration
    {
        public static IEventFlowOptions AddEntityFrameworkReadModel(this IEventFlowOptions efo)
        {
            var queries = new[] {
                typeof(GetOrderQueryHandler),
                typeof(GetOrdersFromUserQueryHandler)
            };

            return efo
                .UseEntityFrameworkReadModel<OrderReadModel, OrderingDbContext>()
                //.UseEntityFrameworkReadModel<CatalogTypeReadModel, CatalogDbContext>()
                //.UseEntityFrameworkReadModel<CatalogBrandReadModel, CatalogDbContext>()
                .AddQueryHandlers(queries)
                .RegisterServices(sr => sr.Register(c => @"Server=tcp:127.0.0.1,5433;Initial Catalog=CapacitacionMicroservicios.OrderingDb;User Id=sa;Password=Pass@word"))
                .ConfigureEntityFramework(EntityFrameworkConfiguration.New)
                .AddDbContextProvider<OrderingDbContext, DbContextProvider>();
        }

        public static void Query(IRootResolver resolver, OrderId exampleId)
        {
            // Resolve the query handler and use the built-in query for fetching
            // read models by identity to get our read model representing the
            // state of our aggregate root
            var queryProcessor = resolver.Resolve<IQueryProcessor>();
            // var exampleReadModel = queryProcessor.Process(new ReadModelByIdQuery<CompetitionReadModel>(exampleId), CancellationToken.None);
        }
    }
}
