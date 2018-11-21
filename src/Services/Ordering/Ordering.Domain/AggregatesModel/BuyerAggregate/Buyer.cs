using System;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using Ordering.Domain.AggregatesModel.BuyerAggregate.Identity;

namespace Ordering.Domain.AggregatesModel.BuyerAggregate
{
    public class Buyer : AggregateRoot<Buyer, BuyerId>
    {
        public Buyer(BuyerId id) : base(id)
        {
        }
    }
}
