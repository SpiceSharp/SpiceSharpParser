using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public class SpiceStatementsOrderer : ISpiceStatementsOrderer
    {
        protected List<string> TopControls { get; set; } = new List<string> { "st_r", "step_r", "param", "sparam", "func", "options", "distribution" };

        protected List<string> ControlsAfterComponents { get; set; } = new List<string> { "plot", "print", "save", "wave" };

        protected List<string> Controls { get; set; } = new List<string> { "temp", "step", "st", "nodeset", "ic", "mc", "op", "ac", "tran", "dc", "noise" };

        /// <summary>
        /// Orders statements for reading.
        /// </summary>
        /// <param name="statements">Statement to order.</param>
        /// <returns>
        /// Ordered statements.
        /// </returns>
        public IEnumerable<Statement> Order(Statements statements)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            return statements.OrderBy(GetOrder);
        }

        protected virtual int GetOrder(Statement statement)
        {
            if (statement is null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (statement is SubCircuit)
            {
                return 1000;
            }

            if (statement is Model)
            {
                return 2000;
            }

            if (statement is Component)
            {
                return 3000;
            }

            if (statement is Control c)
            {
                var name = c.Name.ToLower();

                if (ControlsAfterComponents.Contains(name))
                {
                    return 4000;
                }

                if (TopControls.Contains(name))
                {
                    return TopControls.IndexOf(name);
                }

                return 3500 + Controls.IndexOf(name);
            }

            return int.MaxValue;
        }
    }
}