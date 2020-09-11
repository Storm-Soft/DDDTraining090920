namespace DDDTraining.Tests
{
    public class ModelSelectedEvent : Event
    {
        public Model Model { get; }

        public ModelSelectedEvent(UserProfileId userId, Model model)
            : base(userId)
        {
            Model = model;
        }

    }
}
