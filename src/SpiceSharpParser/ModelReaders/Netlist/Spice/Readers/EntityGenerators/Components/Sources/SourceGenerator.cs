using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    public abstract class SourceGenerator : ComponentGenerator
    {
        protected ISpiceDerivativeParser<double> CreateParser(IReadingContext context)
        {
            var parser = new SimpleDerivativeParser();
            parser.VariableFound += (sender, args) =>
            {
                if (context.ReadingExpressionContext.Parameters.TryGetValue(args.Name, out var expression))
                {
                    var d = new DoubleDerivatives(1);
                    d[0] = () => context.EvaluateDouble(expression.ValueExpression);
                    args.Result = d;
                }
            };
            parser.FunctionFound += (sender, args) =>
            {
                if (context.ReadingExpressionContext.Functions.TryGetValue(args.Name, out var functions))
                {
                    var function = functions.First() as IFunction<double, double>;

                    if (function != null)
                    {
                        var d = new DoubleDerivatives(1);

                        var arguments = new List<Func<double>>();

                        for (var i = 0; i < args.ArgumentCount; i++)
                        {
                            if (args[i].Count > 0)
                            {
                                var arg = args[i];
                                arguments.Add(() => arg[0]());
                            }
                        }

                        d[0] = () => function.Logic(string.Empty, arguments.Select(arg => arg()).ToArray(), context.ReadingEvaluator, context.ReadingExpressionContext);
                        args.Result = d;
                    }
                }
            };

            return parser;
        }

        protected SpiceSharp.Components.Component SetSourceParameters(
           string name,
           ParameterCollection parameters,
           IReadingContext context,
           SpiceSharp.Components.Component component)
        {
            var originalParameters = parameters;
            parameters = parameters.Skip(VoltageSource.VoltageSourcePinCount);

            var acParameter = parameters.FirstOrDefault(p => p.Image.ToLower() == "ac");
            if (acParameter != null)
            {
                int acParameterIndex = parameters.IndexOf(acParameter);

                if (acParameterIndex != parameters.Count - 1)
                {
                    var acParameterValue = parameters.GetValueString(acParameterIndex + 1);
                    context.SetParameter(component, "acmag", acParameterValue);

                    if (acParameterIndex + 1 != parameters.Count - 1)
                    {
                        // Check first if next parameter is waveform
                        var acPhaseCandidate = parameters[acParameterIndex + 2].Image;
                        if (parameters[acParameterIndex + 2] is SingleParameter
                            && !context.WaveformReader.Supports(acPhaseCandidate, context)
                            && acPhaseCandidate.ToLower() != "dc")
                        {
                            var acPhaseParameterValue = parameters.GetValueString(acParameterIndex + 2);
                            context.SetParameter(component, "acphase", acPhaseParameterValue);

                            parameters.RemoveAt(acParameterIndex + 2);
                        }
                    }

                    parameters.RemoveAt(acParameterIndex + 1);
                }

                parameters.RemoveAt(acParameterIndex);
            }

            // 2. Set DC
            var dcParameter = parameters.FirstOrDefault(p => p.Image.ToLower() == "dc");
            if (dcParameter != null)
            {
                int dcParameterIndex = parameters.IndexOf(dcParameter);
                if (dcParameterIndex != parameters.Count - 1)
                {
                    var dcParameterValue = parameters.GetValueString(dcParameterIndex + 1);
                    context.SetParameter(component, "dc", dcParameterValue);
                    parameters.RemoveAt(dcParameterIndex + 1);
                }

                parameters.RemoveAt(dcParameterIndex);
            }
            else
            {
                if (parameters.Count > 0
                    && parameters[0] is SingleParameter sp
                    && !context.WaveformReader.Supports(sp.Image, context)
                    && parameters[0].Image.ToLower() != "value")
                {
                    context.SetParameter(component, "dc", sp.Image);
                    parameters.RemoveAt(0);
                }
            }

            // 3. Set up waveform
            if (parameters.Count > 0)
            {
                var firstParameter = parameters[0];

                if (firstParameter is BracketParameter bp)
                {
                    if (context.WaveformReader.Supports(bp.Name, context))
                    {
                        component.SetParameter("waveform", context.WaveformReader.Generate(bp.Name, bp.Parameters, context));
                    }
                    else
                    {
                        throw new WrongParameterTypeException(name, "Unsupported waveform: " + bp.Name);
                    }
                }
                else
                {
                    if (firstParameter is WordParameter wp && wp.Image.ToLower() != "value")
                    {
                        if (context.WaveformReader.Supports(wp.Image, context))
                        {
                            component.SetParameter("waveform", context.WaveformReader.Generate(wp.Image, parameters.Skip(1), context));
                        }
                        else
                        {
                            throw new WrongParameterTypeException(name, "Unsupported waveform: " + wp);
                        }
                    }
                }
            }

            // 4. Value = { }, Value { }
            if (parameters.Count > 0)
            {
                if (parameters[0] is AssignmentParameter ap && ap.Name.ToLower() == "value")
                {
                    context.SetParameter(component, "dc", ap.Value);
                }

                if (parameters.Count >= 2
                    && parameters[0].Image.ToLower() == "value"
                    && parameters[1] is SingleParameter)
                {
                    context.SetParameter(component, "dc", parameters[1].Image);
                }
            }

            context.CreateNodes(component, originalParameters);
            return component;
        }

        protected string CreatePolyExpression(int dimension, ParameterCollection parameters, bool isVoltageControlled)
        {
            if (isVoltageControlled)
            {
                return ExpressionFactory.CreatePolyVoltageExpression(dimension, parameters);
            }

            return ExpressionFactory.CreatePolyCurrentExpression(dimension, parameters);
        }
    }
}
