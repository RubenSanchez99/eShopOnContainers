using System;
using System.Collections.Generic;
using System.Linq;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Core;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using Ordering.Domain.AggregatesModel.BuyerAggregate;
using Ordering.Domain.AggregatesModel.BuyerAggregate.Identity;
using Ordering.Domain.Events;
using Ordering.Domain.Exceptions;
using Newtonsoft.Json;

namespace Ordering.Domain.AggregatesModel.OrderAggregate
{
    public class Order : AggregateRoot<Order, OrderId>,
    IEmit<OrderStartedDomainEvent>,
    IEmit<OrderItemAddedDomainEvent>,
    //IEmit<OrderItemUnitsAddedDomainEvent>,
    //IEmit<OrderItemNewDiscountSetDomainEvent>,
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

        public OrderStatus OrderStatus => OrderStatus.FromValue<OrderStatus>(_orderStatusId);
        private int _orderStatusId;

        private string _description;


        // Draft orders have this set to true. Currently we don't check anywhere the draft status of an Order, but we could do it if needed
        private bool _isDraft;

        // DDD Patterns comment
        // Using a private collection field, better for DDD Aggregate's encapsulation
        // so OrderItems cannot be added from "outside the AggregateRoot" directly to the collection,
        // but only through the method OrderAggrergateRoot.AddOrderItem() which includes behaviour.
        private List<OrderItem> _orderItems;
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

        private PaymentMethodId _paymentMethodId;


        /*
        public static Order NewDraft()
        {
            var order = new Order(OrderId.New);
            order._isDraft = true;
            return order;
        }*/

        public Order(OrderId id) : base(id)
        {
            _orderItems = new List<OrderItem>();
            _isDraft = false;
        }

        public IExecutionResult Create(string userId, string userName, Address address, int cardTypeId, string cardNumber, string cardSecurityNumber,
                string cardHolderName, DateTime cardExpiration, IEnumerable<OrderItem> items, string buyerId = null)
        {
            var itemList = new List<OrderItem>();
            foreach (var item in items)
            {
                var existingOrderForProduct = itemList.Where(o => o.ProductId == item.ProductId)
                .SingleOrDefault();

                if (existingOrderForProduct != null)
                {
                    //if previous line exist modify it with higher discount  and units..

                    if (item.Discount > existingOrderForProduct.Discount)
                    {
                        existingOrderForProduct.SetNewDiscount(item.Discount);
                        //Emit(new OrderItemNewDiscountSetDomainEvent(item.ProductId, item.Discount));
                    }

                    existingOrderForProduct.AddUnits(item.Units);
                    //Emit(new OrderItemUnitsAddedDomainEvent(item.ProductId, item.Units));
                }
                else
                {
                    //add validated new order item
                    itemList.Add(item);
                    //Emit(new OrderItemAddedDomainEvent(item.ProductId, item.GetOrderItemProductName(), item.GetPictureUri(), item.UnitPrice, item.Discount, item.Units));
                }
            }

            var orderStarted = new OrderStartedDomainEvent(DateTime.UtcNow, address, userId, userName, cardTypeId, cardNumber, cardSecurityNumber, cardHolderName, cardExpiration, itemList);
            Console.Out.WriteLine(JsonConvert.SerializeObject(orderStarted, Formatting.Indented));
            Emit(orderStarted);
            return ExecutionResult.Success();
        }

        public void Apply(OrderStartedDomainEvent aggregateEvent)
        {
            //_buyerId = aggregateEvent._buyerId;
            //_paymentMethodId = aggregateEvent.PaymentMethodId;
            _orderStatusId = OrderStatus.Submitted.Id;
            _orderDate = aggregateEvent.OrderDate;
            Address = aggregateEvent.Address;
            _orderItems = aggregateEvent.Items.ToList();
        }

        public IExecutionResult AddOrderItem(int productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units = 1)
        {
            

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

        public void SetBuyerId(BuyerId buyerId)
        {
            Emit(new OrderBuyerChangedDomainEvent(buyerId));
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
            Console.Out.WriteLine("Setting awaiting validation status for order with value " + OrderStatus.FromValue<OrderStatus>(_orderStatusId).Name);
            if (_orderStatusId == OrderStatus.Submitted.Id)
            {
                Console.Out.WriteLine("Setting OrderStatusChangedToAwaitingValidationDomainEvent for order with value " + OrderStatus.FromValue<OrderStatus>(_orderStatusId).Name);
                Emit(new OrderStatusChangedToAwaitingValidationDomainEvent(_orderItems));
            }
        }

        public void Apply(OrderStatusChangedToAwaitingValidationDomainEvent aggregateEvent)
        {
            _orderStatusId = OrderStatus.AwaitingValidation.Id;
        }

        public void SetStockConfirmedStatus()
        {
            Console.Out.WriteLine("Setting stock confirmed with status " + _orderStatusId);
            if (_orderStatusId == OrderStatus.AwaitingValidation.Id)
            {
                Console.Out.WriteLine("Emitin OrderStatusChangedToStockConfirmed");
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
                    .Select(c => c.ProductName);

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
            return _orderItems.Sum(o => o.Units * o.UnitPrice);
        }
    }
}
