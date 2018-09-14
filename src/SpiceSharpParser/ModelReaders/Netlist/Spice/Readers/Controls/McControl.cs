using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .MC <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class McControl : BaseControl
    {
        public override string SpiceCommandName => "mc";

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            if (statement.Parameters == null)
            {
                throw new System.ArgumentNullException(nameof(statement.Parameters));
            }

            context.Result.SimulationConfiguration.MonteCarloConfiguration.Enabled = true;
            context.Result.SimulationConfiguration.MonteCarloConfiguration.Runs = int.Parse(statement.Parameters.GetString(0));
            context.Result.SimulationConfiguration.MonteCarloConfiguration.SimulationType = statement.Parameters.GetString(1).ToLower();
            context.Result.SimulationConfiguration.MonteCarloConfiguration.OutputVariable = statement.Parameters[2].Image;
            context.Result.SimulationConfiguration.MonteCarloConfiguration.Function = statement.Parameters.GetString(3);

            if (statement.Parameters.Count > 4 && statement.Parameters[4] is Models.Netlist.Spice.Objects.Parameters.AssignmentParameter a)
            {
                if (a.Name.ToLower() == "seed")
                {
                    // TODO: refactor it please
                    context.Result.SimulationConfiguration.Seed = int.Parse(a.Value);
                    context.Result.Seed = context.Result.SimulationConfiguration.Seed.Value;
                    context.Result.SimulationConfiguration.MonteCarloConfiguration.RandomSeed = context.Result.SimulationConfiguration.Seed;
                }
            }
        }
    }
}
