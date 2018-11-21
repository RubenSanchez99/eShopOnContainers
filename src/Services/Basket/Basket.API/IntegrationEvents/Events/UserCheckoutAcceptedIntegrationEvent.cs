using Basket.API.Model;
using System;

namespace eShopOnContainers.Services.IntegrationEvents.Events
{
    public interface UserCheckoutAcceptedIntegrationEvent
    {
        string UserId { get; }

        int OrderNumber { get; }

        string City { get; }

        string Street { get; }

        string State { get; }

        string Country { get; }

        string ZipCode { get; }

        string CardNumber { get; }

        string CardHolderName { get; }

        DateTime CardExpiration { get; }

        string CardSecurityNumber { get; }

        int CardTypeId { get; }

        string Buyer { get; }

        Guid RequestId { get; }

        CustomerBasket Basket { get; }
    }
}
