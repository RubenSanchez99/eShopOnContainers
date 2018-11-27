using System;
using System.Collections.Generic;
using System.Linq;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Core;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using Ordering.Domain.AggregatesModel.BuyerAggregate.Identity;
using Ordering.Domain.Events;
using Ordering.Domain.Exceptions;

namespace Ordering.Domain.AggregatesModel.OrderAggregate
{
    public class Order : AggregateRoot<Order, OrderId>,
    IEmit<OrderStartedDomainEvent>,
    IEmit<OrderItemAddedDomainEvent>,
    IEmit<OrderItemUnitsAddedDomainEvent>,
    IEmit<OrderItemNewDiscountSetDomainEvent>,
    IEmit<OrderPaymentMethodChangedDomainEvent>,
    IEmit<OrderBuyerChangedDomainEvent>,
    IEmit<OrderStatusChangedToAwaitingValidationDomainEvent>,
    IEmit<OrderStatusChangedToStockConfirmedDomainEvent>,
    IEmit<OrderStatusChangedToPaidDomainEvent>,
    IEmit<OrderShippedDomainEvent>,
    IEmit<OrderCancelledDomainEvent>
    {

        // DDD Patterns comment
        // Using private fields, allowed since EF Core 1.1, is a much better encapsulation
        // aligned with DDD Aggregates and Domain Entities (Instead of properties and property collections)
        private DateTime _orderDate;

        // Address is a Value Object pattern example persisted as EF Core 2.0 owned entity
        public Address Address { get; private set; }

        public BuyerId GetBuyerId => _buyerId;
        private BuyerId _buyerId;

        public OrderStatus OrderStatus { get; private set; }
        private int _orderStatusId;

        private string _description;


        // Draft orders have this set to true. Currently we don't check anywhere the draft status of an Order, but we could do it if needed
        private bool _isDraft;

        // DDD Patterns comment
        // Using a private collection field, better for DDD Aggregate's encapsulation
        // so OrderItems cannot be added from "outside the AggregateRoot" directly to the collection,
        // but only through the method OrderAggrergateRoot.AddOrderItem() which includes behaviour.
        private readonly List<OrderItem> _orderItems;
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

        private PaymentMethodId _paymentMethodId;



        public static Order NewDraft()
        {
            var order = new Order(OrderId.New);
            order._isDraft = true;
            return order;
        }

        public Order(OrderId id) : base(id)
        {
            _orderItems = new List<OrderItem>();
            _isDraft = false;
        }

        public IExecutionResult Create(OrderId id, string userId, string userName, Address address, int cardTypeId, string cardNumber, string cardSecurityNumber,
                string cardHolderName, DateTime cardExpiration, string buyerId = null, PaymentMethodId paymentMethodId = null)
        {
            Emit(new OrderStartedDomainEvent(paymentMethodId, DateTime.UtcNow, address, userId, userName, cardTypeId, cardNumber, cardSecurityNumber, cardHolderName, cardExpiration));

            return ExecutionResult.Success();
        }

        public void Apply(OrderStartedDomainEvent aggregateEvent)
        {
            //_buyerId = aggregateEvent._buyerId;
            _paymentMethodId = aggregateEvent.PaymentMethodId;
            _orderStatusId = OrderStatus.Submitted.Id;
            _orderDate = aggregateEvent.OrderDate;
            Address = aggregateEvent.Address;
        }

        public IExecutionResult AddOrderItem(int productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units = 1)
        {
            var existingOrderForProduct = _orderItems.Where(o => o.ProductId == productId)
                .SingleOrDefault();

            if (existingOrderForProduct != null)
            {
                //if previous line exist modify it with higher discount  and units..

                if (discount > existingOrderForProduct.GetCurrentDiscount())
                {
                    Emit(new OrderItemNewDiscountSetDomainEvent(productId, discount));
                }

                Emit(new OrderItemUnitsAddedDomainEvent(productId, units));
            }
            else
            {
                //add validated new order item
                Emit(new OrderItemAddedDomainEvent(productId, productName, pictureUrl, unitPrice, discount, units));
            }

            return ExecutionResult.Success();
        }

        public void Apply(OrderItemAddedDomainEvent aggregateEvent)
        {
            var orderItem = new OrderItem(OrderItemId.New, aggregateEvent.ProductId, aggregateEvent.ProductName, aggregateEvent.UnitPrice, aggregateEvent.Discount, aggregateEvent.PictureUrl, aggregateEvent.Units);
            _orderItems.Add(orderItem);
        }

        public void Apply(OrderItemUnitsAddedDomainEvent aggregateEvent)
        {
            var existingOrderForProduct = _orderItems.Where(o => o.ProductId == aggregateEvent.ProductId)
                .SingleOrDefault();
            existingOrderForProduct.AddUnits(aggregateEvent.UnitsAdded);
        }

        public void Apply(OrderItemNewDiscountSetDomainEvent aggregateEvent)
        {
            var existingOrderForProduct = _orderItems.Where(o => o.ProductId == aggregateEvent.ProductId)
                .SingleOrDefault();
            existingOrderForProduct.SetNewDiscount(aggregateEvent.NewDiscount);
        }

        public void SetPaymentId(PaymentMethodId id)
        {
            Emit(new OrderPaymentMethodChangedDomainEvent(id));
        }

        public void SetBuyerId(BuyerId id)
        {
            Emit(new OrderBuyerChangedDomainEvent(id));
        }

        public void Apply(OrderBuyerChangedDomainEvent aggregateEvent)
        {
            _buyerId = aggregateEvent.BuyerId;
        }

        public void Apply(OrderPaymentMethodChangedDomainEvent aggregateEvent)
        {
            _paymentMethodId = aggregateEvent.PaymentMethodId;
        }

        public void SetAwaitingValidationStatus()
        {
            if (_orderStatusId == OrderStatus.Submitted.Id)
            {
                Emit(new OrderStatusChangedToAwaitingValidationDomainEvent(this, _orderItems));
            }
        }

        public void Apply(OrderStatusChangedToAwaitingValidationDomainEvent aggregateEvent)
        {
            _orderStatusId = OrderStatus.AwaitingValidation.Id;
        }

        public void SetStockConfirmedStatus()
        {
            if (_orderStatusId == OrderStatus.AwaitingValidation.Id)
            {
                Emit(new OrderStatusChangedToStockConfirmedDomainEvent());
            }
        }

        public void Apply(OrderStatusChangedToStockConfirmedDomainEvent aggregateEvent)
        {
            _orderStatusId = OrderStatus.StockConfirmed.Id;
            _description = "All the items were confirmed with available stock.";
        }

        public void SetPaidStatus()
        {
            if (_orderStatusId == OrderStatus.StockConfirmed.Id)
            {
                Emit(new OrderStatusChangedToPaidDomainEvent(OrderItems));                
            }
        }

        public void Apply(OrderStatusChangedToPaidDomainEvent aggregateEvent)
        {
            _orderStatusId = OrderStatus.Paid.Id;
            _description = "The payment was performed at a simulated \"American Bank checking bank account endinf on XX35071\"";
        }

        public IExecutionResult SetShippedStatus()
        {
            if (_orderStatusId != OrderStatus.Paid.Id)
            {
                StatusChangeException(OrderStatus.Shipped);
            }

            Emit(new OrderShippedDomainEvent(this));

            return ExecutionResult.Success();
        }

        public void Apply(OrderShippedDomainEvent aggregateEvent)
        {
            _orderStatusId = OrderStatus.Shipped.Id;
            _description = "The order was shipped.";
        }

        public IExecutionResult SetCancelledStatus()
        {
            if (_orderStatusId == OrderStatus.Paid.Id ||
                _orderStatusId == OrderStatus.Shipped.Id)
            {
                StatusChangeException(OrderStatus.Cancelled);
            }

            Emit(new OrderCancelledDomainEvent(this, $"The order was cancelled."));
            return ExecutionResult.Success();
        }

        public void Apply(OrderCancelledDomainEvent aggregateEvent)
        {
            _orderStatusId = OrderStatus.Cancelled.Id;
            _description = aggregateEvent.Description;
        }

        public void SetCancelledStatusWhenStockIsRejected(IEnumerable<int> orderStockRejectedItems)
        {
            if (_orderStatusId == OrderStatus.AwaitingValidation.Id)
            {
                var itemsStockRejectedProductNames = OrderItems
                    .Where(c => orderStockRejectedItems.Contains(c.ProductId))
                    .Select(c => c.GetOrderItemProductName());

                var itemsStockRejectedDescription = string.Join(", ", itemsStockRejectedProductNames);
                var description = $"The product items don't have stock: ({itemsStockRejectedDescription}).";

                Emit(new OrderCancelledDomainEvent(this, description));
            }
        }

        private void StatusChangeException(OrderStatus orderStatusToChange)
        {
            throw new OrderingDomainException($"Is not possible to change the order status from {OrderStatus.Name} to {orderStatusToChange.Name}.");
        }

        public decimal GetTotal()
        {
            return _orderItems.Sum(o => o.GetUnits() * o.GetUnitPrice());
        }
    }
}
