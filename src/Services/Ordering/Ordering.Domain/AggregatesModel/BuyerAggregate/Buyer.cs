using System;
using System.Collections.Generic;
using System.Linq;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using Ordering.Domain.AggregatesModel.BuyerAggregate;
using Ordering.Domain.AggregatesModel.BuyerAggregate.Identity;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using Ordering.Domain.Events;

namespace Ordering.Domain.AggregatesModel.BuyerAggregate
{
    public class Buyer : AggregateRoot<Buyer, BuyerId>,
        IEmit<BuyerCreatedDomainEvent>,
        IEmit<BuyerPaymentMethodAddedDomainEvent>
    {
        public string IdentityGuid { get; private set; }
        public string BuyerName { get; private set; }
        
        private List<PaymentMethod> _paymentMethods;        

        public IEnumerable<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

        public Buyer(BuyerId id) : base(id)
        {
            _paymentMethods = new List<PaymentMethod>();
        }

        public void Create(string identity, string name)
        {
            Emit(new BuyerCreatedDomainEvent(identity, name));
        }

        public PaymentMethod VerifyOrAddPaymentMethod(
            int cardTypeId, string alias, string cardNumber, 
            string securityNumber, string cardHolderName, DateTime expiration, OrderId orderId)
        {
            var existingPayment = _paymentMethods.Where(p => p.IsEqualTo(cardTypeId, cardNumber, expiration))
                .SingleOrDefault();

            if (existingPayment != null)
            {
                Emit(new BuyerAndPaymentMethodVerifiedDomainEvent(this, existingPayment, orderId));

                return existingPayment;
            }
            else
            {
                var payment = new PaymentMethod(PaymentMethodId.New, cardTypeId, alias, cardNumber, securityNumber, cardHolderName, expiration);

                Emit(new BuyerPaymentMethodAddedDomainEvent(payment));
                
                Emit(new BuyerAndPaymentMethodVerifiedDomainEvent(this, payment, orderId));

                return payment;
            }
        }

        public void Apply(BuyerCreatedDomainEvent aggregateEvent)
        {
            var identity = aggregateEvent.IdentityGuid;
            var name = aggregateEvent.BuyerName;
            IdentityGuid = !string.IsNullOrWhiteSpace(identity) ? identity : throw new ArgumentNullException(nameof(identity));
            BuyerName = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));
        }

        public void Apply(BuyerPaymentMethodAddedDomainEvent aggregateEvent)
        {
            _paymentMethods.Add(aggregateEvent.PaymentMethod);
        }
    }
}
