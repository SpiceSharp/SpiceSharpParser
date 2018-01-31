namespace SpiceNetlist.SpiceObjects
{
    public class Control : Statement
    {
        public string Name { get; set; }
        public ParameterCollection Parameters { get; set; }
    }
}
