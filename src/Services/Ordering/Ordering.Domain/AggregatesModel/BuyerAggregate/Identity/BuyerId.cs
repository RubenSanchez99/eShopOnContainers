using System;
using EventFlow.Core;

namespace Ordering.Domain.AggregatesModel.BuyerAggregate.Identity
{
    public class BuyerId : Identity<BuyerId>
    {
        public BuyerId(string value) : base(value)
        {
        }
    }
}
