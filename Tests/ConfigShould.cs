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
        //private EventStoreStub InitializeEventStoreWithModel1()
        //{
        //    var model1 = new Model("1");
        //    var optionA = new Option("A");
        //    return new EventStoreStub(new Event[]
        //       {
        //        new ModelSelectedEvent(model1),
        //        new OptionAvailableEvent(new []{ optionA, new Option("B")}),
        //        new OptionSelectedEvent(model1, optionA)
        //       });
        //}

        private ConfigAggregateState CreateDefaultConfigState()
            => new ConfigAggregateState(null, null, null, new EventStoreStub());
        private ConfigAggregateState CreateStateWithModel1()
            => new ConfigAggregateState(new Option("A"), new Model("1"), new[] { new Option("A"), new Option("B") }, new EventStoreStub());

        [Fact]
        public async Task Raise_Model_A_Event_Selected()
        {
            var config = new Config(CreateDefaultConfigState());
            config.SelectModel1();

            Assert.Contains(config.GetUncommitedEvents(), e =>e is ModelSelectedEvent modelSelectedEvent &&
                                                                modelSelectedEvent.Model.Id == "1");
        }

        [Fact(DisplayName ="Quand je choisis le model 1, j'ai bien les options A et B, et si ont choisit A on a un event sur A choisi")]
        public async Task Raise_Option_A_when_A_And_B_Available_And_A_Selected()
        {          
            var config = new Config(CreateDefaultConfigState());
            config.SelectModel1();
            Assert.Contains(config.GetUncommitedEvents(), e => e is OptionAvailableEvent optionAvailableEvent &&
                                                         optionAvailableEvent.Options.Any(option => option.Id == "A") &&
                                                         optionAvailableEvent.Options.Any(option => option.Id == "B"));
            Assert.Contains(config.GetUncommitedEvents(), e => e is OptionSelectedEvent optionSelectedEvent &&
                                                            optionSelectedEvent.Option.Id == "A" && 
                                                            optionSelectedEvent.Model.Id == "1");

        }

        [Fact]
        public async Task Raise_Option_B_When_A_Selected_And_Call_On_Selection_Option_B()
        {
            var config = new Config(CreateStateWithModel1());
            config.SelectOption(new Option("B"));
            Assert.Contains(config.GetUncommitedEvents(), e => e is OptionSelectedEvent optionSelectedEvent &&
                                                          optionSelectedEvent.Option.Id == "B" &&
                                                          optionSelectedEvent.Model.Id == "1");
        }

        [Fact]
        public async Task Not_Raise_Option_A_When_A_Already_Selected()
        {
            var config = new Config(CreateStateWithModel1());
            config.SelectOption(new Option("A"));
            Assert.Empty(config.GetUncommitedEvents());
        }

        [Fact]
        public async Task Not_Raise_Option_C_When_Not_Available()
        {
            var config = new Config(CreateStateWithModel1());            
            config.SelectOption(new Option("C"));
            Assert.Empty(config.GetUncommitedEvents());
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

    public sealed class ConfigAggregateState
    {
        public Option? SelectedOption { get; private set; }
        public Model? SelectedModel { get; private set; }
        public IList<Option> AvailableOptions { get; private set; } = Array.Empty<Option>();

        public ConfigAggregateState(Option? selectedOption, 
            Model? selectedModel, 
            IList<Option> availableOptions,
            IEventStore store)
        {
            SelectedOption = selectedOption;
            SelectedModel = selectedModel;
            AvailableOptions = availableOptions;
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
    public sealed class Config
    {
        

        private readonly ConfigAggregateState configAggregateState;
        private readonly Model model1 = new Model("1");
        private readonly List<Event> uncommitedEvents = new List<Event>();

        public Config(ConfigAggregateState configState)
        {
            configAggregateState = configState;
        }

        public IEnumerable<Event> GetUncommitedEvents() => uncommitedEvents;
        public void Commit() => uncommitedEvents.Clear();

        private void PublishEvents(IEnumerable<Event> eventsToPublish)
        {
            foreach (var @event in eventsToPublish)
                uncommitedEvents.Add(@event);
        }

        public void SelectModel1()
            => PublishEvents(new Event[]{
                new ModelSelectedEvent(model1),
                new OptionAvailableEvent(new[] { new Option("A"), new Option("B") }),
                new OptionSelectedEvent(model1, new Option("A"))
            });
    

        public void SelectOption(Option option)
        {
            if (!configAggregateState.AvailableOptions.Contains(option))
                return;
            if (configAggregateState.SelectedOption.HasValue && 
                configAggregateState.SelectedOption.Value.Equals(option))
                return;
            PublishEvents(new[] { new OptionSelectedEvent(model1, option) });
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

        public EventStoreStub(IEnumerable<Event> alreadyPlayedEvents=null)
        {
            foreach (var @event in (alreadyPlayedEvents ?? Enumerable.Empty<Event>()))
                events.Add(@event);
        }

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
