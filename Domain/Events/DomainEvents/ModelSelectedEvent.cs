namespace DDDTraining.Tests
{
    public struct ModelSelectedEvent : IEvent
    {
        public UserProfileId UserId { get; private set; }
        public Model Model { get; private set; }
       
        public ModelSelectedEvent(UserProfileId userId, Model model)
        {
            UserId = userId;
            Model = model;
        }

    }
}
