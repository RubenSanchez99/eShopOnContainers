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
    public class GracePeriodConfirmedIntegrationEventHandler : IConsumer<GracePeriodConfirmedIntegrationEvent>
    {
        private readonly IAggregateStore _aggregateStore;

        public GracePeriodConfirmedIntegrationEventHandler(IAggregateStore aggregateStore)
        {
            _aggregateStore = aggregateStore ?? throw new ArgumentNullException(nameof(aggregateStore));
        }

        public async Task Consume(ConsumeContext<GracePeriodConfirmedIntegrationEvent> context)
        {
            var orderId = new OrderId(context.Message.OrderId);

            var orderToUpdate = await _aggregateStore
                .LoadAsync<Order, OrderId>(orderId, CancellationToken.None)
                .ConfigureAwait(false);

            //orderToUpdate.SetAwaitingValidationStatus();
            await _aggregateStore.UpdateAsync<Order, OrderId>(orderId, SourceId.New,
                (order, c) => {
                        order.SetAwaitingValidationStatus();
                        return Task.FromResult(0);
                }, CancellationToken.None
            ).ConfigureAwait(false);
        }
    }
}
