using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Identifies one of the eight terminal positions in native A-device syntax.
    /// The names deliberately match the one-based positions printed in a netlist.
    /// </summary>
    internal enum ADeviceTerminal
    {
        T1 = 0,
        T2 = 1,
        T3 = 2,
        T4 = 3,
        T5 = 4,
        T6 = 5,
        T7 = 6,
        T8Common = 7,
    }

    internal enum ADeviceLibraryKind
    {
        Digital,
        Analog,
    }

    internal enum ADeviceParameterAdjustment
    {
        None,
        OtaCurrentLimits,
        DefaultUnusedModulatorAmplitude,
    }

    /// <summary>
    /// Describes how one native LTspice A-device maps to one portable subcircuit.
    /// </summary>
    /// <remarks>
    /// Keeping the wiring and parameter map together makes this class the
    /// package's A-device "translation table". The generator can then focus on
    /// orchestration instead of repeating a method for every supported model.
    /// </remarks>
    internal sealed class LTspiceADeviceDefinition
    {
        public LTspiceADeviceDefinition(
            ADeviceLibraryKind libraryKind,
            string subcircuitName,
            IEnumerable<ADeviceTerminal> subcircuitTerminals,
            IEnumerable<ADeviceTerminal> outputTerminals,
            IReadOnlyDictionary<string, string> parameterMap,
            bool createsDigitalRails = false,
            bool appliesDigitalCompatibilityDefaults = false,
            string zeroDelaySubcircuitName = null,
            ADeviceParameterAdjustment parameterAdjustment = ADeviceParameterAdjustment.None)
        {
            LibraryKind = libraryKind;
            SubcircuitName = subcircuitName;
            SubcircuitTerminals = subcircuitTerminals.ToArray();
            OutputTerminals = outputTerminals.ToArray();
            ParameterMap = parameterMap;
            CreatesDigitalRails = createsDigitalRails;
            AppliesDigitalCompatibilityDefaults = appliesDigitalCompatibilityDefaults;
            ZeroDelaySubcircuitName = zeroDelaySubcircuitName;
            ParameterAdjustment = parameterAdjustment;
        }

        public ADeviceLibraryKind LibraryKind { get; }

        public string SubcircuitName { get; }

        public IReadOnlyList<ADeviceTerminal> SubcircuitTerminals { get; }

        public IReadOnlyList<ADeviceTerminal> OutputTerminals { get; }

        public IReadOnlyDictionary<string, string> ParameterMap { get; }

        public bool CreatesDigitalRails { get; }

        public bool AppliesDigitalCompatibilityDefaults { get; }

        public string ZeroDelaySubcircuitName { get; }

        public ADeviceParameterAdjustment ParameterAdjustment { get; }
    }

    /// <summary>
    /// The supported native model names and their portable translations.
    /// </summary>
    internal static class LTspiceADeviceCatalog
    {
        private static readonly IReadOnlyDictionary<string, LTspiceADeviceDefinition> Definitions =
            CreateDefinitions();

        public const string SupportedModelList =
            "SRFLOP, DFLOP, PHASEDET, COUNTER, SAMPLEHOLD, OTA, VARISTOR, "
            + "MODULATE, and MODULATOR";

        public static bool TryGet(
            string model,
            out LTspiceADeviceDefinition definition)
        {
            return Definitions.TryGetValue(model, out definition);
        }

        private static IReadOnlyDictionary<string, LTspiceADeviceDefinition> CreateDefinitions()
        {
            var definitions =
                new Dictionary<string, LTspiceADeviceDefinition>(StringComparer.OrdinalIgnoreCase);

            definitions["SRFLOP"] = new LTspiceADeviceDefinition(
                ADeviceLibraryKind.Digital,
                "DIG_SR_LATCH",
                Terminals(
                    ADeviceTerminal.T1,
                    ADeviceTerminal.T2,
                    ADeviceTerminal.T7,
                    ADeviceTerminal.T6),
                Terminals(ADeviceTerminal.T6, ADeviceTerminal.T7),
                Parameters(
                    ("vhigh", null),
                    ("vlow", null),
                    ("ref", null),
                    ("td", "TPD"),
                    ("rout", "ROUT"),
                    ("ic", "IC")),
                createsDigitalRails: true,
                appliesDigitalCompatibilityDefaults: true,
                zeroDelaySubcircuitName: "DIG_SR_LATCH_ZERO_DELAY");

            definitions["DFLOP"] = new LTspiceADeviceDefinition(
                ADeviceLibraryKind.Digital,
                "DIG_DFF",
                Terminals(
                    ADeviceTerminal.T1,
                    ADeviceTerminal.T3,
                    ADeviceTerminal.T4,
                    ADeviceTerminal.T5,
                    ADeviceTerminal.T7,
                    ADeviceTerminal.T6),
                Terminals(ADeviceTerminal.T6, ADeviceTerminal.T7),
                Parameters(
                    ("vhigh", null),
                    ("vlow", null),
                    ("ref", null),
                    ("td", "TPD"),
                    ("rout", "ROUT"),
                    ("ic", "IC")),
                createsDigitalRails: true,
                appliesDigitalCompatibilityDefaults: true,
                zeroDelaySubcircuitName: "DIG_DFF_ZERO_DELAY");

            definitions["PHASEDET"] = new LTspiceADeviceDefinition(
                ADeviceLibraryKind.Digital,
                "DIG_PHASE_DETECTOR",
                Terminals(
                    ADeviceTerminal.T1,
                    ADeviceTerminal.T2,
                    ADeviceTerminal.T7,
                    ADeviceTerminal.T8Common),
                Terminals(ADeviceTerminal.T7),
                Parameters(
                    ("ref", "REF"),
                    ("iout", "IOUT"),
                    ("vhigh", "VHIGH"),
                    ("vlow", "VLOW"),
                    ("rout", "ROUT"),
                    ("rclamp", "RCLAMP"),
                    ("cout", "COUT")));

            definitions["COUNTER"] = new LTspiceADeviceDefinition(
                ADeviceLibraryKind.Digital,
                "DIG_COUNTER",
                Terminals(
                    ADeviceTerminal.T1,
                    ADeviceTerminal.T2,
                    ADeviceTerminal.T7,
                    ADeviceTerminal.T6),
                Terminals(ADeviceTerminal.T6, ADeviceTerminal.T7),
                Parameters(
                    ("vhigh", null),
                    ("vlow", null),
                    ("ref", null),
                    ("cycles", "CYCLES"),
                    ("duty", "DUTY"),
                    ("rout", "ROUT")),
                createsDigitalRails: true,
                appliesDigitalCompatibilityDefaults: true);

            definitions["SAMPLEHOLD"] = new LTspiceADeviceDefinition(
                ADeviceLibraryKind.Analog,
                "ANALOG_SAMPLE_HOLD",
                Terminals(
                    ADeviceTerminal.T1,
                    ADeviceTerminal.T2,
                    ADeviceTerminal.T3,
                    ADeviceTerminal.T4,
                    ADeviceTerminal.T7,
                    ADeviceTerminal.T8Common),
                Terminals(ADeviceTerminal.T7),
                Parameters(
                    ("ref", "REF"),
                    ("vhigh", "VHIGH"),
                    ("vlow", "VLOW"),
                    ("td", "TPD"),
                    ("rout", "ROUT")));

            definitions["OTA"] = new LTspiceADeviceDefinition(
                ADeviceLibraryKind.Analog,
                "ANALOG_OTA",
                Terminals(
                    ADeviceTerminal.T1,
                    ADeviceTerminal.T2,
                    ADeviceTerminal.T3,
                    ADeviceTerminal.T4,
                    ADeviceTerminal.T6,
                    ADeviceTerminal.T7,
                    ADeviceTerminal.T8Common),
                Terminals(ADeviceTerminal.T6, ADeviceTerminal.T7),
                Parameters(
                    ("g", "G"),
                    ("ref", "REF"),
                    ("iout", "IOUT"),
                    ("isrc", "ISRC"),
                    ("isink", "ISINK"),
                    ("ioffset", "IOFFSET"),
                    ("powerup", "POWERUP"),
                    ("asym", "ASYM"),
                    ("linear", "LINEAR"),
                    ("rout", "ROUT"),
                    ("vhigh", "VHIGH"),
                    ("vlow", "VLOW"),
                    ("rclamp", "RCLAMP"),
                    ("cout", "COUT")),
                parameterAdjustment: ADeviceParameterAdjustment.OtaCurrentLimits);

            definitions["VARISTOR"] = new LTspiceADeviceDefinition(
                ADeviceLibraryKind.Analog,
                "ANALOG_VARISTOR",
                Terminals(
                    ADeviceTerminal.T1,
                    ADeviceTerminal.T2,
                    ADeviceTerminal.T7,
                    ADeviceTerminal.T8Common),
                Terminals(ADeviceTerminal.T7),
                Parameters(
                    ("rclamp", "RCLAMP"),
                    ("roff", "ROFF")));

            var modulator = new LTspiceADeviceDefinition(
                ADeviceLibraryKind.Analog,
                "ANALOG_MODULATOR",
                Terminals(
                    ADeviceTerminal.T1,
                    ADeviceTerminal.T2,
                    ADeviceTerminal.T7,
                    ADeviceTerminal.T8Common),
                Terminals(ADeviceTerminal.T7),
                Parameters(
                    ("mark", "MARK"),
                    ("space", "SPACE"),
                    ("rout", "ROUT")),
                parameterAdjustment: ADeviceParameterAdjustment.DefaultUnusedModulatorAmplitude);
            definitions["MODULATE"] = modulator;
            definitions["MODULATOR"] = modulator;

            return definitions;
        }

        private static ADeviceTerminal[] Terminals(params ADeviceTerminal[] terminals)
        {
            return terminals;
        }

        private static IReadOnlyDictionary<string, string> Parameters(
            params (string NativeName, string PortableName)[] items)
        {
            return items.ToDictionary(
                item => item.NativeName,
                item => item.PortableName,
                StringComparer.OrdinalIgnoreCase);
        }
    }
}
