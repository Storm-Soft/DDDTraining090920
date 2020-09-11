using System.Collections.Generic;

namespace DDDTraining.Tests
{
    public struct OptionAvailableEvent : IEvent
    {
        public UserProfileId UserId { get; }
        public IEnumerable<Option> Options { get; }
        public OptionAvailableEvent(UserProfileId userId, IEnumerable<Option> options)
        {
            UserId = userId;
            Options = options;
        }
    }
}
