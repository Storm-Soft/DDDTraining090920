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

    public class ConfigListProjectionShould
    {
        private static readonly UserProfileId UserProfileId1 = new UserProfileId(Guid.NewGuid());
        private static readonly Model model1 = new Model("1");
        private IEnumerable<Event> InitializeEventHistory(UserProfileId userProfileId)
        {
            var model1 = new Model("1");
            var optionA = new Option("A");
            return new Event[]
                {
                new ModelSelectedEvent(userProfileId, model1),
                new OptionAvailableEvent(userProfileId, new []{ optionA, new Option("B")}),
                new OptionSelectedEvent(userProfileId, model1, optionA)
                };
        }
        [Fact]
        public async Task When_A_Model_Is_Selected_Projection_Is_Updated()
        {
            //var events = InitializeEventHistory(UserProfileId1);
            var eventBus = new EventBusStub(new EventStoreStub());
            var projection = new ConfigListProjection(eventBus);
            await eventBus.Publish(new ModelSelectedEvent(UserProfileId1, model1));            

            var foundModel = projection.GetModelsByUserProfiles().FirstOrDefault(model => model.Key.Equals(UserProfileId1)).Value;

            Assert.Equal(model1, foundModel);
        }
    }

    public class SelectModelCommandHandlerShould
    {
        private static readonly UserProfileId UserProfileId1 = new UserProfileId(Guid.NewGuid());
        private static readonly Model model1 = new Model("1");

        [Fact]
        public async Task When_Publish_Event_On_Aggregate_Then_Projection_Is_Updated()
        {
            var eventStore = new EventStoreStub();
            var eventBus = new EventBusStub(eventStore);
            var projection = new ConfigListProjection(eventBus);
            var commandHandler = new SelectModelCommandHandler(eventBus, eventStore);

            var foundSelected = projection.GetModelsByUserProfiles().FirstOrDefault();
            Assert.Equal(default, foundSelected.Value);

            await commandHandler.Execute(UserProfileId1);
            
            foundSelected = projection.GetModelsByUserProfiles().FirstOrDefault();
            Assert.Equal(model1, foundSelected.Value);
        }

        [Fact]
        public async Task When_Publish_Event_On_Aggregate_Then_Projection_Is_Updated()
        {
            var eventStore = new EventStoreStub(new[] { new ModelSelectedEvent(UserProfileId1, new Model("2")) });
            var eventBus = new EventBusStub(eventStore);
            var projection = new ConfigListProjection(eventBus);
            var commandHandler = new SelectModelCommandHandler(eventBus, eventStore);

            var foundSelected = projection.GetModelsByUserProfiles().FirstOrDefault();
            Assert.Equal(default, foundSelected.Value);

            await commandHandler.Execute(UserProfileId1);

            foundSelected = projection.GetModelsByUserProfiles().FirstOrDefault();
            Assert.Equal(model1, foundSelected.Value);
        }
    }

    public class ConfigListProjection
    {
        private readonly Dictionary<UserProfileId,Model> selectedModels = new Dictionary<UserProfileId, Model>();

        public ConfigListProjection(IEventBus eventBus)
        {
            eventBus.Subscribe(Apply);
        }

        private void Apply(Event @event)
        {
            switch(@event)
            {
                case ModelSelectedEvent modelSelectedEvent:
                    selectedModels[modelSelectedEvent.UserId] = modelSelectedEvent.Model;
                    break;
            }
        }

        public IDictionary<UserProfileId, Model> GetModelsByUserProfiles() => selectedModels;
    }

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

    class SelectModelCommandHandler
    {
        private readonly IEventBus eventBus;
        private readonly IEventStore eventStore;

        public SelectModelCommandHandler(IEventBus eventBus, IEventStore eventStore)
        {
            this.eventBus = eventBus;
            this.eventStore = eventStore;
        }

        public async Task Execute(UserProfileId userProfileId)
        {
            var previousEvents = await eventStore.LoadEvents(userProfileId);
            var config = new Config(userProfileId);            
            var events = config.SelectModel1(previousEvents);
            foreach (var @event in events)
                await eventBus.Publish(@event);
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

    public interface IEventBus
    {
        Task Publish<TEvent>(TEvent @event) where TEvent : Event;
        Task Subscribe(Action<Event> eventHandler);
    }

    public interface IEventStore
    {
        Task<IEnumerable<Event>> LoadEvents(UserProfileId userProfileId);
        Task Persist(Event @event);
    }

    public class EventStoreStub : IEventStore
    {
        private readonly IEnumerable<Event> playedEvents;

        public EventStoreStub(IEnumerable<Event> playedEvents = null)
        {
            this.playedEvents = playedEvents ?? Array.Empty<Event>();
        }

        public Task<IEnumerable<Event>> LoadEvents(UserProfileId userProfileId)
         => Task.FromResult(playedEvents.Where(x => x.UserId.Equals(userProfileId)));

        public Task Persist(Event @event)
            => Task.CompletedTask;
    }
    public class EventBusStub : IEventBus
    {
        private readonly IEventStore store;
        private List<Action<Event>> handlers = new List<Action<Event>>();

        public EventBusStub(IEventStore store)
        {
            this.store = store;
        }

        public async Task Publish<TEvent>(TEvent @event) where TEvent : Event
        {
            await store.Persist(@event);
            foreach (var eventHandler in handlers)
                eventHandler(@event);
        }

        public Task Subscribe(Action<Event> eventHandler)
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
