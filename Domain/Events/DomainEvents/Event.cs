namespace DDDTraining.Tests
{
    public abstract class Event
    {
        public UserProfileId UserId { get; }

        protected Event(UserProfileId userId)
        {
            UserId = userId;
        }
    }
}
