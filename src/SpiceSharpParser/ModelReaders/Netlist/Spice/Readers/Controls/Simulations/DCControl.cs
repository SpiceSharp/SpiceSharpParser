﻿using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

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
        public override void Read(Control statement, IReadingContext context)
        {
            CreateSimulations(statement, context, CreateDCSimulation);
        }

        private ISimulationWithEvents CreateDCSimulation(string name, Control statement, IReadingContext context)
        {
            int count = statement.Parameters.Count / 4;
            switch (statement.Parameters.Count - (4 * count))
            {
                case 0:
                    if (statement.Parameters.Count == 0)
                    {
                        context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, ".dc - Source Name expected", statement.LineInfo);
                        return null;
                    }

                    break;

                case 1:
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, ".dc - Start value expected", statement.LineInfo);
                    return null;

                case 2:
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, ".dc - Stop value expected", statement.LineInfo);
                    return null;

                case 3:
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, ".dc - Step value expected", statement.LineInfo);
                    return null;
            }

            // Format: .DC SRCNAM VSTART VSTOP VINCR [SRC2 START2 STOP2 INCR2]
            List<ISweep> sweeps = new List<ISweep>();

            for (int i = 0; i < count; i++)
            {
                var start = context.Evaluator.EvaluateDouble(statement.Parameters.Get((4 * i) + 1));
                var stop = context.Evaluator.EvaluateDouble(statement.Parameters.Get((4 * i) + 2));
                var step = context.Evaluator.EvaluateDouble(statement.Parameters.Get((4 * i) + 3));
                ParameterSweep sweep = new ParameterSweep(statement.Parameters.Get(4 * i).Value, Enumerable.Range(0, (int)((stop - start) / step) + 1).Select(index => start + (index * step)));

                sweeps.Add(sweep);
            }

            DcWithEvents dc = new DcWithEvents(name, sweeps);

            // TODO: Consult with Sven
            /*dc.OnParameterSearch += (sender, e) =>
            {
                string sweepParameterName = e.Name;
                if (context.Evaluator.HaveParameter(dc, sweepParameterName))
                {
                    e.TemperatureNeeded = true;
                    e.Result = new EvaluationParameter(context.Evaluator.GetEvaluationContext(dc), sweepParameterName);
                }
            };*/

            ConfigureCommonSettings(dc, context);
            ConfigureDcSettings(dc.DCParameters, context);

            context.Result.Simulations.Add(dc);

            return dc;
        }

        private void ConfigureDcSettings(DCParameters dCConfiguration, IReadingContext context)
        {
            if (context.SimulationConfiguration.SweepMaxIterations.HasValue)
            {
                dCConfiguration.SweepMaxIterations = context.SimulationConfiguration.SweepMaxIterations.Value;
            }
        }
    }
}