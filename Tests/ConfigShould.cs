using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DDDTraining.Tests
{
    public class ConfigShould
    {
        [Fact]
        public async Task Raise_Option_A_Selected()
        {
            var eventStore = new EventStoreStub();
            var config = new Config(eventStore);
            await config.SelectModelA();

            Assert.Contains(eventStore.GetEvents(), e =>e is ModelSelectedEvent modelSelectedEvent &&
                                                        modelSelectedEvent.Model == "A");
        }
    }

    public class ModelSelectedEvent : Event
    {
        public string Model { get; }

        public ModelSelectedEvent(string model)
        {
            Model = model;
        }

    }

    public sealed class Config
    {
        private readonly IEventStore store;
        public Config(IEventStore store)
        {
            this.store = store;
        }

        public Task SelectModelA()
            => store.Publish(new ModelSelectedEvent("A"));
    }
    public interface IEventStore
    {
        Task Publish<TEvent>(TEvent @event) where TEvent : Event;
    }
    public class EventStoreStub : IEventStore
    {
        private readonly List<Event> events = new List<Event>();

        public Task Publish<TEvent>(TEvent @event) where TEvent : Event
        {
            events.Add(@event);
            return Task.CompletedTask;
        }

        public List<Event> GetEvents() => events;
    }

    public abstract class Event
    {

    }
}
