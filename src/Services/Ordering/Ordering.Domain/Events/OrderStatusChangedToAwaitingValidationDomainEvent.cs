namespace Ordering.Domain.Events
{
    using Ordering.Domain.AggregatesModel.OrderAggregate;
    using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
    using EventFlow.Aggregates;
    using System.Collections.Generic;

    /// <summary>
    /// Event used when the grace period order is confirmed
    /// </summary>
    public class OrderStatusChangedToAwaitingValidationDomainEvent
         : IAggregateEvent<Order, OrderId>
    {
        public Order Order { get; }
        public IEnumerable<OrderItem> OrderItems { get; }

        public OrderStatusChangedToAwaitingValidationDomainEvent(Order order,
            IEnumerable<OrderItem> orderItems)
        {
            Order = order;
            OrderItems = orderItems;
        }
    }
}