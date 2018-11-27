using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using eShopOnContainers.Services.IntegrationEvents.Events;
using MassTransit;
using EventFlow.Aggregates;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;

namespace Ordering.API.Application.IntegrationEvents.EventHandling
{
    public class OrderPaymentFailedIntegrationEventHandler : IConsumer<OrderPaymentFailedIntegrationEvent>
    {
        private readonly IAggregateStore _aggregateStore;

        public OrderPaymentFailedIntegrationEventHandler(IAggregateStore aggregateStore)
        {
            _aggregateStore = aggregateStore ?? throw new ArgumentNullException(nameof(aggregateStore));
        }

        public async Task Consume(ConsumeContext<OrderPaymentFailedIntegrationEvent> context)
        {
            var orderToUpdate = await _aggregateStore
                .LoadAsync<Order, OrderId>(new OrderId(context.Message.OrderId), CancellationToken.None)
                .ConfigureAwait(false);

            orderToUpdate.SetCancelledStatus();
        }
    }
}
