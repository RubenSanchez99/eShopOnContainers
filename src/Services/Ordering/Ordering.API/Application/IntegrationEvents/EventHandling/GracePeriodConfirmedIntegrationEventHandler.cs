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
    public class GracePeriodConfirmedIntegrationEventHandler : IConsumer<GracePeriodConfirmedIntegrationEvent>
    {
        private readonly IAggregateStore _aggregateStore;

        public GracePeriodConfirmedIntegrationEventHandler(IAggregateStore aggregateStore)
        {
            _aggregateStore = aggregateStore ?? throw new ArgumentNullException(nameof(aggregateStore));
        }

        public async Task Consume(ConsumeContext<GracePeriodConfirmedIntegrationEvent> context)
        {
            var orderToUpdate = await _aggregateStore
                .LoadAsync<Order, OrderId>(new OrderId(context.Message.OrderId), CancellationToken.None)
                .ConfigureAwait(false);

            orderToUpdate.SetAwaitingValidationStatus();
        }
    }
}
