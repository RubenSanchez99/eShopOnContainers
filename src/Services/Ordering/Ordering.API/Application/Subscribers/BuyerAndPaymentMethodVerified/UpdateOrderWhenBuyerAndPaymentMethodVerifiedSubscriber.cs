using System;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Subscribers;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using Ordering.Domain.AggregatesModel.BuyerAggregate;
using Ordering.Domain.AggregatesModel.BuyerAggregate.Identity;
using Ordering.Domain.Events;

namespace Ordering.API.Application.Subscribers.BuyerAndPaymentMethodVerified
{
    public class UpdateOrderWhenBuyerAndPaymentMethodVerifiedSubscriber
        : ISubscribeSynchronousTo<Buyer, BuyerId, BuyerAndPaymentMethodVerifiedDomainEvent>
    {
        IAggregateStore _aggregateStore;

        public UpdateOrderWhenBuyerAndPaymentMethodVerifiedSubscriber(IAggregateStore aggreateStore)
        {
            _aggregateStore = aggreateStore;
        }

        public async Task HandleAsync(IDomainEvent<Buyer, BuyerId, BuyerAndPaymentMethodVerifiedDomainEvent> domainEvent, CancellationToken cancellationToken)
        {
            var order = await _aggregateStore.LoadAsync<Order, OrderId>(
                                      domainEvent.AggregateEvent.OrderId
                                    , CancellationToken.None).ConfigureAwait(false); 
            order.SetBuyerId(domainEvent.AggregateEvent.Buyer.Id);
            order.SetPaymentId(domainEvent.AggregateEvent.Payment.Id);
            return;
        }
    }
}
