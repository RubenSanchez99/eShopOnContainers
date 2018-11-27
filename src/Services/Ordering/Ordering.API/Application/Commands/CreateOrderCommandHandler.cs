using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;

namespace Ordering.API.Application.Commands
{
    public class CreateOrderCommandHandler : CommandHandler<Order, OrderId, IExecutionResult, CreateOrderCommand>
    {
        public override Task<IExecutionResult> ExecuteCommandAsync(Order aggregate, CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var address = new Address(command.Street, command.City, command.State, command.Country, command.ZipCode);

            var executionResult = aggregate.Create(OrderId.New, command.UserId, command.UserName, address, command.CardTypeId, command.CardNumber, command.CardSecurityNumber, command.CardHolderName, command.CardExpiration);

            foreach (var item in command.OrderItems)
            {
                aggregate.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
            }

            return Task.FromResult(executionResult);
        }
    }
}
