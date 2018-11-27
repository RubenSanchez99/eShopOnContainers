using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using eShopOnContainers.Services.IntegrationEvents.Events;
using MassTransit;
using EventFlow;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using Ordering.API.Application.Commands;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;

namespace Ordering.API.Application.IntegrationEvents.EventHandling
{
    public class UserCheckoutAcceptedIntegrationEventHandler : IConsumer<UserCheckoutAcceptedIntegrationEvent>
    {
        private readonly ICommandBus _commandBus;
        private readonly IPublishEndpoint _endpoint;
        public UserCheckoutAcceptedIntegrationEventHandler(IPublishEndpoint endpoint, ICommandBus commandBus)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
        }

        public async Task Consume(ConsumeContext<UserCheckoutAcceptedIntegrationEvent> context)
        {
            IExecutionResult result = ExecutionResult.Failed();

            // Send Integration event to clean basket once basket is converted to Order and before starting with the order creation process
            var orderStartedIntegrationEvent = new OrderStartedIntegrationEvent(context.Message.UserId);
            await _endpoint.Publish(orderStartedIntegrationEvent);

            if (context.Message.RequestId != Guid.Empty)
            {
                var createOrderCommand = new CreateOrderCommand(OrderId.New, context.Message.Basket.Items, context.Message.UserId, context.Message.UserName, context.Message.City, context.Message.Street, 
                    context.Message.State, context.Message.Country, context.Message.ZipCode,
                    context.Message.CardNumber, context.Message.CardHolderName, context.Message.CardExpiration,
                    context.Message.CardSecurityNumber, context.Message.CardTypeId);

                result = await _commandBus.PublishAsync(createOrderCommand, CancellationToken.None);
            }
        }
    }
}
