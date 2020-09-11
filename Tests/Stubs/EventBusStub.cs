using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDDTraining.Tests
{
    public class EventBusStub : IEventBus
    {
        private readonly IEventStore store;
        private List<Action<Event>> handlers = new List<Action<Event>>();

        public EventBusStub(IEventStore store)
        {
            this.store = store;
        }

        public async Task Publish(IEnumerable<Event> events)
        {
            await store.Persist(events);
            foreach(var @event in events)
                foreach (var eventHandler in handlers)
                    eventHandler(@event);
        }

        public Task Subscribe(Action<Event> eventHandler)
        {
           handlers.Add(eventHandler);
            return Task.CompletedTask;
        }
    }
}
