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
    public class OrderPaymentSuccededIntegrationEventHandler : IConsumer<OrderPaymentSuccededIntegrationEvent>
    {
        private readonly IAggregateStore _aggregateStore;

        public OrderPaymentSuccededIntegrationEventHandler(IAggregateStore aggregateStore)
        {
            _aggregateStore = aggregateStore ?? throw new ArgumentNullException(nameof(aggregateStore));
        }

        public async Task Consume(ConsumeContext<OrderPaymentSuccededIntegrationEvent> context)
        {
            var orderId = new OrderId(context.Message.OrderId);

            // Simulate a work time for validating the payment
            await Task.Delay(10000);

            var orderToUpdate = await _aggregateStore
                .LoadAsync<Order, OrderId>(orderId, CancellationToken.None)
                .ConfigureAwait(false);

            //orderToUpdate.SetPaidStatus();

            await _aggregateStore.UpdateAsync<Order, OrderId>(orderId, SourceId.New,
                (order, c) => {
                        order.SetPaidStatus();
                        return Task.FromResult(0);
                }, CancellationToken.None
            ).ConfigureAwait(false);
        }
    }
}
