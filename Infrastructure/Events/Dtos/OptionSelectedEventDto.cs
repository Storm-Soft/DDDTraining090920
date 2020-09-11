namespace DDDTraining.Tests
{
    public struct OptionSelectedEventDto
    {
        public UserProfileIdDto UserId { get; set; }
        public ModelDto Model { get; set; }
        public OptionDto Option { get; set; }

        public OptionSelectedEvent ToDomain()
            => new OptionSelectedEvent(UserId.ToDomain(), Model.ToDomain(), Option.ToDomain());

    }
}
