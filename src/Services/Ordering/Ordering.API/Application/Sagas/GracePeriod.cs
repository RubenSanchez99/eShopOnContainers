using System;
using Automatonymous;

namespace Ordering.API.Application.Sagas
{
    public class GracePeriod : SagaStateMachineInstance
    {
        public string CurrentState { get; set; }
        public Guid CorrelationId { get ; set; }
        public string OrderId { get; set; }
        public Guid? ExpirationId { get; set; }
    }
}