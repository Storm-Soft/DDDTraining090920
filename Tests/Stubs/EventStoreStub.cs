using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDDTraining.Tests
{
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
}
