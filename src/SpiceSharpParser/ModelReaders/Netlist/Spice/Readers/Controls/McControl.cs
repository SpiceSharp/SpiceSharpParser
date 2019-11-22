using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .MC <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class McControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, ICircuitContext context)
        {
            if (statement.Parameters == null)
            {
                throw new System.ArgumentNullException(nameof(statement.Parameters));
            }

            context.Result.SimulationConfiguration.MonteCarloConfiguration.Enabled = true;
            context.Result.SimulationConfiguration.MonteCarloConfiguration.Runs = int.Parse(statement.Parameters.Get(0).Image);
            context.Result.SimulationConfiguration.MonteCarloConfiguration.SimulationType = statement.Parameters.Get(1).Image.ToLower();
            context.Result.SimulationConfiguration.MonteCarloConfiguration.OutputVariable = statement.Parameters[2];
            context.Result.SimulationConfiguration.MonteCarloConfiguration.Function = statement.Parameters.Get(3).Image;

            if (statement.Parameters.Count > 4 && statement.Parameters[4] is Models.Netlist.Spice.Objects.Parameters.AssignmentParameter a)
            {
                if (a.Name.ToLower() == "seed")
                {
                    int seed = int.Parse(a.Value);
                    context.Result.SimulationConfiguration.MonteCarloConfiguration.Seed = seed;
                }
            }
        }
    }
}