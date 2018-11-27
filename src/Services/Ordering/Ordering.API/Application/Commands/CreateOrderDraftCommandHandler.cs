using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate.Identity;

namespace Ordering.API.Application.Commands
{
    public class CreateOrderDraftCommandHandler : CommandHandler<Order, OrderId, IExecutionResult, CreateOrderDraftCommand>
    {
        public override Task<IExecutionResult> ExecuteCommandAsync(Order aggregate, CreateOrderDraftCommand command, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
