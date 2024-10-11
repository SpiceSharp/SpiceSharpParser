﻿using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Reads .NOISE <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class NoiseControl : SimulationControl
    {
        private IEnumerable<double> _sweep;

        public NoiseControl(IMapper<Exporter> mapper)
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
            CreateSimulations(statement, context, CreateNoiseSimulation);
        }

        private ISimulationWithEvents CreateNoiseSimulation(string name, Control statement, IReadingContext context)
        {
            NoiseWithEvents noise = null;

            // Check parameter count
            switch (statement.Parameters.Count)
            {
                case 0:
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Parameters expected for .NOISE", statement.LineInfo);
                    return null;
                case 1:
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Source expected", statement.LineInfo);
                    return null;

                case 2:
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Step type expected", statement.LineInfo);
                    return null;
                case 3:
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Number of points expected", statement.LineInfo);
                    return null;
                case 4:
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Starting frequency expected", statement.LineInfo);
                    return null;
                case 5:
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Stopping frequency expected", statement.LineInfo);
                    return null;
                case 6: break;
                case 7: break;
                default:
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Too many parameters for .NOISE", statement.LineInfo);
                    return null;
            }

            string type = statement.Parameters.Get(2).Value.ToLower();
            var numberSteps = context.Evaluator.EvaluateDouble(statement.Parameters.Get(3));
            var start = context.Evaluator.EvaluateDouble(statement.Parameters.Get(4));
            var stop = context.Evaluator.EvaluateDouble(statement.Parameters.Get(5));

            switch (type)
            {
                case "lin": _sweep = new LinearSweep(start, stop, (int)numberSteps); break;
                case "oct": _sweep = new OctaveSweep(start, stop, (int)numberSteps); break;
                case "dec": _sweep = new DecadeSweep(start, stop, (int)numberSteps); break;
                default:
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "LIN, DEC or OCT expected", statement.LineInfo);
                    return null;
            }

            // The first parameters needs to specify the output voltage
            if (statement.Parameters[0] is BracketParameter bracket)
            {
                if (bracket.Name.ToLower() == "v")
                {
                    switch (bracket.Parameters.Count)
                    {
                        // V(A, B) - V(vector)
                        // V(A) - V(singleParameter)
                        case 1:
                            if (bracket.Parameters[0] is VectorParameter v && v.Elements.Count == 2)
                            {
                                var output = v.Elements[0].Value;
                                var reference = v.Elements[1].Value;
                                var input = statement.Parameters[1].Value;
                                noise = new NoiseWithEvents(name, input, output, reference, _sweep);
                            }
                            else if (bracket.Parameters[0] is SingleParameter s)
                            {
                                var output = s.Value;
                                var input = statement.Parameters[1].Value;
                                noise = new NoiseWithEvents(name, input, output, _sweep);
                            }

                            break;

                        default:
                            context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "1 or 2 nodes expected", statement.LineInfo);
                            return null;
                    }
                }
                else
                {
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader,  "Invalid output", statement.LineInfo);
                    return null;
                }
            }
            else
            {
                context.Result.ValidationResult.AddError(ValidationEntrySource.Reader,  "Invalid output", statement.LineInfo);
                return null;
            }

            context.Result.Simulations.Add(noise);

            return noise;
        }
    }
}