using System.Threading.Tasks;

namespace DDDTraining.Tests
{
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
                await eventBus.Publish(events);
        }        
    }
}
