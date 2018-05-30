using System.Collections.Generic;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers
{
    //TODO: refactor this even more
    public class StatementsOrderer : IStatementsOrderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatementsOrderer"/> class.
        /// </summary>
        /// <param name="controlRegistry">control registry</param>
        public StatementsOrderer(IControlRegistry controlRegistry)
        {
            ControlRegistry = controlRegistry;
        }

        protected IControlRegistry ControlRegistry { get; }

        /// <summary>
        /// Orders statements for processing.
        /// </summary>
        /// <param name="statements">Statement to order.</param>
        /// <returns>
        /// Ordered statements.
        /// </returns>
        public IEnumerable<Statement> Order(Statements statements)
        {
            return statements.OrderBy(GetOrder);
        }

        protected virtual int GetOrder(Statement statement)
        {
            if (statement is Model.Netlist.Spice.Objects.Model)
            {
                return 200;
            }

            if (statement is Component)
            {
                return 300;
            }

            if (statement is SubCircuit)
            {
                return 100;
            }

            if (statement is Control c)
            {
                if (c.Name.ToLower() == "plot" || c.Name.ToLower() == "save")
                {
                    return 400;
                }

                return ControlRegistry.IndexOf(c.Name.ToLower());
            }

            if (statement is CommentLine)
            {
                return 0;
            }

            return -1;
        }
    }
}
