using System;
using EventFlow.Core;

namespace Ordering.Domain.AggregatesModel.BuyerAggregate.Identity
{
    public class PaymentMethodId : Identity<PaymentMethodId>
    {
        public PaymentMethodId(string value) : base(value)
        {
        }
    }
}
