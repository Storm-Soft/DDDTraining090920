using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDDTraining.Tests
{
    public class EventStoreStub : IEventStore
    {
        private readonly IEnumerable<IEvent> playedEvents;

        public EventStoreStub(IEnumerable<IEvent> playedEvents = null)
        {
            this.playedEvents = playedEvents ?? Array.Empty<IEvent>();
        }

        public Task<IEnumerable<IEvent>> LoadEvents(UserProfileId userProfileId)
         => Task.FromResult(playedEvents.Where(x => x.UserId.Equals(userProfileId)));

        public Task Persist(IEnumerable<IEvent> events)
            => Task.CompletedTask;
    }
}
