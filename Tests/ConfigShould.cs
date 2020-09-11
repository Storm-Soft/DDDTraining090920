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
        private static readonly UserProfileId UserProfileId1 = new UserProfileId(Guid.NewGuid());

        private IEnumerable<Event> InitializeDefaultConfigHistory()
            => Array.Empty<Event>();

        private IEnumerable<Event> InitializeEventStoreWithModel1()
        {
            var model1 = new Model("1");
            var optionA = new Option("A");
            return new Event[]
                {
                new ModelSelectedEvent(UserProfileId1, model1),
                new OptionAvailableEvent(UserProfileId1, new []{ optionA, new Option("B")}),
                new OptionSelectedEvent(UserProfileId1, model1, optionA)
                };
        }

        [Fact]
        public async Task Raise_Model_A_Event_Selected()
        {
            var config = new Config(UserProfileId1);
            var events = config.SelectModel1(InitializeDefaultConfigHistory());

            Assert.Contains(events, e =>e is ModelSelectedEvent modelSelectedEvent &&
                                                                modelSelectedEvent.Model.Id == "1");
        }

        [Fact(DisplayName ="Quand je choisis le model 1, j'ai bien les options A et B, et si ont choisit A on a un event sur A choisi")]
        public async Task Raise_Option_A_when_A_And_B_Available_And_A_Selected()
        {            
            var config = new Config(UserProfileId1);
            var events = config.SelectModel1(InitializeDefaultConfigHistory());
            Assert.Contains(events, e => e is OptionAvailableEvent optionAvailableEvent &&
                                                         optionAvailableEvent.Options.Any(option => option.Id == "A") &&
                                                         optionAvailableEvent.Options.Any(option => option.Id == "B"));
            Assert.Contains(events, e => e is OptionSelectedEvent optionSelectedEvent &&
                                                            optionSelectedEvent.Option.Id == "A" && 
                                                            optionSelectedEvent.Model.Id == "1");

        }

        [Fact]
        public async Task Raise_Option_B_When_A_Selected_And_Call_On_Selection_Option_B()
        {
            var config = new Config(UserProfileId1);
            var events = config.SelectOption(new Option("B"), InitializeEventStoreWithModel1());
            Assert.Contains(events, e => e is OptionSelectedEvent optionSelectedEvent &&
                                                          optionSelectedEvent.Option.Id == "B" &&
                                                          optionSelectedEvent.Model.Id == "1");
        }

        [Fact]
        public async Task Not_Raise_Option_A_When_A_Already_Selected()
        {
            var config = new Config(UserProfileId1);
            var events = config.SelectOption(new Option("A"), InitializeEventStoreWithModel1());
            Assert.Empty(events);
        }

        [Fact]
        public async Task Not_Raise_Option_C_When_Not_Available()
        {
            var config = new Config(UserProfileId1);            
            var events = config.SelectOption(new Option("C"), InitializeEventStoreWithModel1());
            Assert.Empty(events);
        }
    }

    //public class ConfigListProjectionShould
    //{
    //    [Fact]
    //    public void 
    //}
    

    public class ModelSelectedEvent : Event
    {
        public Model Model { get; }

        public ModelSelectedEvent(UserProfileId userId, Model model)
            : base(userId)
        {
            Model = model;
        }

    }
    public class OptionAvailableEvent : Event
    {

        public IEnumerable<Option> Options { get; }
        public OptionAvailableEvent(UserProfileId userId, IEnumerable<Option> options)
            : base(userId)
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

    public struct UserProfileId
    {
        public Guid UserId { get; set; }
        public UserProfileId(Guid userId)
        {
            UserId = userId;
        }
    }
    public class OptionSelectedEvent : Event
    {
        public Model Model { get; }
        public Option Option { get; }

        public OptionSelectedEvent(UserProfileId userId, Model model, Option option)
            : base(userId)
        {
            Model = model;
            Option = option;
        }

    }

    
    public sealed class Config
    {
        private readonly Model model1 = new Model("1");
        private readonly List<Event> uncommitedEvents = new List<Event>();
        private readonly UserProfileId userProfileId;
        
        public sealed class ConfigAggregateState
        {
            public Option? SelectedOption { get; private set; }
            public Model? SelectedModel { get; private set; }
            public IList<Option> AvailableOptions { get; private set; } = Array.Empty<Option>();

            public ConfigAggregateState(IEnumerable<Event> events)
            {
                foreach (var @event in events)
                {
                    switch (@event)
                    {
                        case ModelSelectedEvent modelSelectedEvent:
                            HandleModelSelectedEvent(modelSelectedEvent);
                            break;
                        case OptionSelectedEvent optionSelectedEvent:
                            HandleOptionSelectedEvent(optionSelectedEvent);
                            break;
                        case OptionAvailableEvent availableOptionEvent:
                            HandleOptionAvailableEvent(availableOptionEvent);
                            break;
                    }
                }
            }

            private void HandleOptionSelectedEvent(OptionSelectedEvent e)
                => SelectedOption = e.Option;
            
            private void HandleOptionAvailableEvent(OptionAvailableEvent e)
            => AvailableOptions = e.Options?.ToArray() ?? Array.Empty<Option>();

            private void HandleModelSelectedEvent(ModelSelectedEvent e)
            => SelectedModel = e.Model;
        }

        public Config(UserProfileId userProfileId)
        {
            this.userProfileId = userProfileId;
        }
        //public IEnumerable<Event> GetUncommitedEvents() => uncommitedEvents;
        //public void Commit() => uncommitedEvents.Clear();

        //private void AddPendingEvents(IEnumerable<Event> eventsToPublish)
        //{
        //    foreach (var @event in eventsToPublish)
        //        uncommitedEvents.Add(@event);
        //}

        public IEnumerable<Event> SelectModel1(IEnumerable<Event> previousEvents)
        => new Event[]{
                new ModelSelectedEvent(userProfileId, model1),
                new OptionAvailableEvent(userProfileId, new[] { new Option("A"), new Option("B") }),
                new OptionSelectedEvent(userProfileId, model1, new Option("A"))
            };


        public IEnumerable<Event> SelectOption(Option option, IEnumerable<Event> previousEvents)
        {
            var previousState = new ConfigAggregateState(previousEvents);
            if (!previousState.AvailableOptions.Contains(option))
                return Array.Empty<Event>();
            if (previousState.SelectedOption.HasValue &&
                previousState.SelectedOption.Value.Equals(option))
                return Array.Empty<Event>();
            return new[] { new OptionSelectedEvent(userProfileId, model1, option) };
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
        public UserProfileId UserId { get; }

        protected Event(UserProfileId userId)
        {
            UserId = userId;
        }
    }
}
