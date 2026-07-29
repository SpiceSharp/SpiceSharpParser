using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Evaluates A-device parameter expressions and rejects sweep-dependent values.
    /// </summary>
    internal static class LTspiceADeviceParameterEvaluator
    {
        public static bool TryMap(
            string componentName,
            IReadOnlyDictionary<string, ADeviceParameter> source,
            IReadOnlyDictionary<string, string> map,
            IReadingContext context,
            out Dictionary<string, string> result)
        {
            result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool success = true;
            foreach (KeyValuePair<string, ADeviceParameter> item in source)
            {
                if (!map.TryGetValue(item.Key, out string portableName))
                {
                    LTspiceADeviceDiagnostics.AddError(
                        context,
                        $"Unsupported LTspice A-device parameter '{item.Key}' on component '{componentName}'.",
                        item.Value.LineInfo);
                    success = false;
                    continue;
                }

                if (portableName == null)
                {
                    continue;
                }

                if (TryEvaluateExpression(
                        componentName,
                        item.Key,
                        item.Value,
                        context,
                        out double value))
                {
                    result[portableName] = value.ToString("R", CultureInfo.InvariantCulture);
                }
                else
                {
                    success = false;
                }
            }

            return success;
        }

        public static bool TryEvaluate(
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            string name,
            string componentName,
            IReadingContext context,
            double defaultValue,
            out double result)
        {
            if (!parameters.TryGetValue(name, out ADeviceParameter parameter))
            {
                result = defaultValue;
                return true;
            }

            return TryEvaluateExpression(
                componentName,
                name,
                parameter,
                context,
                out result);
        }

        private static bool TryEvaluateExpression(
            string componentName,
            string parameterName,
            ADeviceParameter parameter,
            IReadingContext context,
            out double result)
        {
            if (TryFindSteppedParameter(
                    parameter.Expression,
                    context,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    out string steppedParameter))
            {
                LTspiceADeviceDiagnostics.AddError(
                    context,
                    $"LTspice A-device '{componentName}' parameter '{parameterName}' depends on "
                    + $"stepped parameter '{steppedParameter}'. Sweep-dependent A-device parameters "
                    + "are not supported because expanding them would freeze the first sweep value.",
                    parameter.LineInfo);
                result = double.NaN;
                return false;
            }

            try
            {
                result = context.Evaluator.EvaluateDouble(parameter.Expression);
                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    LTspiceADeviceDiagnostics.AddError(
                        context,
                        $"LTspice A-device '{componentName}' parameter '{parameterName}' must be finite.",
                        parameter.LineInfo);
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                LTspiceADeviceDiagnostics.AddError(
                    context,
                    $"Could not evaluate LTspice A-device '{componentName}' parameter "
                    + $"'{parameterName}': {exception.Message}",
                    parameter.LineInfo);
                result = double.NaN;
                return false;
            }
        }

        private static bool TryFindSteppedParameter(
            string expression,
            IReadingContext context,
            ISet<string> visited,
            out string steppedParameter)
        {
            steppedParameter = null;
            foreach (string dependency in context.EvaluationContext.GetExpressionParameters(expression, false))
            {
                if (context.SimulationConfiguration.RegisteredParameterSweeps.Contains(dependency))
                {
                    steppedParameter = dependency;
                    return true;
                }

                if (visited.Add(dependency)
                    && context.EvaluationContext.Parameters.TryGetValue(dependency, out var parameter)
                    && TryFindSteppedParameter(
                        parameter.ValueExpression,
                        context,
                        visited,
                        out steppedParameter))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
