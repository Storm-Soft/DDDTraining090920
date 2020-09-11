namespace DDDTraining.Tests
{
    public struct Option
    {
        public string Id { get; }
        public Option(string id)
        {
            Id = id;
        }

        public override string ToString() => $"Id{Id}";
    }
}
