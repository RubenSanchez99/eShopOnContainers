using System;
using EventFlow.Entities;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;

namespace Ordering.Domain.AggregatesModel.OrderAggregate
{
    public class OrderItem : Entity<OrderItemId>
    {
        public OrderItem(OrderItemId id) : base(id)
        {
        }
    }
}
