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
        public async Task Raise_Model_A_Event_Selected()
        {
            var eventStore = new EventStoreStub();
            var config = new Config(eventStore);
            await config.SelectModel1();

            Assert.Contains(eventStore.GetEvents(), e =>e is ModelSelectedEvent modelSelectedEvent &&
                                                        modelSelectedEvent.Model.Id == "1");
        }

        [Fact(DisplayName ="Quand je choisis le model 1, j'ai bien les options A et B, et si ont choisit A on a un event sur A choisi")]
        public async Task Raise_Option_A_when_A_And_B_Available_And_A_Selected()
        {
            var eventStore = new EventStoreStub();
            var config = new Config(eventStore);
            await config.SelectModel1();
            Assert.Contains(eventStore.GetEvents(), e => e is OptionAvailableEvent optionAvailableEvent &&
                                                         optionAvailableEvent.Options.Any(option => option.Id == "A") &&
                                                         optionAvailableEvent.Options.Any(option => option.Id == "B"));
            Assert.Contains(eventStore.GetEvents(), e => e is OptionSelectedEvent optionSelectedEvent &&
                                                            optionSelectedEvent.Option.Id == "A" && 
                                                            optionSelectedEvent.Model.Id == "1");

        }

        [Fact]
        public async Task Raise_Option_B_When_A_Selected_And_Call_On_Selection_Option_B()
        {
            var eventStore = new EventStoreStub();
            var config = new Config(eventStore);
            await config.SelectModel1();
            await config.SelectOption(new Option("B"));
            Assert.Contains(eventStore.GetEvents(), e => e is OptionSelectedEvent optionSelectedEvent &&
                                                          optionSelectedEvent.Option.Id == "B" &&
                                                          optionSelectedEvent.Model.Id == "1");
        }

        [Fact]
        public async Task Not_Raise_Option_A_When_A_Already_Selected()
        {
            var eventStore = new EventStoreStub();
            var config = new Config(eventStore);
            await config.SelectModel1();
            await config.SelectOption(new Option("A"));
            Assert.Single(eventStore.GetEvents(), e => e is OptionSelectedEvent optionSelectedEvent &&
                                                          optionSelectedEvent.Option.Id == "A" &&
                                                          optionSelectedEvent.Model.Id == "1");
        }

        [Fact]
        public async Task Not_Raise_Option_C_When_Not_Available()
        {
            var eventStore = new EventStoreStub();
            var config = new Config(eventStore);
            await config.SelectModel1();
            await config.SelectOption(new Option("C"));
            Assert.Single(eventStore.GetEvents(), e => e is OptionSelectedEvent optionSelectedEvent);
            var optionSelectedEvent = eventStore.GetEvents().First(e => e is OptionSelectedEvent optionSelectedEvent) as OptionSelectedEvent;
            Assert.True(optionSelectedEvent.Option.Id != "C");
        }
    }

    public class ModelSelectedEvent : Event
    {
        public Model Model { get; }

        public ModelSelectedEvent(Model model)
        {
            Model = model;
        }

    }
    public class OptionAvailableEvent : Event
    {

        public IEnumerable<Option> Options { get; }
        public OptionAvailableEvent(IEnumerable<Option> options)
        {
            Options = options;
        }
    }

    public struct Option
    {
        public string Id { get; }
        public Option(string id)
        {
            Id = id;
        }

        public override string ToString() => $"Id{Id}";
    }

    public class OptionSelectedEvent : Event
    {
        public Model Model { get; }
        public Option Option { get; }

        public OptionSelectedEvent(Model model, Option option)
        {
            Model = model;
            Option = option;
        }

    }

    public sealed class Config
    {
        private sealed class ConfigAggregateState
        {
            public Option? SelectedOption { get; private set; }
            public Model? SelectedModel { get; private set; }
            public IList<Option> AvailableOptions { get; private set; } = Array.Empty<Option>();

            public ConfigAggregateState(IEventStore store)
            {
                store.Register(HandleModelSelectedEvent);
                store.Register(HandleOptionAvailableEvent);
                store.Register(HandleOptionSelectedEvent);
            }

            private void HandleOptionSelectedEvent(Event e)
            {
                if (!(e is OptionSelectedEvent optionSelectedEvent))
                    return;
                SelectedOption = optionSelectedEvent.Option;
            }

            private void HandleOptionAvailableEvent(Event e)
            {
                if (!(e is OptionAvailableEvent optionAvailableEvent))
                    return;
                AvailableOptions = optionAvailableEvent.Options?.ToArray() ?? Array.Empty<Option>();
            }

            private void HandleModelSelectedEvent(Event e)
            {
                if (!(e is ModelSelectedEvent modelSelectedEvent))
                    return;
                SelectedModel = modelSelectedEvent.Model;
            }
        }

        private readonly ConfigAggregateState configAggregateState;
        private readonly IEventStore store;
        private readonly Model model1 = new Model("1");

        public Config(IEventStore store)
        {
            this.store = store;
            configAggregateState = new ConfigAggregateState(store);
        }

        private async Task PublishEvents(IEnumerable<Event> eventsToPublish)
        {
            foreach (var @event in eventsToPublish)
                await store.Publish(@event);
        }

        public Task SelectModel1()
            => PublishEvents(new Event[]{
                new ModelSelectedEvent(model1),
                new OptionAvailableEvent(new[] { new Option("A"), new Option("B") }),
                new OptionSelectedEvent(model1, new Option("A"))
            });
    

        public async Task SelectOption(Option option)
        {
            if (!configAggregateState.AvailableOptions.Contains(option))
                return;
            if (configAggregateState.SelectedOption.HasValue && 
                configAggregateState.SelectedOption.Value.Equals(option))
                return;
            await PublishEvents(new[] { new OptionSelectedEvent(model1, option) });
        }
    }

    public struct Model
    {
        public string Id { get; }
        
        public Model(string id)
        {
            Id = id;
        }
    }

    //public interface IAggregatePublisher
    //{
    //    Task Publish<TEvent>(TEvent @event) where TEvent : Event;
    //}

    public interface IEventStore
    {
        Task Publish<TEvent>(TEvent @event) where TEvent : Event;
        Task Register(Action<Event> eventHandler);
    }

    public class EventStoreStub : IEventStore
    {
        private readonly List<Event> events = new List<Event>();

        private List<Action<Event>> handlers = new List<Action<Event>>();

        public Task Publish<TEvent>(TEvent @event) where TEvent : Event
        {
            events.Add(@event);
            foreach (var eventHandler in handlers)
                    eventHandler(@event);            
            return Task.CompletedTask;
        }

        public List<Event> GetEvents() => events;

        public Task Register(Action<Event> eventHandler)
        {
           handlers.Add(eventHandler);
            return Task.CompletedTask;
        }
    }

    public abstract class Event
    {

    }
}
