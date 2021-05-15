using System.Linq;
using SpiceSharp.Components;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Component = SpiceSharp.Components.Component;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    public abstract class SourceGenerator : ComponentGenerator
    {
        protected void SetSourceParameters(
            ParameterCollection parameters,
            IReadingContext context,
            Component component,
            bool isCurrentSource)
        {
            var originalParameters = parameters;
            parameters = parameters.Skip(VoltageSource.PinCount);

            var acParameter = parameters.FirstOrDefault(p => p.Value.ToLower() == "ac");
            if (acParameter != null)
            {
                int acParameterIndex = parameters.IndexOf(acParameter);

                if (acParameterIndex != parameters.Count - 1)
                {
                    var acParameterValue = parameters.Get(acParameterIndex + 1);
                    context.SetParameter(component, "acmag", acParameterValue);

                    if (acParameterIndex + 1 != parameters.Count - 1)
                    {
                        // Check first if next parameter is waveform
                        var acPhaseCandidate = parameters[acParameterIndex + 2].Value;
                        if (parameters[acParameterIndex + 2] is SingleParameter
                            && !context.WaveformReader.Supports(acPhaseCandidate, context)
                            && acPhaseCandidate.ToLower() != "dc")
                        {
                            var acPhaseParameterValue = parameters.Get(acParameterIndex + 2);
                            context.SetParameter(component, "acphase", acPhaseParameterValue);

                            parameters.RemoveAt(acParameterIndex + 2);
                        }
                    }

                    parameters.RemoveAt(acParameterIndex + 1);
                }

                parameters.RemoveAt(acParameterIndex);
            }

            // 2. Set DC
            var dcParameter = parameters.FirstOrDefault(p => p.Value.ToLower() == "dc");
            if (dcParameter != null)
            {
                int dcParameterIndex = parameters.IndexOf(dcParameter);
                if (dcParameterIndex != parameters.Count - 1)
                {
                    var dcParameterValue = parameters.Get(dcParameterIndex + 1);
                    context.SetParameter(component, "dc", dcParameterValue);
                    parameters.RemoveAt(dcParameterIndex + 1);
                }

                parameters.RemoveAt(dcParameterIndex);
            }
            else
            {
                if (parameters.Count > 0
                    && parameters[0] is SingleParameter sp
                    && !context.WaveformReader.Supports(sp.Value, context)
                    && parameters[0].Value.ToLower() != "value")
                {
                    context.SetParameter(component, "dc", sp);
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
                        context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Unsupported waveform: {bp.Name}", bp.LineInfo));
                    }
                }
                else
                {
                    if (firstParameter is WordParameter wp && wp.Value.ToLower() != "value")
                    {
                        if (context.WaveformReader.Supports(wp.Value, context))
                        {
                            component.SetParameter("waveform", context.WaveformReader.Generate(wp.Value, parameters.Skip(1), context));
                        }
                        else
                        {
                            context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Unsupported waveform: {wp}", wp.LineInfo));
                        }
                    }

                    if (firstParameter is AssignmentParameter assignmentParameter)
                    {
                        if (context.WaveformReader.Supports(assignmentParameter.Name, context))
                        {
                            component.SetParameter("waveform", context.WaveformReader.Generate(assignmentParameter.Name, parameters, context));
                        }
                        else
                        {
                            context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Unsupported waveform: {assignmentParameter.Name}", assignmentParameter.LineInfo));
                        }
                    }
                }

                if (firstParameter is AssignmentParameter ap && ap.Name.ToLower() == "value")
                {
                    context.SetParameter(component, "dc", ap.Value);
                }

                if (parameters.Count >= 2
                    && parameters[0].Value.ToLower() == "value"
                    && parameters[1] is SingleParameter)
                {
                    context.SetParameter(component, "dc", parameters[1].Value);
                }
            }

            if (isCurrentSource && parameters.Any(p => p is AssignmentParameter mParameter && mParameter.Name.ToLower() == "m"))
            {
                var mParameter = parameters.First(p => p is AssignmentParameter m && m.Name.ToLower() == "m");
                context.SetParameter(component, "m", mParameter);
            }
        }

        protected string CreatePolyExpression(int dimension, ParameterCollection parameters, bool isVoltageControlled, IEvaluationContext context)
        {
            if (isVoltageControlled)
            {
                return ExpressionFactory.CreatePolyVoltageExpression(dimension, parameters, context);
            }

            return ExpressionFactory.CreatePolyCurrentExpression(dimension, parameters, context);
        }
    }
}