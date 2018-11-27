using Ordering.Domain.AggregatesModel.BuyerAggregate;
using Ordering.Domain.AggregatesModel.BuyerAggregate.Identity;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using EventFlow.Aggregates;

namespace Ordering.Domain.Events
{
    public class BuyerAndPaymentMethodVerifiedDomainEvent
        : IAggregateEvent<Buyer, BuyerId>
    {
        public Buyer Buyer { get; private set; }
        public PaymentMethod Payment { get; private set; }
        public OrderId OrderId { get; private set; }

        public BuyerAndPaymentMethodVerifiedDomainEvent(Buyer buyer, PaymentMethod payment, OrderId orderId)
        {
            Buyer = buyer;
            Payment = payment;
            OrderId = orderId;
        }
    }
}
