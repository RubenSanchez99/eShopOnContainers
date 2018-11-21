using System;
using EventFlow.Core;

namespace Ordering.Domain.AggregatesModel.OrderAggregate.Identity
{
    public class OrderId : Identity<OrderId>
    {
        public OrderId(string value) : base(value)
        {
        }
    }
}
