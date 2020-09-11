using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDDTraining.Tests
{
    public interface IEventStore
    {
        Task<IEnumerable<Event>> LoadEvents(UserProfileId userProfileId);
        Task Persist(Event @event);
    }
}
