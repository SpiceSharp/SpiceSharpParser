namespace SpiceNetlist.SpiceObjects
{
    public class Model : Statement
    {
        public string Name { get; set; }

        public ParameterCollection Parameters { get; set; }
    }
}
