using System.Collections.Generic;

namespace DDDTraining.Tests
{
    public class OptionAvailableEvent : Event
    {

        public IEnumerable<Option> Options { get; }
        public OptionAvailableEvent(UserProfileId userId, IEnumerable<Option> options)
            : base(userId)
        {
            Options = options;
        }
    }
}
