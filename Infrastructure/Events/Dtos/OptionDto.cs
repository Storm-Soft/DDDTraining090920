namespace DDDTraining.Tests
{
    public struct OptionDto
    {
        public string Id { get; set; }

        public Option ToDomain()
            => new Option(Id);
    }
}
