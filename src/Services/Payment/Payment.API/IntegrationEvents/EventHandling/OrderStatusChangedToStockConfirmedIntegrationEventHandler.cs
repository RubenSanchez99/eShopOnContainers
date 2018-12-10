namespace Payment.API.IntegrationEvents.EventHandling
{
    using MassTransit;
    using Microsoft.Extensions.Options;
    using eShopOnContainers.Services.IntegrationEvents.Events;
    using System.Threading.Tasks;

    public class OrderStatusChangedToStockConfirmedIntegrationEventHandler : 
        IConsumer<OrderStatusChangedToStockConfirmedIntegrationEvent>
    {
        private readonly IPublishEndpoint _endpoint;
        private readonly PaymentSettings _settings;

        public OrderStatusChangedToStockConfirmedIntegrationEventHandler(IPublishEndpoint endpoint, 
            IOptionsSnapshot<PaymentSettings> settings)
        {
            _endpoint = endpoint;
            _settings = settings.Value;
        }

        public async Task Consume(ConsumeContext<OrderStatusChangedToStockConfirmedIntegrationEvent> context)
        {
            //Business feature comment:
            // When OrderStatusChangedToStockConfirmed Integration Event is handled.
            // Here we're simulating that we'd be performing the payment against any payment gateway
            // Instead of a real payment we just take the env. var to simulate the payment 
            // The payment can be successful or it can fail

            if (_settings.PaymentSucceded)
            {
                await _endpoint.Publish(new OrderPaymentSuccededIntegrationEvent(context.Message.OrderId));
            }
            else
            {
                await _endpoint.Publish(new OrderPaymentFailedIntegrationEvent(context.Message.OrderId));
            }

            await Task.CompletedTask;
        }
    }
}