using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using SpiceSharp;

namespace SpiceSharpParser.CustomComponents.Digital
{
    /// <summary>
    /// Provides reusable, parameterized digital and mixed-signal models backed by
    /// <see cref="SpiceSubcircuitLibrary"/>.
    /// </summary>
    public sealed partial class DigitalSubcircuitLibrary
    {
        private const string EmbeddedResourceName =
            "SpiceSharpParser.CustomComponents.Digital.standard-digital.lib";
        private const string ADeviceZeroDelayResourceName =
            "SpiceSharpParser.CustomComponents.Digital.ltspice-a-device-zero-delay.lib";

        private static readonly IReadOnlyDictionary<DigitalGateKind, string> SubcircuitNames =
            new ReadOnlyDictionary<DigitalGateKind, string>(
                new Dictionary<DigitalGateKind, string>
                {
                    [DigitalGateKind.Buffer] = "DIG_BUF",
                    [DigitalGateKind.Inverter] = "DIG_NOT",
                    [DigitalGateKind.And2] = "DIG_AND2",
                    [DigitalGateKind.Nand2] = "DIG_NAND2",
                    [DigitalGateKind.Or2] = "DIG_OR2",
                    [DigitalGateKind.Nor2] = "DIG_NOR2",
                    [DigitalGateKind.Xor2] = "DIG_XOR2",
                    [DigitalGateKind.Xnor2] = "DIG_XNOR2",
                });

        private DigitalSubcircuitLibrary(SpiceSubcircuitLibrary library)
        {
            Library = library ?? throw new ArgumentNullException(nameof(library));
        }

        /// <summary>
        /// Gets the underlying general-purpose SPICE subcircuit library.
        /// </summary>
        public SpiceSubcircuitLibrary Library { get; }

        /// <summary>
        /// Loads the digital models embedded in the custom-components assembly.
        /// </summary>
        /// <param name="options">Compilation options, or null for defaults.</param>
        /// <returns>A reusable digital subcircuit library.</returns>
        public static DigitalSubcircuitLibrary LoadBuiltIn(SpiceCompileOptions options = null)
        {
            return LoadEmbedded(EmbeddedResourceName, options);
        }

        internal static DigitalSubcircuitLibrary LoadADeviceZeroDelay(SpiceCompileOptions options = null)
        {
            return LoadEmbedded(ADeviceZeroDelayResourceName, options);
        }

        private static DigitalSubcircuitLibrary LoadEmbedded(
            string resourceName,
            SpiceCompileOptions options)
        {
            Assembly assembly = typeof(DigitalSubcircuitLibrary).GetTypeInfo().Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException(
                        $"The embedded digital subcircuit resource '{resourceName}' was not found.");
                }

                using (var reader = new StreamReader(stream))
                {
                    return new DigitalSubcircuitLibrary(
                        SpiceSubcircuitLibrary.LoadText(
                            reader.ReadToEnd(),
                            resourceName,
                            options));
                }
            }
        }

        private static string GetSubcircuitName(DigitalGateKind kind)
        {
            if (!SubcircuitNames.TryGetValue(kind, out string result))
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown digital gate type.");
            }

            return result;
        }

        private static bool IsUnary(DigitalGateKind kind)
        {
            return kind == DigitalGateKind.Buffer || kind == DigitalGateKind.Inverter;
        }

        private static List<string> MaterializeFourNodes(
            IEnumerable<string> nodes,
            string parameterName)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            var result = new List<string>(nodes);
            if (result.Count != 4)
            {
                throw new ArgumentException(
                    $"Exactly four nodes are required, but {result.Count} were supplied.",
                    parameterName);
            }

            return result;
        }
    }
}
