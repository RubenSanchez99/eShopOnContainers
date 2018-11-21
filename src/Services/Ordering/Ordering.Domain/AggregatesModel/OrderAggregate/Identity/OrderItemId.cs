using System;
using EventFlow.Core;

namespace Ordering.Domain.AggregatesModel.OrderAggregate.Identity
{
    public class OrderItemId : Identity<OrderItemId>
    {
        public OrderItemId(string value) : base(value)
        {
        }
    }
}
