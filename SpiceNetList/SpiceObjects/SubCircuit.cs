namespace SpiceNetlist.SpiceObjects
{
    public class SubCircuit : Statement
    {
        public string Name { get; set; }

        public Parameters Parameters { get; set; }
        public Statement Statements { get; set; }
    }
}
