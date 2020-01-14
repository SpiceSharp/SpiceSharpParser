using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Processors
{
    public class SweepsPreprocessor : IProcessor
    {
        public SpiceParserValidationResult Validation { get; set; }

        /// <summary>
        /// Preprocess .ST and .STEP.
        /// </summary>
        /// <param name="statements">Statements</param>
        public Statements Process(Statements statements)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            foreach (var statement in statements.ToArray())
            {
                if (statement is Control c)
                {
                    if (c.Name.ToLower() == "st")
                    {
                        var cloned = (Control)c.Clone();
                        cloned.Name = "ST_R";
                        statements.Add(cloned);
                    }

                    if (c.Name.ToLower() == "step")
                    {
                        var cloned = (Control)c.Clone();
                        cloned.Name = "STEP_R";
                        statements.Add(cloned);
                    }
                }
            }

            return statements;
        }
    }
}