namespace DDDTraining.Tests
{
    public struct OptionSelectedEvent : IEvent
    {
        public UserProfileId UserId { get; }
        public Model Model { get; }
        public Option Option { get; }

        public OptionSelectedEvent(UserProfileId userId, Model model, Option option)
        {
            UserId = userId;
            Model = model;
            Option = option;
        }

    }
}
