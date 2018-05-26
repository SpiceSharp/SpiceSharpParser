﻿using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReader.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls.Simulations
{
    /// <summary>
    /// Processes .DC <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class DCControl : SimulationControl
    {
        /// <summary>
        /// Gets the Spice type
        /// </summary>
        public override string TypeName => "dc";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, IProcessingContext context)
        {
            CreateSimulations(statement, context, CreateDCSimulation);
        }

        private DC CreateDCSimulation(string name, Control statement, IProcessingContext context)
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
                    statement.Parameters.GetString(4 * i),
                    context.ParseDouble(statement.Parameters.GetString((4 * i) + 1)),
                    context.ParseDouble(statement.Parameters.GetString((4 * i) + 2)),
                    context.ParseDouble(statement.Parameters.GetString((4 * i) + 3)));

                sweeps.Add(sweep);
            }

            DC dc = new DC(name, sweeps);
            dc.OnParameterSearch += (sender, e) =>
            {
                string sweepParameterName = e.Name.ToString();
                if (context.Evaluator.HasParameter(sweepParameterName))
                {
                    e.TemperatureNeeded = true;
                    e.Result = new EvaluationParameter(context.Evaluator, sweepParameterName);
                }
            };

            SetBaseConfiguration(dc.BaseConfiguration, context);
            SetDcParameters(dc.DcConfiguration, context);

            context.Result.AddSimulation(dc);

            return dc;
        }

        private void SetDcParameters(DcConfiguration dCConfiguration, IProcessingContext context)
        {
            if (context.Result.SimulationConfiguration.SweepMaxIterations.HasValue)
            {
                dCConfiguration.SweepMaxIterations = context.Result.SimulationConfiguration.SweepMaxIterations.Value;
            }
        }
    }
}