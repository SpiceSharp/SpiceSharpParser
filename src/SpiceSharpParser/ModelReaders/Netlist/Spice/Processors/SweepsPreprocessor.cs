using System.Linq;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Processors
{
    public class SweepsPreprocessor : IProcessor
    {
        /// <summary>
        /// Preprocess .ST and .STEP.
        /// </summary>
        /// <param name="statements">Statements</param>
        public Statements Process(Statements statements)
        {
            foreach (var statement in statements.ToArray())
            {
                if (statement is Control c)
                {
                    if (c.Name.ToLower() == "st")
                    {
                        var cloned = (Control)c.Clone();
                        cloned.Name = "st_r";
                        statements.Add(cloned);
                    }

                    if (c.Name.ToLower() == "step")
                    {
                        var cloned = (Control)c.Clone();
                        cloned.Name = "step_r";
                        statements.Add(cloned);
                    }
                }
            }

            return statements;
        }
    }
}
