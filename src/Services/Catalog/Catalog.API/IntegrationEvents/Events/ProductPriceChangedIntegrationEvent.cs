namespace Catalog.API.IntegrationEvents.Events
{
    public interface ProductPriceChangedIntegrationEvent
    {
        int ProductId { get; }

        decimal NewPrice { get; }

        decimal OldPrice { get; }
    }
}