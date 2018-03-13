using System;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations
{
    /// <summary>
    /// Processes .NOISE <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class NoiseControl : SimulationControl
    {
        public override string TypeName => "noise";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, ProcessingContextBase context)
        {
            Noise noise = null;

            // Check parameter count
            switch (statement.Parameters.Count)
            {
                case 0: throw new Exception("Output expected for .NOISE");
                case 1: throw new Exception("Source expected");
                case 2: throw new Exception("Step type expected");
                case 3: throw new Exception("Number of points expected");
                case 4: throw new Exception("Starting frequency expected");
                case 5: throw new Exception("Stopping frequency expected");
                case 6: break;
                case 7: break;
                default:
                    throw new Exception("Too many parameter");
            }

            string type = statement.Parameters.GetString(2);
            var numberSteps = context.ParseDouble(statement.Parameters.GetString(3));
            var start = context.ParseDouble(statement.Parameters.GetString(4));
            var stop = context.ParseDouble(statement.Parameters.GetString(5));

            Sweep<double> sweep;

            switch (type)
            {
                case "lin": sweep = new SpiceSharp.Simulations.LinearSweep(start, stop, (int)numberSteps); break;
                case "oct": sweep = new SpiceSharp.Simulations.OctaveSweep(start, stop, (int)numberSteps); break;
                case "dec": sweep = new SpiceSharp.Simulations.DecadeSweep(start, stop, (int)numberSteps); break;
                default:
                    throw new Exception("LIN, DEC or OCT expected");
            }

            // The first parameters needs to specify the output voltage
            if (statement.Parameters[0] is BracketParameter bracket)
            {
                if (bracket.Name.ToLower() == "v")
                {
                    switch (bracket.Parameters.Count)
                    {
                        // V(A, B) - V(vector)
                        // V(A) - V(singleParameter
                        case 1:
                            if (bracket.Parameters[0] is VectorParameter v && v.Elements.Count == 2)
                            {
                                var output = new Identifier(v.Elements[0].Image);
                                var reference = new Identifier(v.Elements[1].Image);
                                var input = new Identifier(statement.Parameters[2].Image);
                                noise = new Noise("Noise " + (context.Simulations.Count() + 1), output, reference, input, sweep);
                            }
                            else if (bracket.Parameters[0] is SingleParameter s)
                            {
                                var output = new Identifier(s.Image);
                                var input = new Identifier(statement.Parameters[1].Image);
                                noise = new Noise("Noise " + (context.Simulations.Count() + 1), output, input, sweep);
                            }

                            break;
                        default:
                            throw new Exception("1 or 2 nodes expected");
                    }
                }
                else
                {
                    throw new Exception("Invalid output");
                }
            }
            else
            {
                throw new Exception("Invalid output");
            }

            context.Adder.AddSimulation(noise);
        }
    }
}
