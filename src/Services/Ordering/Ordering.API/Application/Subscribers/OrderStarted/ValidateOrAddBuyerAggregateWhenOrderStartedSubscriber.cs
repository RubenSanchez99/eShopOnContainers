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

namespace Ordering.API.Application.Subscribers.OrderStarted
{
    public class ValidateOrAddBuyerAggregateWhenOrderStartedSubscriber
        : ISubscribeSynchronousTo<Order, OrderId, OrderStartedDomainEvent>
    {
        IAggregateStore _aggregateStore;

        public ValidateOrAddBuyerAggregateWhenOrderStartedSubscriber(IAggregateStore aggreateStore)
        {
            _aggregateStore = aggreateStore;
        }

        public async Task HandleAsync(IDomainEvent<Order, OrderId, OrderStartedDomainEvent> domainEvent, CancellationToken cancellationToken)
        {
            var cardTypeId = (domainEvent.AggregateEvent.CardTypeId != 0) ? domainEvent.AggregateEvent.CardTypeId : 1;
            var buyer = await _aggregateStore.LoadAsync<Buyer, BuyerId>(new BuyerId(domainEvent.AggregateEvent.UserId), CancellationToken.None).ConfigureAwait(false);
            
            if (buyer.IsNew)
            {                
                buyer.Create(domainEvent.AggregateEvent.UserId, domainEvent.AggregateEvent.UserName);
            }

            buyer.VerifyOrAddPaymentMethod(cardTypeId,
                                           $"Payment Method on {DateTime.UtcNow}",
                                           domainEvent.AggregateEvent.CardNumber,
                                           domainEvent.AggregateEvent.CardSecurityNumber,
                                           domainEvent.AggregateEvent.CardHolderName,
                                           domainEvent.AggregateEvent.CardExpiration,
                                           domainEvent.AggregateIdentity);
        }
    }
}
