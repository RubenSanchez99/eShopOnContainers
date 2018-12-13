using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using eShopOnContainers.Services.IntegrationEvents.Events;
using MassTransit;
using EventFlow.Aggregates;
using EventFlow.Core;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;

namespace Ordering.API.Application.IntegrationEvents.EventHandling
{
    public class OrderStockConfirmedIntegrationEventHandler : IConsumer<OrderStockConfirmedIntegrationEvent>
    {
        private readonly IAggregateStore _aggregateStore;

        public OrderStockConfirmedIntegrationEventHandler(IAggregateStore aggregateStore)
        {
            _aggregateStore = aggregateStore ?? throw new ArgumentNullException(nameof(aggregateStore));
        }

        public async Task Consume(ConsumeContext<OrderStockConfirmedIntegrationEvent> context)
        {
            var orderId = new OrderId(context.Message.OrderId);
            // Simulate a work time for confirming the stock
            await Task.Delay(5000);
            
            var orderToUpdate = await _aggregateStore
                .LoadAsync<Order, OrderId>(orderId, CancellationToken.None)
                .ConfigureAwait(false);

            await Console.Out.WriteLineAsync("Confirmed stock for order" + orderToUpdate.Id.Value);
            
            //orderToUpdate.SetStockConfirmedStatus();

            await _aggregateStore.UpdateAsync<Order, OrderId>(orderId, SourceId.New,
                (order, c) => {
                        order.SetStockConfirmedStatus();
                        return Task.FromResult(0);
                }, CancellationToken.None
            ).ConfigureAwait(false);
        }
    }
}
