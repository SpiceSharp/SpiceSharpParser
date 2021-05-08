using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Reads .AC <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class ACControl : SimulationControl
    {
        public ACControl(IMapper<Exporter> mapper)
            : base(mapper)
        {
        }

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            CreateSimulations(statement, context, CreateAcSimulation);
        }

        private AC CreateAcSimulation(string name, Control statement, IReadingContext context)
        {
            switch (statement.Parameters.Count)
            {
                case 0:
                    context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "LIN, DEC or OCT expected", statement.LineInfo));
                    return null;
                case 1:
                    context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Number of points expected", statement.LineInfo));
                    return null;

                case 2:
                    context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Starting frequency expected", statement.LineInfo));
                    return null;

                case 3:
                    context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Stopping frequency expected", statement.LineInfo));
                    return null;
            }

            AC ac;

            string type = statement.Parameters.Get(0).Value.ToLower();
            var numberSteps = context.Evaluator.EvaluateDouble(statement.Parameters.Get(1));
            var start = context.Evaluator.EvaluateDouble(statement.Parameters.Get(2));
            var stop = context.Evaluator.EvaluateDouble(statement.Parameters.Get(3));

            switch (type)
            {
                case "lin": ac = new AC(name, new LinearSweep(start, stop, (int)numberSteps)); break;
                case "oct": ac = new AC(name, new OctaveSweep(start, stop, (int)numberSteps)); break;
                case "dec": ac = new AC(name, new DecadeSweep(start, stop, (int)numberSteps)); break;
                default:
                    context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "LIN, DEC or OCT expected", statement.LineInfo));
                    return null;
            }

            ConfigureCommonSettings(ac, context);
            ConfigureAcSettings(ac.FrequencyParameters, context);

            /*ac.BeforeFrequencyLoad += (sender, args) =>
                {
                    if (ac.ComplexState != null)
                    {
                        var freq = ac.ComplexState.Laplace.Imaginary / (2.0 * Math.PI);
                        context.Evaluator.SetParameter(ac, "FREQ", freq);
                    }
                };*/

            // TODO
            context.Result.Simulations.Add(ac);
            return ac;
        }

        private void ConfigureAcSettings(FrequencyParameters frequencyConfiguration, IReadingContext context)
        {
            if (context.SimulationConfiguration.KeepOpInfo.HasValue)
            {
                frequencyConfiguration.KeepOpInfo = context.SimulationConfiguration.KeepOpInfo.Value;
            }
        }
    }
}