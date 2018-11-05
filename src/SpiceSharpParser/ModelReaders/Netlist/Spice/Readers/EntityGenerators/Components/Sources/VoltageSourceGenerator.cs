using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    /// <summary>
    /// Voltage sources generator.
    /// </summary>
    public class VoltageSourceGenerator : ComponentGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSourceGenerator"/> class.
        /// </summary>
        public VoltageSourceGenerator()
        {
        }

        /// <summary>
        /// Gets generated types.
        /// </summary>
        /// <returns>
        /// Generated types.
        /// </returns>
        public override IEnumerable<string> GeneratedTypes => new List<string>() { "V", "H", "E" };

        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type.ToLower())
            {
                case "v": return GenerateVoltageSource(componentIdentifier, parameters, context);
                case "h": return GenerateCurrentControlledVoltageSource(componentIdentifier, parameters, context);
                case "e": return GenerateVoltageControlledVoltageSource(componentIdentifier, parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Generates new voltage controlled voltage source: EName.
        /// </summary>
        /// <param name="name">The name of voltage source to generate.</param>
        /// <param name="parameters">The parameters for voltage source.</param>
        /// <param name="context">The reading context.</param>
        /// <returns>
        /// A new instance of voltage controlled voltage source.
        /// </returns>
        protected SpiceSharp.Components.Component GenerateVoltageControlledVoltageSource(
            string name,
            ParameterCollection parameters,
            IReadingContext context)
        {
            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value")
                && parameters.Count == 3)
            {
                var valueParameter = (AssignmentParameter)parameters.Single(
                    p => p is AssignmentParameter ap && ap.Name.ToLower() == "value");

                var vs = new VoltageSource(name);
                context.CreateNodes(vs, parameters);
                context.SetParameter(vs, "dc", valueParameter.Value);
                return vs;
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

                var vs = new VoltageSource(name);
                context.CreateNodes(vs, parameters);
                context.SetParameter(vs, "dc", expression);
                return vs;
            }

            if (parameters.Any(p => p is ExpressionEqualParameter eep) && parameters.Any(p => p.Image.ToLower() == "table"))
            {
                var formulaParameter = (ExpressionEqualParameter)parameters.Single(p => p is ExpressionEqualParameter);

                var vs = new VoltageSource(name);
                context.CreateNodes(vs, parameters);

                var tableParameter = name + "_table_variable";
                context.SetParameter(tableParameter, formulaParameter.Expression);
                string expression = ExpressionGenerator.CreateTableExpression(tableParameter, formulaParameter);
                context.SetParameter(vs, "dc", expression);
                return vs;
            }

            if (parameters.Count == 3 
                && parameters[0] is PointParameter pp1 
                && pp1.Values.Count() == 2
                && parameters[1] is PointParameter pp2
                && pp2.Values.Count() == 2)
            {
                var vcvs = new VoltageControlledVoltageSource(name);
                var vcvsNodes = new ParameterCollection();
                vcvsNodes.Add(pp1.Values.Items[0]);
                vcvsNodes.Add(pp1.Values.Items[1]);
                vcvsNodes.Add(pp2.Values.Items[0]);
                vcvsNodes.Add(pp2.Values.Items[1]);
                context.CreateNodes(vcvs, vcvsNodes);
                context.SetParameter(vcvs, "gain", parameters.GetString(2));
                return vcvs;
            }

            if (parameters.Count == 5)
            {
                var vcvs = new VoltageControlledVoltageSource(name);
                context.CreateNodes(vcvs, parameters);

                if (parameters[4] is SingleParameter sp)
                {
                    context.SetParameter(vcvs, "gain", sp.Image);
                    return vcvs;
                }
            }

            throw new WrongParametersCountException(name, "invalid syntax for voltage controlled voltage source");
        }

        /// <summary>
        /// Generates new current controlled voltage source HName.
        /// </summary>
        /// <param name="name">The name of voltage source to generate.</param>
        /// <param name="parameters">The parameters for voltage source.</param>
        /// <param name="context">The reading context.</param>
        /// <returns>
        /// A new instance of current controlled voltage source.
        /// </returns>
        protected SpiceSharp.Components.Component GenerateCurrentControlledVoltageSource(
            string name,
            ParameterCollection parameters,
            IReadingContext context)
        {
            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value")
                && parameters.Count == 3)
            {
                var valueParameter = (AssignmentParameter)parameters.Single(
                    p => p is AssignmentParameter ap && ap.Name.ToLower() == "value");

                var vs = new VoltageSource(name);
                context.CreateNodes(vs, parameters);
                context.SetParameter(vs, "dc", valueParameter.Value);
                return vs;
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

                var vs = new VoltageSource(name);
                context.CreateNodes(vs, parameters);
                context.SetParameter(vs, "dc", expression);
                return vs;
            }

            if (parameters.Any(p => p is ExpressionEqualParameter) && parameters.Any(p => p.Image.ToLower() == "table"))
            {
                var formulaParameter = (ExpressionEqualParameter)parameters.Single(p => p is ExpressionEqualParameter);

                var vs = new VoltageSource(name);
                context.CreateNodes(vs, parameters);

                var tableParameter = name + "_table_variable";
                context.SetParameter(tableParameter, formulaParameter.Expression);
                string expression = ExpressionGenerator.CreateTableExpression(tableParameter, formulaParameter);
                context.SetParameter(vs, "dc", expression);
                return vs;
            }

            if (parameters.Count == 4)
            {
                var ccvs = new CurrentControlledVoltageSource(name);
                context.CreateNodes(ccvs, parameters);
                ccvs.ControllingName = parameters.GetString(2);
                context.SetParameter(ccvs, "gain", parameters.GetString(3));

                return ccvs;
            }

            throw new WrongParametersCountException(name, "invalid syntax for current controlled voltage source");
        }

        /// <summary>
        /// Generates new voltage source.
        /// </summary>
        /// <param name="name">The name of voltage source to generate.</param>
        /// <param name="parameters">The parameters for voltage source.</param>
        /// <param name="context">The reading context.</param>
        /// <returns>
        /// A new instance of voltage source.
        /// </returns>
        protected SpiceSharp.Components.Component GenerateVoltageSource(string name, ParameterCollection parameters, IReadingContext context)
        {
            var vsrc = new VoltageSource(name);
            context.CreateNodes(vsrc, parameters);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters[i] is SingleParameter s && s.Image.ToLower() == "dc" && i != parameters.Count - 1)
                {
                    context.SetParameter(vsrc, "dc", parameters.GetString(i + 1));
                    i++;
                }
                else if (i == 2 && parameters[i] is SingleParameter vp && parameters[i].Image.ToLower() != "dc" && parameters[i].Image.ToLower() != "ac")
                {
                    if (parameters[i] is WordParameter && context.WaveformReader.Supports(parameters[i].Image, context))
                    {
                        vsrc.SetParameter("waveform", context.WaveformReader.Generate(parameters[i].Image, parameters.Skip(i + 1), context));
                        return vsrc;
                    }
                    else
                    {
                        context.SetParameter(vsrc, "dc", parameters.GetString(i));
                    }
                }
                else if (parameters[i] is SingleParameter s2 && s2.Image.ToLower() == "ac")
                {
                    i++;
                    if (i < parameters.Count)
                    {
                        if (parameters[i] is SingleParameter == false)
                        {
                            throw new WrongParameterTypeException(name, "Voltage source AC magnitude has wrong type of parameter: " + parameters[i].GetType());
                        }

                        context.SetParameter(vsrc, "acmag", parameters.GetString(i));

                        // Look forward for one more value
                        if (i + 1 < parameters.Count)
                        {
                            // support for all single parameters
                            if (parameters[i + 1] is SingleParameter)
                            {
                                i++;
                                context.SetParameter(vsrc, "acphase", parameters.GetString(i));
                            }
                            else
                            {
                                if (!(parameters[i + 1] is BracketParameter))
                                {
                                    throw new WrongParameterTypeException(name, "Voltage source AC phase has wrong type of parameter: " + parameters[i].GetType());
                                }
                            }
                        }
                    }
                }
                else if (parameters[i] is BracketParameter cp)
                {
                    vsrc.SetParameter("waveform", context.WaveformReader.Generate(cp.Name, cp.Parameters, context));
                }
                else if (parameters[i] is AssignmentParameter ap)
                {
                    if (ap.Name.ToLower() == "value")
                    {
                        context.SetParameter(vsrc, "dc", ap.Value);
                    }
                    else
                    {
                        throw new WrongParameterException("Wrong parameter at the position " + i + " for voltage source: " + parameters[i].Image);
                    }
                }
                else
                {
                    if (parameters[i] is WordParameter && context.WaveformReader.Supports(parameters[i].Image, context))
                    {
                        vsrc.SetParameter("waveform", context.WaveformReader.Generate(parameters[i].Image, parameters.Skip(i + 1), context));
                        return vsrc;
                    }
                    else if (parameters[i].Image.ToLower() != "dc")
                    {
                        throw new WrongParameterException("Wrong parameter at the position " + (i + 1) + " for voltage source: " + parameters[i].Image);
                    }
                }
            }

            return vsrc;
        }
    }
}
