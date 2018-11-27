using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using EventFlow.Aggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.Events
{
    public class OrderItemNewDiscountSetDomainEvent : IAggregateEvent<Order, OrderId>
    {
        public OrderItemNewDiscountSetDomainEvent(int productId, decimal newDiscount)
        {
            this.ProductId = productId;
            this.NewDiscount = newDiscount;

        }
        public int ProductId { get; }
        public decimal NewDiscount { get; }
    }
}
