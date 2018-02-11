namespace SpiceNetlist.SpiceObjects
{
    public class Component : Statement
    {
        public string Name { get; set; }

        public ParameterCollection Parameters { get; set; }
    }
}
