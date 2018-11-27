using Ordering.Domain.AggregatesModel.BuyerAggregate;
using Ordering.Domain.AggregatesModel.BuyerAggregate.Identity;
using EventFlow.Aggregates;

namespace Ordering.Domain.Events
{
    public class BuyerPaymentMethodAddedDomainEvent : IAggregateEvent<Buyer, BuyerId>
    {
        public BuyerPaymentMethodAddedDomainEvent(PaymentMethod paymentMethod)
        {
            this.PaymentMethod = paymentMethod;

        }
        public PaymentMethod PaymentMethod { get; private set; }
    }
}
