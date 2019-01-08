using System;
using Automatonymous;
using eShopOnContainers.Services.IntegrationEvents.Events;
using EventFlow;
using Ordering.API.Application.Commands;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.API.Application.Sagas
{
    public class GracePeriodStateMachine :
    MassTransitStateMachine<GracePeriod>
    {
        private readonly ICommandBus _commandBus;
        public GracePeriodStateMachine(ICommandBus commandBus)
        {
            _commandBus = commandBus;

            InstanceState(x => x.CurrentState);

            Event(() => OrderStarted, x => x.CorrelateBy(gracePeriod => gracePeriod.OrderId, context => context.Message.OrderId).SelectId(context => Guid.NewGuid()));

            Event(() => OrderStockConfirmed, x => x.CorrelateBy(gracePeriod => gracePeriod.OrderId, context => context.Message.OrderId));

            Event(() => OrderStockRejected, x => x.CorrelateBy(gracePeriod => gracePeriod.OrderId, context => context.Message.OrderId));

            Event(() => OrderPaymentSucceded, x => x.CorrelateBy(gracePeriod => gracePeriod.OrderId, context => context.Message.OrderId));

            Event(() => OrderPaymentFailed, x => x.CorrelateBy(gracePeriod => gracePeriod.OrderId, context => context.Message.OrderId));

            Event(() => OrderCanceled, x => x.CorrelateBy(gracePeriod => gracePeriod.OrderId, context => context.Message.OrderId));

            Event(() => OrderStockSent, x => x.CorrelateBy(gracePeriod => gracePeriod.OrderId, context => context.Message.OrderId));

            Schedule(() => GracePeriodFinished, x => x.ExpirationId, x => 
            {
                x.Delay = TimeSpan.FromMinutes(1);
                x.Received = e => e.CorrelateBy(gracePeriod => gracePeriod.OrderId, context => context.Message.OrderId);
            });

            Initially(
                When(OrderStarted)
                    .Then(context => context.Instance.OrderId = context.Data.OrderId)
                    .TransitionTo(AwaitingValidation)
                    .Publish(context => new GracePeriodConfirmedIntegrationEvent(context.Data.OrderId))
                    .Schedule(GracePeriodFinished, context => new GracePeriodExpired(context.Data.OrderId))
            );

            During(AwaitingValidation,
                When(OrderStockConfirmed)
                    .TransitionTo(StockConfirmed),
                When(OrderStockRejected)
                    .Unschedule(GracePeriodFinished)
                    .TransitionTo(Failed),
                When(GracePeriodFinished.Received)
                    .TransitionTo(Failed)
            );

            During(StockConfirmed,
                When(OrderPaymentSucceded)
                    .Unschedule(GracePeriodFinished)
                    .TransitionTo(Validated),
                When(OrderPaymentFailed)
                    .Unschedule(GracePeriodFinished)
                    .TransitionTo(Failed),
                When(GracePeriodFinished.Received)
                    .TransitionTo(Failed)
            );

            During(PaymentSucceded,
                When(OrderStockConfirmed)
                    .Unschedule(GracePeriodFinished)
                    .TransitionTo(Validated),
                When(OrderStockRejected)
                    .Unschedule(GracePeriodFinished)
                    .Finalize(),
                When(GracePeriodFinished.Received)
                    .TransitionTo(Failed)
            );

            During(Validated,
                When(OrderStockSent)
                    .ThenAsync(context => _commandBus.PublishAsync(new ShipOrderCommand(new OrderId(context.Instance.OrderId), 0), CancellationToken.None))
                    .Finalize()
            );


            WhenEnter(Failed, 
                x => x.ThenAsync(context => _commandBus.PublishAsync(new CancelOrderCommand(new OrderId(context.Instance.OrderId), 0), CancellationToken.None))
                      .Finalize()
            );

            During(Final,
                When(GracePeriodFinished.AnyReceived).Finalize()
            );
        }

        public Event<OrderStartedIntegrationEvent> OrderStarted { get; private set; }
        public Event<OrderStockConfirmedIntegrationEvent> OrderStockConfirmed { get; private set; }
        public Event<OrderStockRejectedIntegrationEvent> OrderStockRejected { get; private set; }
        public Event<OrderPaymentSuccededIntegrationEvent> OrderPaymentSucceded { get; private set; }
        public Event<OrderPaymentFailedIntegrationEvent> OrderPaymentFailed { get; private set; }
        public Event<OrderStatusChangedToCancelledIntegrationEvent> OrderCanceled { get; private set; }
        public Event<OrderStockSentForOrderIntegrationEvent> OrderStockSent { get; private set; }
        
        public Schedule<GracePeriod, GracePeriodExpired> GracePeriodFinished { get; private set; }

        public State AwaitingValidation { get; private set; }
        public State StockConfirmed { get; private set; }
        public State PaymentSucceded { get; private set; }
        public State Validated { get; private set; }
        public State Failed { get; private set; }
        
        public class GracePeriodExpired
        {
            public string OrderId { get; private set; }
            
            public GracePeriodExpired(string orderId) => OrderId = orderId;
        }
    }
}
