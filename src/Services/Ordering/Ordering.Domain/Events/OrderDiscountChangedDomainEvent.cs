using System;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using EventFlow.Aggregates;

namespace Ordering.Domain.Events
{
    public class OrderDiscountChanged : IAggregateEvent<Order, OrderId>
    {
        public OrderDiscountChanged(decimal newDiscount, decimal oldDiscount)
        {
            this.NewDiscount = newDiscount;
            this.OldDiscount = oldDiscount;

        }
        public decimal NewDiscount { get; }
        public decimal OldDiscount { get; }
    }
}
