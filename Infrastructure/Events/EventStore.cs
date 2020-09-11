using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDDTraining.Tests
{
    public class EventStore : IEventStore
    {
        private readonly IEnumerable<Event> playedEvents;

        public EventStore()
        {
            this.playedEvents = playedEvents ?? Array.Empty<Event>();
        }

        public Task<IEnumerable<Event>> LoadEvents(UserProfileId userProfileId)
         => Task.FromResult(playedEvents.Where(x => x.UserId.Equals(userProfileId)));

        public Task Persist(IEnumerable<Event> events)
            => Task.CompletedTask;
    }
}
