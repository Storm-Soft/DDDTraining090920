﻿using System;
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
        private readonly IEventStore store;
        private readonly Model model1 = new Model("1");

        public Config(IEventStore store)
        {
            this.store = store;
        }

        public async Task<Model> SelectModel1()
        {
            await store.Publish(new ModelSelectedEvent(model1));
            await store.Publish(new OptionAvailableEvent(new[] { new Option("A"), new Option("B") }));
            await store.Publish(new OptionSelectedEvent(model1, new Option("A")));
            return model1;
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
