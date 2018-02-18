using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ControlProcessor : StatementProcessor<Control>
    {
        public ControlProcessor(ControlRegistry registry)
        {
            Registry = registry;
        }

        public ControlRegistry Registry { get; }

        public override void Process(Control statement, ProcessingContext context)
        {
            string type = statement.Name.ToLower();

            if (!Registry.Supports(type))
            {
                throw new System.Exception("Unsupported control");
            }

            Registry.Get(type).Process(statement, context);
        }

        internal int GetSubOrder(Control statement)
        {
            string type = statement.Name.ToLower();
            return Registry.IndexOf(type);
        }
    }
}
