namespace DDDTraining.Tests
{
    public struct Model
    {
        public string Id { get; private set; }
        
        public Model(string id)
        {
            Id = id;
        }
    }
}
