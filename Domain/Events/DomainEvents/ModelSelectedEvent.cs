namespace DDDTraining.Tests
{
    public struct ModelSelectedEvent : IEvent
    {
        public UserProfileId UserId { get;  }
        public Model Model { get;  }
       
        public ModelSelectedEvent(UserProfileId userId, Model model)
        {
            UserId = userId;
            Model = model;
        }

    }
}
