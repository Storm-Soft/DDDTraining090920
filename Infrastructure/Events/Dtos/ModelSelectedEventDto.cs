namespace DDDTraining.Tests
{
    public struct ModelSelectedEventDto 
    {
        public UserProfileIdDto UserId { get; set; }
        public ModelDto Model { get; set; }

        public ModelSelectedEvent ToDomain()
            => new ModelSelectedEvent(UserId.ToDomain(), Model.ToDomain());
    }
}
