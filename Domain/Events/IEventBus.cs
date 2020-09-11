using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDDTraining.Tests
{
    //public interface IAggregatePublisher
    //{
    //    Task Publish<TEvent>(TEvent @event) where TEvent : Event;
    //}

    public interface IEventBus
    {
        Task Publish(IEnumerable<IEvent> events);
        Task Subscribe(Action<IEvent> eventHandler);
    }
}
