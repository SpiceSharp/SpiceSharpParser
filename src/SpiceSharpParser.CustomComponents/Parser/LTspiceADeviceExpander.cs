using System;
using System.Collections.Generic;
using System.Globalization;
using SpiceSharp.Components;
using SpiceSharpParser.CustomComponents.Analog;
using SpiceSharpParser.CustomComponents.Digital;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Applies one catalog definition to an already-read and validated A-device.
    /// </summary>
    internal sealed class LTspiceADeviceExpander
    {
        private readonly DigitalSubcircuitLibrary _digital;
        private readonly DigitalSubcircuitLibrary _zeroDelayDigital;
        private readonly AnalogSubcircuitLibrary _analog;

        public LTspiceADeviceExpander()
        {
            _digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            _zeroDelayDigital = DigitalSubcircuitLibrary.LoadADeviceZeroDelay();
            _analog = AnalogSubcircuitLibrary.LoadBuiltIn();
        }

        public bool TryExpand(
            IReadingContext context,
            string originalName,
            string portableInstanceName,
            LTspiceADeviceInstance instance,
            LTspiceADeviceDefinition definition,
            LTspiceADeviceConnections connections)
        {
            if (!LTspiceADeviceParameterEvaluator.TryMap(
                    originalName,
                    instance.Parameters,
                    definition.ParameterMap,
                    context,
                    out Dictionary<string, string> mappedParameters))
            {
                return false;
            }

            string highRailNode = null;
            string lowRailNode = null;
            if (definition.CreatesDigitalRails
                && !TryCreateDigitalRails(
                    context,
                    originalName,
                    portableInstanceName,
                    connections.CommonNode,
                    instance.Parameters,
                    mappedParameters,
                    out highRailNode,
                    out lowRailNode))
            {
                return false;
            }

            ApplyParameterAdjustment(
                definition.ParameterAdjustment,
                connections,
                mappedParameters);

            string subcircuitName = definition.SubcircuitName;
            DigitalSubcircuitLibrary digitalLibrary = _digital;
            if (definition.ZeroDelaySubcircuitName != null
                && UsesZeroDelay(mappedParameters))
            {
                subcircuitName = definition.ZeroDelaySubcircuitName;
                digitalLibrary = _zeroDelayDigital;
                mappedParameters.Remove("TPD");
            }

            if (definition.AppliesDigitalCompatibilityDefaults)
            {
                ApplyDigitalCompatibilityDefaults(mappedParameters);
            }

            IReadOnlyList<string> nodes = connections.CreateSubcircuitNodes(
                definition,
                highRailNode,
                lowRailNode);

            if (definition.LibraryKind == ADeviceLibraryKind.Digital)
            {
                digitalLibrary.Library.AddInstance(
                    context.ContextEntities,
                    subcircuitName,
                    portableInstanceName,
                    nodes,
                    mappedParameters);
            }
            else
            {
                _analog.Library.AddInstance(
                    context.ContextEntities,
                    subcircuitName,
                    portableInstanceName,
                    nodes,
                    mappedParameters);
            }

            return true;
        }

        private static void ApplyParameterAdjustment(
            ADeviceParameterAdjustment adjustment,
            LTspiceADeviceConnections connections,
            IDictionary<string, string> parameters)
        {
            switch (adjustment)
            {
                case ADeviceParameterAdjustment.OtaCurrentLimits:
                    ApplyOtaCurrentLimitDefaults(parameters);
                    break;
                case ADeviceParameterAdjustment.DefaultUnusedModulatorAmplitude:
                    if (connections.IsUnused(ADeviceTerminal.T2))
                    {
                        parameters["AMDEFAULT"] = "1";
                    }

                    break;
            }
        }

        private static void ApplyOtaCurrentLimitDefaults(
            IDictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("IOUT", out string outputLimit))
            {
                return;
            }

            if (!parameters.ContainsKey("ISRC"))
            {
                parameters["ISRC"] = outputLimit;
            }

            if (!parameters.ContainsKey("ISINK"))
            {
                parameters["ISINK"] = (-double.Parse(
                    outputLimit,
                    CultureInfo.InvariantCulture)).ToString(
                        "R",
                        CultureInfo.InvariantCulture);
            }
        }

        private static bool TryCreateDigitalRails(
            IReadingContext context,
            string originalName,
            string instanceName,
            string commonNode,
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            IDictionary<string, string> mapped,
            out string highNode,
            out string lowNode)
        {
            highNode = instanceName + ".__a_vhigh";
            lowNode = instanceName + ".__a_vlow";
            if (!TryEvaluate(
                    parameters,
                    "vhigh",
                    originalName,
                    context,
                    1.0,
                    out double high)
                || !TryEvaluate(
                    parameters,
                    "vlow",
                    originalName,
                    context,
                    0.0,
                    out double low))
            {
                return false;
            }

            if (high <= low)
            {
                LTspiceADeviceDiagnostics.AddError(
                    context,
                    $"LTspice A-device '{originalName}' Vhigh ({high}) must be greater than Vlow ({low}).",
                    parameters.TryGetValue("vhigh", out ADeviceParameter highParameter)
                        ? highParameter.LineInfo
                        : null);
                return false;
            }

            if (parameters.ContainsKey("ref"))
            {
                if (!TryEvaluate(
                        parameters,
                        "ref",
                        originalName,
                        context,
                        0.0,
                        out double reference))
                {
                    return false;
                }

                mapped["VTH"] = ((reference - low) / (high - low)).ToString(
                    "R",
                    CultureInfo.InvariantCulture);
            }

            string highSourceName = "V" + instanceName + ".__a_vhigh";
            string lowSourceName = "V" + instanceName + ".__a_vlow";
            if (context.ContextEntities.Contains(highSourceName)
                || context.ContextEntities.Contains(lowSourceName))
            {
                LTspiceADeviceDiagnostics.AddError(
                    context,
                    $"Internal rail entities for LTspice A-device '{instanceName}' already exist.",
                    null);
                return false;
            }

            context.ContextEntities.Add(
                new VoltageSource(highSourceName, highNode, commonNode, high));
            context.ContextEntities.Add(
                new VoltageSource(lowSourceName, lowNode, commonNode, low));
            return true;
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

        private static bool UsesZeroDelay(
            IReadOnlyDictionary<string, string> parameters)
        {
            return !parameters.TryGetValue("TPD", out string delay)
                || (double.TryParse(
                        delay,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double delayValue)
                    && delayValue == 0.0);
        }

        private static void ApplyDigitalCompatibilityDefaults(
            IDictionary<string, string> parameters)
        {
            if (!parameters.ContainsKey("ROUT"))
            {
                parameters["ROUT"] = "1";
            }

            parameters["COUT"] = "1f";
        }
    }
}
