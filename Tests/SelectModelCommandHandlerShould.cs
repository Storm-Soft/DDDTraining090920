using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DDDTraining.Tests
{
    public class SelectModelCommandHandlerShould
    {
        private static readonly UserProfileId UserProfileId1 = new UserProfileId(Guid.NewGuid());
        private static readonly Model model1 = new Model("1");

        [Fact]
        public async Task When_Publish_Event_On_Aggregate_Then_Projection_Is_Updated()
        {
            var eventStore = new EventStore();
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
}
