using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Applies model-specific requirements after A-device syntax has been read.
    /// </summary>
    internal static class LTspiceADeviceValidator
    {
        public static bool TryValidate(
            string componentName,
            string model,
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            IReadingContext context)
        {
            bool success = true;
            switch (model)
            {
                case "SRFLOP":
                case "DFLOP":
                    success &= TryValidateOptional(
                        componentName, parameters, "td", context, value => value >= 0.0, "zero or greater");
                    success &= TryValidateOptional(
                        componentName, parameters, "rout", context, value => value > 0.0, "greater than zero");
                    success &= TryValidateOptional(
                        componentName, parameters, "ic", context, value => value >= 0.0 && value <= 1.0, "between zero and one");
                    break;
                case "PHASEDET":
                    success &= TryValidateOptional(
                        componentName, parameters, "iout", context, value => value > 0.0, "greater than zero");
                    success &= TryValidateOptional(
                        componentName, parameters, "rout", context, value => value > 0.0, "greater than zero");
                    success &= TryValidateOptional(
                        componentName, parameters, "rclamp", context, value => value > 0.0, "greater than zero");
                    success &= TryValidateOptional(
                        componentName, parameters, "cout", context, value => value >= 0.0, "zero or greater");
                    success &= TryValidateVoltageWindow(componentName, parameters, context, 10.0, -10.0);
                    break;
                case "COUNTER":
                    if (!parameters.ContainsKey("cycles"))
                    {
                        LTspiceADeviceDiagnostics.AddError(
                            context,
                            $"LTspice A-device '{componentName}' (COUNTER) requires the Cycles parameter.",
                            null);
                        success = false;
                    }
                    else if (TryEvaluate(parameters, "cycles", componentName, context, 0.0, out double cycles))
                    {
                        if (cycles < 2.0 || Math.Abs(cycles - Math.Round(cycles)) > 1e-12)
                        {
                            AddParameterRangeError(
                                context,
                                componentName,
                                "cycles",
                                "an integer of at least two",
                                parameters["cycles"]);
                            success = false;
                        }
                    }
                    else
                    {
                        success = false;
                    }

                    success &= TryValidateOptional(
                        componentName, parameters, "duty", context, value => value > 0.0 && value < 1.0, "greater than zero and less than one");
                    success &= TryValidateOptional(
                        componentName, parameters, "rout", context, value => value > 0.0, "greater than zero");
                    break;
                case "SAMPLEHOLD":
                    if (parameters.TryGetValue("td", out ADeviceParameter delay))
                    {
                        LTspiceADeviceDiagnostics.AddError(
                            context,
                            $"LTspice A-device '{componentName}' parameter 'Td' is not supported for "
                            + "SAMPLEHOLD because its clock and track timing semantics are not yet implemented.",
                            delay.LineInfo);
                        success = false;
                    }

                    success &= TryValidateOptional(
                        componentName, parameters, "rout", context, value => value > 0.0, "greater than zero");
                    success &= TryValidateVoltageWindow(componentName, parameters, context, 10.0, -10.0);
                    break;
                case "OTA":
                    success &= TryValidateOta(componentName, parameters, context);
                    break;
                case "VARISTOR":
                    success &= TryValidateOptional(
                        componentName, parameters, "rclamp", context, value => value > 0.0, "greater than zero");
                    success &= TryValidateOptional(
                        componentName, parameters, "roff", context, value => value > 0.0, "greater than zero");
                    break;
                case "MODULATE":
                case "MODULATOR":
                    if (!parameters.ContainsKey("mark"))
                    {
                        LTspiceADeviceDiagnostics.AddError(
                            context,
                            $"LTspice A-device '{componentName}' ({model}) requires the Mark parameter.",
                            null);
                        success = false;
                    }
                    else
                    {
                        success &= TryValidateOptional(
                            componentName, parameters, "mark", context, value => value >= 0.0, "zero or greater");
                    }

                    if (!parameters.ContainsKey("space"))
                    {
                        LTspiceADeviceDiagnostics.AddError(
                            context,
                            $"LTspice A-device '{componentName}' ({model}) requires the Space parameter.",
                            null);
                        success = false;
                    }
                    else
                    {
                        success &= TryValidateOptional(
                            componentName, parameters, "space", context, value => value >= 0.0, "zero or greater");
                    }

                    success &= TryValidateOptional(
                        componentName, parameters, "rout", context, value => value > 0.0, "greater than zero");
                    break;
            }

            return success;
        }

        private static bool TryValidateOta(
            string componentName,
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            IReadingContext context)
        {
            bool success = true;
            if (!TryEvaluate(parameters, "iout", componentName, context, 10e-6, out double outputLimit))
            {
                success = false;
            }
            else if (outputLimit <= 0.0)
            {
                AddParameterRangeError(
                    context,
                    componentName,
                    "iout",
                    "greater than zero",
                    parameters.TryGetValue("iout", out ADeviceParameter iout) ? iout : null);
                success = false;
            }

            if (!TryEvaluate(parameters, "isrc", componentName, context, outputLimit, out double sourceLimit))
            {
                success = false;
            }
            else if (sourceLimit <= 0.0)
            {
                AddParameterRangeError(
                    context,
                    componentName,
                    "isrc",
                    "greater than zero",
                    parameters.TryGetValue("isrc", out ADeviceParameter isrc) ? isrc : null);
                success = false;
            }

            if (!TryEvaluate(parameters, "isink", componentName, context, -outputLimit, out double sinkLimit))
            {
                success = false;
            }
            else if (sinkLimit >= 0.0)
            {
                AddParameterRangeError(
                    context,
                    componentName,
                    "isink",
                    "less than zero",
                    parameters.TryGetValue("isink", out ADeviceParameter isink) ? isink : null);
                success = false;
            }

            success &= TryValidateOptional(
                componentName, parameters, "g", context, value => value > 0.0, "greater than zero");
            success &= TryValidateOptional(
                componentName, parameters, "rout", context, value => value > 0.0, "greater than zero");
            success &= TryValidateOptional(
                componentName, parameters, "rclamp", context, value => value > 0.0, "greater than zero");
            success &= TryValidateOptional(
                componentName, parameters, "cout", context, value => value >= 0.0, "zero or greater");
            success &= TryValidateVoltageWindow(componentName, parameters, context, 2.0, 0.0);
            return success;
        }

        private static bool TryValidateOptional(
            string componentName,
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            string parameterName,
            IReadingContext context,
            Func<double, bool> predicate,
            string requirement)
        {
            if (!parameters.ContainsKey(parameterName))
            {
                return true;
            }

            if (!TryEvaluate(parameters, parameterName, componentName, context, 0.0, out double value))
            {
                return false;
            }

            if (predicate(value))
            {
                return true;
            }

            AddParameterRangeError(
                context,
                componentName,
                parameterName,
                requirement,
                parameters[parameterName]);
            return false;
        }

        private static bool TryValidateVoltageWindow(
            string componentName,
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            IReadingContext context,
            double defaultHigh,
            double defaultLow)
        {
            if (!TryEvaluate(parameters, "vhigh", componentName, context, defaultHigh, out double high)
                || !TryEvaluate(parameters, "vlow", componentName, context, defaultLow, out double low))
            {
                return false;
            }

            if (high > low)
            {
                return true;
            }

            LTspiceADeviceDiagnostics.AddError(
                context,
                $"LTspice A-device '{componentName}' Vhigh ({high}) must be greater than Vlow ({low}).",
                parameters.TryGetValue("vhigh", out ADeviceParameter highParameter)
                    ? highParameter.LineInfo
                    : null);
            return false;
        }

        private static bool TryEvaluate(
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            string name,
            string componentName,
            IReadingContext context,
            double defaultValue,
            out double result)
        {
            return LTspiceADeviceParameterEvaluator.TryEvaluate(
                parameters,
                name,
                componentName,
                context,
                defaultValue,
                out result);
        }

        private static void AddParameterRangeError(
            IReadingContext context,
            string componentName,
            string parameterName,
            string requirement,
            ADeviceParameter parameter)
        {
            LTspiceADeviceDiagnostics.AddError(
                context,
                $"LTspice A-device '{componentName}' parameter '{parameterName}' must be {requirement}.",
                parameter?.LineInfo);
        }
    }
}
