using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Preprocessors
{
    public class SweepsPreprocessor : ISweepsPreprocessor
    {
        /// <summary>
        /// Preprocess .ST and .STEP.
        /// </summary>
        /// <param name="netlistModel">Netlist model to seach for .ST and .STEP statements</param>
        public void Preprocess(SpiceNetlist netlistModel)
        {
            foreach (var statement in netlistModel.Statements.ToArray())
            {
                if (statement is Control c)
                {
                    if (c.Name.ToLower() == "st")
                    {
                        var cloned = (Control)c.Clone();
                        cloned.Name = "st_r";
                        netlistModel.Statements.Add(cloned);
                    }

                    if (c.Name.ToLower() == "step")
                    {
                        var cloned = (Control)c.Clone();
                        cloned.Name = "step_r";
                        netlistModel.Statements.Add(cloned);
                    }
                }
            }
        }
    }
}
