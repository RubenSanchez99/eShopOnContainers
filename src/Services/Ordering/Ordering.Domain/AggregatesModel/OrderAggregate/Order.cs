using System;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;

namespace Ordering.Domain.AggregatesModel.OrderAggregate
{
    public class Order : AggregateRoot<Order, OrderId>
    {
        public Order(OrderId id) : base(id)
        {
        }
    }
}
