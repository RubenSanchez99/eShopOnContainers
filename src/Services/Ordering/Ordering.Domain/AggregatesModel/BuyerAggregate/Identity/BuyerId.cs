using System;
using EventFlow.Core;

namespace Ordering.Domain.AggregatesModel.BuyerAggregate.Identity
{
    public class BuyerId : IIdentity
    {
        private Guid _value;

        public BuyerId(Guid guid)
        {
            _value = guid;
        }

        public string Value => _value.ToString();
    }
}
