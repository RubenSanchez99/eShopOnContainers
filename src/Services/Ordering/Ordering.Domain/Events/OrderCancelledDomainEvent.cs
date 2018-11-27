using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using EventFlow.Aggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.Events
{
    public class OrderCancelledDomainEvent : IAggregateEvent<Order, OrderId>
    {
        public Order Order { get; }
        public string Description { get; }

        public OrderCancelledDomainEvent(Order order, string description)
        {
            Description = description;
            Order = order;
        }
    }
}
