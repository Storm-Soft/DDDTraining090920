﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDDTraining.Tests
{
    public interface IEventStore
    {
        Task<IEnumerable<IEvent>> LoadEvents(UserProfileId userProfileId);
        Task Persist(IEnumerable<IEvent> events);
    }
}
