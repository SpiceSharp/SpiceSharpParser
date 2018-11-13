using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    using SpiceSharpParser.Common.Evaluation;

    /// <summary>
    /// Current sources generator.
    /// </summary>
    public class CurrentSourceGenerator : ComponentGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSourceGenerator"/> class.
        /// </summary>
        public CurrentSourceGenerator()
        {
        }

        /// <summary>
        /// Gets generated types.
        /// </summary>
        /// <returns>
        /// Generated types.
        /// </returns>
        public override IEnumerable<string> GeneratedTypes => new List<string>() { "I", "G", "F" };

        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type.ToLower())
            {
                case "i": return GenerateCurrentSource(componentIdentifier, parameters, context);
                case "g": return GenerateVoltageControlledCurrentSource(componentIdentifier, parameters, context);
                case "f": return GenerateCurrentControlledCurrentSource(componentIdentifier, parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Generates a new current controlled current source: FName
        /// </summary>
        /// <param name="name">Name of generated current controlled current source</param>
        /// <param name="parameters">Parameters for current source</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of current controlled current source
        /// </returns>
        protected SpiceSharp.Components.Component GenerateCurrentControlledCurrentSource(string name,  ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value")
               && parameters.Count == 3)
            {
                var valueParameter = (AssignmentParameter)parameters.Single(
                    p => p is AssignmentParameter ap && ap.Name.ToLower() == "value");

                var cs = new CurrentSource(name);
                context.CreateNodes(cs, parameters);
                context.SetParameter(cs, "dc", valueParameter.Value);
                return cs;
            }

            if (parameters.Any(p => p is BracketParameter bp && bp.Name.ToLower() == "poly"))
            {
                var polyParameter = (BracketParameter)parameters.Single(
                    p => p is BracketParameter bp && bp.Name.ToLower() == "poly");

                if (polyParameter.Parameters.Count != 1)
                {
                    throw new WrongParametersCountException(name, "poly expects one argument => dimension");
                }

                var dimension = (int)context.EvaluateDouble(polyParameter.Parameters[0].Image);
                var expression = ExpressionGenerator.CreatePolyCurrentExpression(dimension, parameters.Skip(3));

                var cs = new CurrentSource(name);
                context.CreateNodes(cs, parameters);
                context.SetParameter(cs, "dc", expression);
                return cs;
            }

            if (parameters.Any(p => p is WordParameter bp && bp.Image.ToLower() == "poly"))
            {
                var dimension = 1;
                var expression = ExpressionGenerator.CreatePolyCurrentExpression(dimension, parameters.Skip(3));

                var cs = new CurrentSource(name);
                context.CreateNodes(cs, parameters);
                context.SetParameter(cs, "dc", expression);
                return cs;
            }

            if (parameters.Any(p => p is ExpressionEqualParameter) && parameters.Any(p => p.Image.ToLower() == "table"))
            {
                var formulaParameter = (ExpressionEqualParameter)parameters.Single(p => p is ExpressionEqualParameter);
                var cs = new CurrentSource(name);
                context.CreateNodes(cs, parameters);

                var tableParameter = name + "_table_variable";
                context.SetParameter(tableParameter, formulaParameter.Expression);

                string expression = ExpressionGenerator.CreateTableExpression(tableParameter, formulaParameter);
                context.SetParameter(cs, "dc", expression);
                return cs;
            }

            if (parameters.Count == 4)
            {
                var cccs = new CurrentControlledCurrentSource(name);
                context.CreateNodes(cccs, parameters);
                cccs.ControllingName = context.ComponentNameGenerator.Generate(parameters.GetString(2));
                context.SetParameter(cccs, "gain", parameters.GetString(3));

                return cccs;
            }

            throw new WrongParametersCountException(name, "invalid syntax for current controlled current source");
        }

        /// <summary>
        /// Generates a new voltage controlled current source: GName
        /// </summary>
        /// <param name="name">Name of generated voltage controlled current source</param>
        /// <param name="parameters">Parameters for current source</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of voltage controlled current source
        /// </returns>
        protected SpiceSharp.Components.Component GenerateVoltageControlledCurrentSource(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value")
               && parameters.Count == 3)
            {
                var valueParameter = (AssignmentParameter)parameters.Single(
                    p => p is AssignmentParameter ap && ap.Name.ToLower() == "value");

                var cs = new CurrentSource(name);
                context.CreateNodes(cs, parameters);
                context.SetParameter(cs, "dc", valueParameter.Value);
                return cs;
            }

            if (parameters.Any(p => p is BracketParameter bp && bp.Name.ToLower() == "poly"))
            {
                var polyParameter = (BracketParameter)parameters.Single(
                    p => p is BracketParameter bp && bp.Name.ToLower() == "poly");

                if (polyParameter.Parameters.Count != 1)
                {
                    throw new WrongParametersCountException(name, "poly expects one argument => dimension");
                }

                var dimension = (int)context.EvaluateDouble(polyParameter.Parameters[0].Image);
                var expression = ExpressionGenerator.CreatePolyVoltageExpression(dimension, parameters.Skip(3));

                var cs = new CurrentSource(name);
                context.CreateNodes(cs, parameters);
                context.SetParameter(cs, "dc", expression);
                return cs;
            }

            if (parameters.Any(p => p is WordParameter bp && bp.Image.ToLower() == "poly"))
            {
                var dimension = 1;
                var expression = ExpressionGenerator.CreatePolyVoltageExpression(dimension, parameters.Skip(3));

                var cs = new CurrentSource(name);
                context.CreateNodes(cs, parameters);
                context.SetParameter(cs, "dc", expression);
                return cs;
            }

            if (parameters.Any(p => p is ExpressionEqualParameter) && parameters.Any(p => p.Image.ToLower() == "table"))
            {
                var formulaParameter = (ExpressionEqualParameter)parameters.Single(p => p is ExpressionEqualParameter);
                var cs = new CurrentSource(name);
                context.CreateNodes(cs, parameters);

                var tableParameter = name + "_table_variable";
                context.SetParameter(tableParameter, formulaParameter.Expression);
                string expression = ExpressionGenerator.CreateTableExpression(tableParameter, formulaParameter);
                context.SetParameter(cs, "dc", expression);
                return cs;
            }

            if (parameters.Count == 3
                && parameters[0] is PointParameter pp1
                && pp1.Values.Count() == 2
                && parameters[1] is PointParameter pp2
                && pp2.Values.Count() == 2)
            {
                var vccs = new VoltageControlledCurrentSource(name);
                var vccsNodes = new ParameterCollection();
                vccsNodes.Add(pp1.Values.Items[0]);
                vccsNodes.Add(pp1.Values.Items[1]);
                vccsNodes.Add(pp2.Values.Items[0]);
                vccsNodes.Add(pp2.Values.Items[1]);
                context.CreateNodes(vccs, vccsNodes);
                context.SetParameter(vccs, "gain", parameters.GetString(2));
                return vccs;
            }

            if (parameters.Count == 5)
            {
                var vccs = new VoltageControlledCurrentSource(name);
                context.CreateNodes(vccs, parameters);

                if (parameters[4] is SingleParameter sp)
                {
                    context.SetParameter(vccs, "gain", sp.Image);
                    return vccs;
                }
            }

            throw new WrongParametersCountException(name, "invalid syntax for voltage controlled current source");
        }

        /// <summary>
        /// Generates a new current source.
        /// </summary>
        /// <param name="name">Name of generated current source.</param>
        /// <param name="parameters">Parameters for current source.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of current source.
        /// </returns>
        protected SpiceSharp.Components.Component GenerateCurrentSource(string name,  ParameterCollection parameters, IReadingContext context)
        {
            CurrentSource isrc = new CurrentSource(name);
            context.CreateNodes(isrc, parameters);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters[i] is SingleParameter s && s.Image.ToLower() == "dc" && i != parameters.Count - 1)
                {
                    context.SetParameter(isrc, "dc", parameters.GetString(i + 1));
                    i++;
                }
                else if (i == 2 && parameters[i] is SingleParameter vp && parameters[i].Image.ToLower() != "dc" && parameters[i].Image.ToLower() != "ac")
                {
                    if (parameters[i] is WordParameter && context.WaveformReader.Supports(parameters[i].Image, context))
                    {
                        isrc.SetParameter("waveform", context.WaveformReader.Generate(parameters[i].Image, parameters.Skip(i + 1), context));
                        return isrc;
                    }
                    else
                    {
                        context.SetParameter(isrc, "dc", parameters.GetString(i));
                    }
                }
                else if (parameters[i] is SingleParameter s2 && s2.Image.ToLower() == "ac")
                {
                    i++;
                    if (i < parameters.Count)
                    {
                        if (parameters[i] is SingleParameter == false)
                        {
                            throw new WrongParameterTypeException(name, "Current source AC magnitude has wrong type of parameter: " + parameters[i].GetType());
                        }

                        context.SetParameter(isrc, "acmag", parameters.GetString(i));

                        // Look forward for one more value
                        if (i + 1 < parameters.Count)
                        {
                            // support for all single parameters
                            if (parameters[i + 1] is SingleParameter)
                            {
                                i++;
                                context.SetParameter(isrc, "acphase", parameters.GetString(i));
                            }
                            else
                            {
                                if (!(parameters[i + 1] is BracketParameter))
                                {
                                    throw new WrongParameterTypeException(name, "Current source AC phase has wrong type of parameter: " + parameters[i].GetType());
                                }
                            }
                        }
                    }
                }
                else if (parameters[i] is BracketParameter cp)
                {
                    isrc.SetParameter("waveform", context.WaveformReader.Generate(cp.Name, cp.Parameters, context));
                }
                else if (parameters[i] is AssignmentParameter ap && ap.Name.ToLower() == "value")
                {
                    context.SetParameter(isrc, "dc", ap.Value);
                }
                else
                {
                    if (parameters[i].Image.ToLower() != "dc")
                    {
                        throw new WrongParameterException("Wrong parameter at the position " + (i + 1) + " for current source: " + parameters[i].Image);
                    }
                }
            }

            return isrc;
        }
    }
}
