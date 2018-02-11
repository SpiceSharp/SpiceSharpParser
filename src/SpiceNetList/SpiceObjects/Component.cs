namespace SpiceNetlist.SpiceObjects
{
    public class Component : Statement
    {
        public string Name { get; set; }

        public ParameterCollection PinsAndParameters { get; set; }
    }
}
