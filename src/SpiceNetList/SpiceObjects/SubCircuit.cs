namespace SpiceNetlist.SpiceObjects
{
    public class SubCircuit : Statement
    {
        public string Name { get; set; }

        public ParameterCollection Parameters { get; set; }

        public Statements Statements { get; set; }
    }
}
