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
    public class OrderStockRejectedIntegrationEventHandler : IConsumer<OrderStockRejectedIntegrationEvent>
    {
        IAggregateStore _aggregateStore;
        public OrderStockRejectedIntegrationEventHandler(IAggregateStore aggregateStore)
        {
            _aggregateStore = aggregateStore ?? throw new ArgumentNullException(nameof(aggregateStore));
        }

        public async Task Consume(ConsumeContext<OrderStockRejectedIntegrationEvent> context)
        {
            var orderId = new OrderId(context.Message.OrderId);

            var orderToUpdate = await _aggregateStore
                .LoadAsync<Order, OrderId>(orderId, CancellationToken.None)
                .ConfigureAwait(false);

            var orderStockRejectedItems = context.Message.OrderStockItems
                .FindAll(c => !c.HasStock)
                .Select(c => c.ProductId);

            await _aggregateStore.UpdateAsync<Order, OrderId>(orderId, SourceId.New,
                (order, c) => {
                        order.SetCancelledStatusWhenStockIsRejected(orderStockRejectedItems);
                        return Task.FromResult(0);
                }, CancellationToken.None
            ).ConfigureAwait(false);
        }
    }
}
