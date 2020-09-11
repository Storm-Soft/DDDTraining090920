namespace DDDTraining.Tests
{
    public struct ModelDto
    {
        public string Id { get;  set; }
        public Model ToDomain()
            => new Model(Id);
    }
}
