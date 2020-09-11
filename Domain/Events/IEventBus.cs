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
        Task Publish(IEnumerable<Event> events);
        Task Subscribe(Action<Event> eventHandler);
    }
}
