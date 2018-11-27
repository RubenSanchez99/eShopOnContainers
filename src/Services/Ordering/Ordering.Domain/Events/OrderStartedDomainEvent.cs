using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using Ordering.Domain.AggregatesModel.BuyerAggregate.Identity;
using EventFlow.Aggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.Events
{
    /// <summary>
    /// Event used when an order is created
    /// </summary>
    public class OrderStartedDomainEvent : IAggregateEvent<Order, OrderId>
    {
        public string UserId { get; }
        public string UserName { get; }
        public int CardTypeId { get; }
        public string CardNumber { get; }
        public string CardSecurityNumber { get; }
        public string CardHolderName { get; }
        public DateTime CardExpiration { get; }
        public PaymentMethodId PaymentMethodId { get; }
        public DateTime OrderDate { get; }
        public Address Address { get; }

        public OrderStartedDomainEvent(PaymentMethodId paymentMethodId, DateTime orderDate, Address address, string userId, string userName,
                                       int cardTypeId, string cardNumber, 
                                       string cardSecurityNumber, string cardHolderName, 
                                       DateTime cardExpiration)
        {
            PaymentMethodId = paymentMethodId;
            OrderDate = orderDate;
            Address = address;
            UserId = userId;
            UserName = userName;
            CardTypeId = cardTypeId;
            CardNumber = cardNumber;
            CardSecurityNumber = cardSecurityNumber;
            CardHolderName = cardHolderName;
            CardExpiration = cardExpiration;
        }
    }
}
