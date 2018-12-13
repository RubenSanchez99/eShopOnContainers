using System;
using EventFlow.Core;
using EventFlow.ValueObjects;
using Newtonsoft.Json;

namespace Ordering.Domain.AggregatesModel.BuyerAggregate.Identity
{
    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class BuyerId : IIdentity
    {
        private Guid Id;

        public BuyerId(Guid guid)
        {
            Id = guid;
        }

        [JsonConstructor]
        public BuyerId(string Value)
        {
            Id = Guid.Parse(Value);
        }

        public string Value => Id.ToString();

        public static bool operator== (BuyerId obj1, BuyerId obj2)
        {
            return (obj1.Id == obj2.Id);
        }

        public static bool operator!= (BuyerId obj1, BuyerId obj2)
        {
            return (obj1.Id != obj2.Id);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var b2 = (BuyerId)obj;
            return (this.Id == b2.Id);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
