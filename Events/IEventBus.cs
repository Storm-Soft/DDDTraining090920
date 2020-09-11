using System;
using System.Threading.Tasks;

namespace DDDTraining.Tests
{
    //public interface IAggregatePublisher
    //{
    //    Task Publish<TEvent>(TEvent @event) where TEvent : Event;
    //}

    public interface IEventBus
    {
        Task Publish<TEvent>(TEvent @event) where TEvent : Event;
        Task Subscribe(Action<Event> eventHandler);
    }
}
