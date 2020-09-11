namespace DDDTraining.Tests
{
    public class OptionSelectedEvent : Event
    {
        public Model Model { get; }
        public Option Option { get; }

        public OptionSelectedEvent(UserProfileId userId, Model model, Option option)
            : base(userId)
        {
            Model = model;
            Option = option;
        }

    }
}
