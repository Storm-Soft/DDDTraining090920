using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DDDTraining.Tests
{
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
            await eventBus.Publish(new[] { new ModelSelectedEvent(UserProfileId1, model1) });            

            var foundModel = projection.GetModelsByUserProfiles().FirstOrDefault(model => model.Key.Equals(UserProfileId1)).Value;

            Assert.Equal(model1, foundModel);
        }
    }
}
