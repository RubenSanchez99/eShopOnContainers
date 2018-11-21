using System;
using EventFlow.Entities;
using Ordering.Domain.AggregatesModel.BuyerAggregate.Identity;

namespace Ordering.Domain.AggregatesModel.BuyerAggregate
{
    public class PaymentMethod : Entity<PaymentMethodId>
    {
        public PaymentMethod(PaymentMethodId id) : base(id)
        {
        }
    }
}
