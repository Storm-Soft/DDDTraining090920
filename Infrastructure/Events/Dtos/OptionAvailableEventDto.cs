using System.Collections.Generic;
using System.Linq;

namespace DDDTraining.Tests
{
    public struct OptionAvailableEventDto
    {
        public UserProfileIdDto UserId { get; set; }
        public IEnumerable<OptionDto> Options { get; set; }

        public OptionAvailableEvent ToDomain()
          => new OptionAvailableEvent(UserId.ToDomain(), 
                                       Options.Select(option=>option.ToDomain()));
    }
}
