using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Reads .DC <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class DCControl : SimulationControl
    {
        public DCControl(IMapper<Exporter> mapper)
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
            CreateSimulations(statement, context, CreateDCSimulation);
        }

        private DC CreateDCSimulation(string name, Control statement, ICircuitContext context)
        {
            int count = statement.Parameters.Count / 4;
            switch (statement.Parameters.Count - (4 * count))
            {
                case 0:
                    if (statement.Parameters.Count == 0)
                    {
                        throw new WrongParametersCountException(".dc - Source Name expected");
                    }

                    break;

                case 1: throw new WrongParametersCountException(".dc - Start value expected");
                case 2: throw new WrongParametersCountException(".dc - Stop value expected");
                case 3: throw new WrongParametersCountException(".dc - Step value expected");
            }

            // Format: .DC SRCNAM VSTART VSTOP VINCR [SRC2 START2 STOP2 INCR2]
            List<SweepConfiguration> sweeps = new List<SweepConfiguration>();

            for (int i = 0; i < count; i++)
            {
                SweepConfiguration sweep = new SweepConfiguration(
                    statement.Parameters.Get(4 * i).Image,
                    context.Evaluator.EvaluateDouble(statement.Parameters.Get((4 * i) + 1)),
                    context.Evaluator.EvaluateDouble(statement.Parameters.Get((4 * i) + 2)),
                    context.Evaluator.EvaluateDouble(statement.Parameters.Get((4 * i) + 3)));

                sweeps.Add(sweep);
            }

            DC dc = new DC(name, sweeps);
            dc.OnParameterSearch += (sender, e) =>
            {
                string sweepParameterName = e.Name;
                if (context.Evaluator.HaveParameter(dc, sweepParameterName))
                {
                    e.TemperatureNeeded = true;
                    e.Result = new EvaluationParameter(context.Evaluator.GetEvaluationContext(dc), sweepParameterName);
                }
            };

            ConfigureCommonSettings(dc, context);
            ConfigureDcSettings(dc.Configurations.Get<DCConfiguration>(), context);

            context.Result.AddSimulation(dc);

            return dc;
        }

        private void ConfigureDcSettings(DCConfiguration dCConfiguration, ICircuitContext context)
        {
            if (context.Result.SimulationConfiguration.SweepMaxIterations.HasValue)
            {
                dCConfiguration.SweepMaxIterations = context.Result.SimulationConfiguration.SweepMaxIterations.Value;
            }
        }
    }
}