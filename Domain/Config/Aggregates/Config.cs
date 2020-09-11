using System;
using System.Collections.Generic;
using System.Linq;

namespace DDDTraining.Tests
{
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
}
