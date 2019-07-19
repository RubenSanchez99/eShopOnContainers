namespace eShopOnContainers.Services.IntegrationEvents.Events
{
    public class OrderStatusChangedToStockConfirmedIntegrationEvent
    {
        public string OrderId { get; }

        public OrderStatusChangedToStockConfirmedIntegrationEvent(string orderId)
            => OrderId = orderId;
    }
}