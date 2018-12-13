using System;
using System.Collections.Generic;
using System.Linq;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using Newtonsoft.Json;
using Ordering.Domain.AggregatesModel.BuyerAggregate;
using Ordering.Domain.AggregatesModel.BuyerAggregate.Identity;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using Ordering.Domain.Events;

namespace Ordering.Domain.AggregatesModel.BuyerAggregate
{
    public class Buyer : AggregateRoot<Buyer, BuyerId>,
        IEmit<BuyerCreatedDomainEvent>,
        IEmit<BuyerPaymentMethodAddedDomainEvent>,
        IEmit<BuyerAndPaymentMethodVerifiedDomainEvent>
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

        public void VerifyOrAddPaymentMethod(
            int cardTypeId, string alias, string cardNumber, 
            string securityNumber, string cardHolderName, DateTime expiration, OrderId orderId)
        {
            Console.Out.WriteLine("Card type id: " + cardTypeId);
            Console.Out.WriteLine("Card number: " + cardNumber);
            Console.Out.WriteLine("Card expiration: " + expiration);

            Console.Out.WriteLine(JsonConvert.SerializeObject(_paymentMethods, Formatting.Indented));
            
            var existingPayment = _paymentMethods.Where(p => p.IsEqualTo(cardTypeId, cardNumber, expiration))
                .SingleOrDefault();

            if (existingPayment != null)
            {
                Emit(new BuyerAndPaymentMethodVerifiedDomainEvent(existingPayment.Id, orderId));

                //return existingPayment;
            }
            else
            {
                //TODO: Revisar por qué cardNumber aparece como inexistente
                //var payment = new PaymentMethod(PaymentMethodId.New, cardTypeId, alias, cardNumber, securityNumber, cardHolderName, expiration);

                var paymentMethodId = PaymentMethodId.New;

                var paymentMethodAdded = new BuyerPaymentMethodAddedDomainEvent(paymentMethodId ,cardTypeId, alias, cardNumber, securityNumber, cardHolderName, expiration);
                Console.Out.WriteLine(JsonConvert.SerializeObject(paymentMethodAdded, Formatting.Indented));
                Emit(paymentMethodAdded);
                
                var paymentMethodVerified = new BuyerAndPaymentMethodVerifiedDomainEvent(paymentMethodId, orderId);
                Console.Out.WriteLine(JsonConvert.SerializeObject(paymentMethodVerified, Formatting.Indented));

                Emit(paymentMethodVerified);

                //return payment;
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
            _paymentMethods.Add(new PaymentMethod(aggregateEvent.PaymentMethodId, aggregateEvent.CardTypeId, aggregateEvent.Alias, aggregateEvent.CardNumber, aggregateEvent.SecurityNumber, aggregateEvent.CardHolderName, aggregateEvent.Expiration));
        }

        public void Apply(BuyerAndPaymentMethodVerifiedDomainEvent aggregateEvent)
        {
        }
    }
}
