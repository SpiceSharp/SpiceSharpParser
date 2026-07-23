using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SpiceSharp;
using SpiceSharp.Entities;

namespace SpiceSharpParser.CustomComponents.Analog
{
    /// <summary>
    /// Provides reusable analog and mixed-signal models backed by
    /// <see cref="SpiceSubcircuitLibrary"/>.
    /// </summary>
    public sealed class AnalogSubcircuitLibrary
    {
        private const string EmbeddedResourceName =
            "SpiceSharpParser.CustomComponents.Analog.standard-analog.lib";

        private AnalogSubcircuitLibrary(SpiceSubcircuitLibrary library)
        {
            Library = library ?? throw new ArgumentNullException(nameof(library));
        }

        /// <summary>
        /// Gets the underlying general-purpose SPICE subcircuit library.
        /// </summary>
        public SpiceSubcircuitLibrary Library { get; }

        /// <summary>
        /// Loads the analog models embedded in the custom-components assembly.
        /// </summary>
        /// <param name="options">Compilation options, or null for defaults.</param>
        /// <returns>A reusable analog subcircuit library.</returns>
        public static AnalogSubcircuitLibrary LoadBuiltIn(SpiceCompileOptions options = null)
        {
            Assembly assembly = typeof(AnalogSubcircuitLibrary).GetTypeInfo().Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(EmbeddedResourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException(
                        $"The embedded analog subcircuit resource '{EmbeddedResourceName}' was not found.");
                }

                using (var reader = new StreamReader(stream))
                {
                    return new AnalogSubcircuitLibrary(
                        SpiceSubcircuitLibrary.LoadText(
                            reader.ReadToEnd(),
                            EmbeddedResourceName,
                            options));
                }
            }
        }

        /// <summary>
        /// Adds an analog sample/hold. SH high selects track mode; otherwise a rising
        /// CLK edge samples the differential input.
        /// </summary>
        public IReadOnlyList<IEntity> AddSampleHold(
            Circuit circuit,
            string instanceName,
            string positiveInputNode,
            string negativeInputNode,
            string clockNode,
            string sampleHoldNode,
            string outputNode,
            string commonNode,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            return Library.AddInstance(
                circuit,
                "ANALOG_SAMPLE_HOLD",
                instanceName,
                new[]
                {
                    positiveInputNode,
                    negativeInputNode,
                    clockNode,
                    sampleHoldNode,
                    outputNode,
                    commonNode,
                },
                parameters);
        }

        /// <summary>
        /// Adds a four-quadrant operational transconductance amplifier with two
        /// differential input pairs and a current output.
        /// </summary>
        public IReadOnlyList<IEntity> AddOperationalTransconductanceAmplifier(
            Circuit circuit,
            string instanceName,
            string firstNegativeInputNode,
            string firstPositiveInputNode,
            string secondPositiveInputNode,
            string secondNegativeInputNode,
            string railNode,
            string outputNode,
            string commonNode,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            return Library.AddInstance(
                circuit,
                "ANALOG_OTA",
                instanceName,
                new[]
                {
                    firstNegativeInputNode,
                    firstPositiveInputNode,
                    secondPositiveInputNode,
                    secondNegativeInputNode,
                    railNode,
                    outputNode,
                    commonNode,
                },
                parameters);
        }

        /// <summary>
        /// Adds a voltage-controlled bidirectional varistor.
        /// </summary>
        public IReadOnlyList<IEntity> AddVoltageControlledVaristor(
            Circuit circuit,
            string instanceName,
            string positiveControlNode,
            string negativeControlNode,
            string outputNode,
            string commonNode,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            return Library.AddInstance(
                circuit,
                "ANALOG_VARISTOR",
                instanceName,
                new[] { positiveControlNode, negativeControlNode, outputNode, commonNode },
                parameters);
        }

        /// <summary>
        /// Adds an LTspice-style voltage-controlled oscillator/modulator. MARK is the
        /// frequency at FM=1 V, SPACE is the frequency at FM=0 V, and AM sets amplitude.
        /// </summary>
        public IReadOnlyList<IEntity> AddModulator(
            Circuit circuit,
            string instanceName,
            string frequencyModulationNode,
            string amplitudeModulationNode,
            string outputNode,
            string commonNode,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            return Library.AddInstance(
                circuit,
                "ANALOG_MODULATOR",
                instanceName,
                new[]
                {
                    frequencyModulationNode,
                    amplitudeModulationNode,
                    outputNode,
                    commonNode,
                },
                parameters);
        }
    }
}
