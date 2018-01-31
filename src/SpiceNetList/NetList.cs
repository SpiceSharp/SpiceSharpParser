using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist
{
    public class NetList : SpiceObject
    {
        public string Title { get; set; }
        public Statements Statements { get; set; }
    }
}
