using System.Collections.Generic;

namespace DDDTraining.Tests
{
    public class ConfigListProjection
    {
        private readonly Dictionary<UserProfileId,Model> selectedModels = new Dictionary<UserProfileId, Model>();

        public ConfigListProjection(IEventBus eventBus)
        {
            eventBus.Subscribe(Apply);
        }

        private void Apply(IEvent @event)
        {
            switch(@event)
            {
                case ModelSelectedEvent modelSelectedEvent:
                    selectedModels[modelSelectedEvent.UserId] = modelSelectedEvent.Model;
                    break;
            }
        }

        public IDictionary<UserProfileId, Model> GetModelsByUserProfiles() => selectedModels;
    }
}
