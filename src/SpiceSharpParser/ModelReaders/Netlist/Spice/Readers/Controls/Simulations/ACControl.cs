using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;

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
        public override void Read(Control statement, ICircuitContext context)
        {
            CreateSimulations(statement, context, CreateAcSimulation);
        }

        private AC CreateAcSimulation(string name, Control statement, ICircuitContext context)
        {
            switch (statement.Parameters.Count)
            {
                case 0: throw new WrongParametersCountException("LIN, DEC or OCT expected", statement.LineInfo);
                case 1: throw new WrongParametersCountException("Number of points expected", statement.LineInfo);
                case 2: throw new WrongParametersCountException("Starting frequency expected", statement.LineInfo);
                case 3: throw new WrongParametersCountException("Stopping frequency expected", statement.LineInfo);
            }

            AC ac;

            string type = statement.Parameters.Get(0).Image.ToLower();
            var numberSteps = context.Evaluator.EvaluateDouble(statement.Parameters.Get(1));
            var start = context.Evaluator.EvaluateDouble(statement.Parameters.Get(2));
            var stop = context.Evaluator.EvaluateDouble(statement.Parameters.Get(3));

            switch (type)
            {
                case "lin": ac = new AC(name, new LinearSweep(start, stop, (int)numberSteps)); break;
                case "oct": ac = new AC(name, new OctaveSweep(start, stop, (int)numberSteps)); break;
                case "dec": ac = new AC(name, new DecadeSweep(start, stop, (int)numberSteps)); break;
                default:
                    throw new WrongParameterException("LIN, DEC or OCT expected", statement.Parameters.LineInfo);
            }

            ConfigureCommonSettings(ac, context);
            ConfigureAcSettings(ac.Configurations.Get<FrequencyConfiguration>(), context);

            ac.BeforeFrequencyLoad += (sender, args) =>
                {
                    if (ac.ComplexState != null)
                    {
                        var freq = ac.ComplexState.Laplace.Imaginary / (2.0 * Math.PI);
                        context.Evaluator.SetParameter(ac, "FREQ", freq);
                    }
                };

            context.Result.AddSimulation(ac);
            return ac;
        }

        private void ConfigureAcSettings(FrequencyConfiguration frequencyConfiguration, ICircuitContext context)
        {
            if (context.Result.SimulationConfiguration.KeepOpInfo.HasValue)
            {
                frequencyConfiguration.KeepOpInfo = context.Result.SimulationConfiguration.KeepOpInfo.Value;
            }
        }
    }
}