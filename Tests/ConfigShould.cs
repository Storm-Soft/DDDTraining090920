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
}
