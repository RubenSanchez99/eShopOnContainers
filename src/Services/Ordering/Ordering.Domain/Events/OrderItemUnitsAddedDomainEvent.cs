using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using EventFlow.Aggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.Events
{
    public class OrderItemUnitsAddedDomainEvent : IAggregateEvent<Order, OrderId>
    {
        public OrderItemUnitsAddedDomainEvent(int productId, int unitsAdded)
        {
            this.ProductId = productId;
            this.UnitsAdded = unitsAdded;

        }
        public int ProductId { get; }
        public int UnitsAdded { get; }
    }
}
