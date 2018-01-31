using System.Collections.Generic;

namespace SpiceNetlist.SpiceObjects
{
    public class Statements : SpiceObject
    {
        public List<Statement> List { get; set; }

        public Statements()
        {
            List = new List<Statement>();
        }
    }
}
