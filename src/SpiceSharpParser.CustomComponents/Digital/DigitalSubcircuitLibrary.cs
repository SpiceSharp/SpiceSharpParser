using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using SpiceSharp;
using SpiceSharp.Entities;

namespace SpiceSharpParser.CustomComponents.Digital
{
    /// <summary>
    /// Provides reusable, parameterized digital and mixed-signal models backed by
    /// <see cref="SpiceSubcircuitLibrary"/>.
    /// </summary>
    public sealed class DigitalSubcircuitLibrary
    {
        private const string EmbeddedResourceName =
            "SpiceSharpParser.CustomComponents.Digital.standard-digital.lib";

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
            Assembly assembly = typeof(DigitalSubcircuitLibrary).GetTypeInfo().Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(EmbeddedResourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException(
                        $"The embedded digital subcircuit resource '{EmbeddedResourceName}' was not found.");
                }

                using (var reader = new StreamReader(stream))
                {
                    return new DigitalSubcircuitLibrary(
                        SpiceSubcircuitLibrary.LoadText(
                            reader.ReadToEnd(),
                            EmbeddedResourceName,
                            options));
                }
            }
        }

        /// <summary>
        /// Adds a built-in gate instance using its ordered input nodes followed by
        /// output, VDD and VSS.
        /// </summary>
        /// <param name="circuit">The target SpiceSharp circuit.</param>
        /// <param name="kind">The digital gate type.</param>
        /// <param name="instanceName">A unique instance name.</param>
        /// <param name="inputNodes">One input for buffer/inverter; two for other gates.</param>
        /// <param name="outputNode">The output node.</param>
        /// <param name="positiveSupplyNode">The VDD node.</param>
        /// <param name="negativeSupplyNode">The VSS node.</param>
        /// <param name="parameters">Optional electrical parameter overrides.</param>
        /// <returns>The entities added to the circuit.</returns>
        public IReadOnlyList<IEntity> AddGate(
            Circuit circuit,
            DigitalGateKind kind,
            string instanceName,
            IEnumerable<string> inputNodes,
            string outputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalGateParameters parameters = null)
        {
            if (inputNodes == null)
            {
                throw new ArgumentNullException(nameof(inputNodes));
            }

            var nodes = new List<string>(inputNodes);
            int expectedInputCount = IsUnary(kind) ? 1 : 2;
            if (nodes.Count != expectedInputCount)
            {
                throw new ArgumentException(
                    $"Gate '{kind}' requires {expectedInputCount} input node(s), but {nodes.Count} were supplied.",
                    nameof(inputNodes));
            }

            nodes.Add(outputNode);
            nodes.Add(positiveSupplyNode);
            nodes.Add(negativeSupplyNode);

            IReadOnlyDictionary<string, string> overrides = parameters?.ToOverrides();
            return Library.AddInstance(
                circuit,
                GetSubcircuitName(kind),
                instanceName,
                nodes,
                overrides);
        }

        /// <summary>
        /// Adds a buffer instance.
        /// </summary>
        public IReadOnlyList<IEntity> AddBuffer(
            Circuit circuit,
            string instanceName,
            string inputNode,
            string outputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalGateParameters parameters = null)
        {
            return AddGate(
                circuit,
                DigitalGateKind.Buffer,
                instanceName,
                new[] { inputNode },
                outputNode,
                positiveSupplyNode,
                negativeSupplyNode,
                parameters);
        }

        /// <summary>
        /// Adds an inverter instance.
        /// </summary>
        public IReadOnlyList<IEntity> AddInverter(
            Circuit circuit,
            string instanceName,
            string inputNode,
            string outputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalGateParameters parameters = null)
        {
            return AddGate(
                circuit,
                DigitalGateKind.Inverter,
                instanceName,
                new[] { inputNode },
                outputNode,
                positiveSupplyNode,
                negativeSupplyNode,
                parameters);
        }

        /// <summary>
        /// Adds a non-inverting Schmitt trigger with separate rising and falling thresholds.
        /// </summary>
        public IReadOnlyList<IEntity> AddSchmittBuffer(
            Circuit circuit,
            string instanceName,
            string inputNode,
            string outputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalSchmittParameters parameters = null)
        {
            IReadOnlyDictionary<string, string> overrides = parameters?.ToOverrides();
            return Library.AddInstance(
                circuit,
                "DIG_SCHMITT_BUF",
                instanceName,
                new[] { inputNode, outputNode, positiveSupplyNode, negativeSupplyNode },
                overrides);
        }

        /// <summary>
        /// Adds an inverting Schmitt trigger with separate rising and falling thresholds.
        /// </summary>
        public IReadOnlyList<IEntity> AddSchmittInverter(
            Circuit circuit,
            string instanceName,
            string inputNode,
            string outputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalSchmittParameters parameters = null)
        {
            IReadOnlyDictionary<string, string> overrides = parameters?.ToOverrides();
            return Library.AddInstance(
                circuit,
                "DIG_SCHMITT_NOT",
                instanceName,
                new[] { inputNode, outputNode, positiveSupplyNode, negativeSupplyNode },
                overrides);
        }

        /// <summary>
        /// Adds a non-inverting tri-state driver with an active-high output enable.
        /// </summary>
        public IReadOnlyList<IEntity> AddTriStateBuffer(
            Circuit circuit,
            string instanceName,
            string inputNode,
            string outputEnableNode,
            string outputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalTriStateParameters parameters = null)
        {
            IReadOnlyDictionary<string, string> overrides = parameters?.ToOverrides();
            return Library.AddInstance(
                circuit,
                "DIG_TRI_BUF",
                instanceName,
                new[]
                {
                    inputNode,
                    outputEnableNode,
                    outputNode,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                overrides);
        }

        /// <summary>
        /// Adds an inverting tri-state driver with an active-high output enable.
        /// </summary>
        public IReadOnlyList<IEntity> AddTriStateInverter(
            Circuit circuit,
            string instanceName,
            string inputNode,
            string outputEnableNode,
            string outputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalTriStateParameters parameters = null)
        {
            IReadOnlyDictionary<string, string> overrides = parameters?.ToOverrides();
            return Library.AddInstance(
                circuit,
                "DIG_TRI_NOT",
                instanceName,
                new[]
                {
                    inputNode,
                    outputEnableNode,
                    outputNode,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                overrides);
        }

        /// <summary>
        /// Adds a two-input gate instance.
        /// </summary>
        public IReadOnlyList<IEntity> AddBinaryGate(
            Circuit circuit,
            DigitalGateKind kind,
            string instanceName,
            string firstInputNode,
            string secondInputNode,
            string outputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalGateParameters parameters = null)
        {
            if (IsUnary(kind))
            {
                throw new ArgumentException(
                    $"Gate '{kind}' is unary; use AddGate, AddBuffer or AddInverter.",
                    nameof(kind));
            }

            return AddGate(
                circuit,
                kind,
                instanceName,
                new[] { firstInputNode, secondInputNode },
                outputNode,
                positiveSupplyNode,
                negativeSupplyNode,
                parameters);
        }

        /// <summary>
        /// Adds a 2:1 multiplexer. A low select chooses D0 and a high select chooses D1.
        /// </summary>
        public IReadOnlyList<IEntity> AddMultiplexer2(
            Circuit circuit,
            string instanceName,
            string data0Node,
            string data1Node,
            string selectNode,
            string outputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalGateParameters parameters = null)
        {
            IReadOnlyDictionary<string, string> overrides = parameters?.ToOverrides();
            return Library.AddInstance(
                circuit,
                "DIG_MUX2",
                instanceName,
                new[]
                {
                    data0Node,
                    data1Node,
                    selectNode,
                    outputNode,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                overrides);
        }

        /// <summary>
        /// Adds a 4:1 multiplexer with S0 as the least-significant select bit.
        /// </summary>
        public IReadOnlyList<IEntity> AddMultiplexer4(
            Circuit circuit,
            string instanceName,
            string data0Node,
            string data1Node,
            string data2Node,
            string data3Node,
            string select0Node,
            string select1Node,
            string outputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalGateParameters parameters = null)
        {
            IReadOnlyDictionary<string, string> overrides = parameters?.ToOverrides();
            return Library.AddInstance(
                circuit,
                "DIG_MUX4",
                instanceName,
                new[]
                {
                    data0Node,
                    data1Node,
                    data2Node,
                    data3Node,
                    select0Node,
                    select1Node,
                    outputNode,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                overrides);
        }

        /// <summary>
        /// Adds a one-bit full adder.
        /// </summary>
        public IReadOnlyList<IEntity> AddFullAdder(
            Circuit circuit,
            string instanceName,
            string firstInputNode,
            string secondInputNode,
            string carryInputNode,
            string sumOutputNode,
            string carryOutputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalGateParameters parameters = null)
        {
            IReadOnlyDictionary<string, string> overrides = parameters?.ToOverrides();
            return Library.AddInstance(
                circuit,
                "DIG_FULL_ADDER",
                instanceName,
                new[]
                {
                    firstInputNode,
                    secondInputNode,
                    carryInputNode,
                    sumOutputNode,
                    carryOutputNode,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                overrides);
        }

        /// <summary>
        /// Adds an active-high 2-to-4 decoder with A as the least-significant address bit.
        /// </summary>
        public IReadOnlyList<IEntity> AddDecoder2To4(
            Circuit circuit,
            string instanceName,
            string address0Node,
            string address1Node,
            string enableNode,
            string output0Node,
            string output1Node,
            string output2Node,
            string output3Node,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalGateParameters parameters = null)
        {
            IReadOnlyDictionary<string, string> overrides = parameters?.ToOverrides();
            return Library.AddInstance(
                circuit,
                "DIG_DEC2TO4",
                instanceName,
                new[]
                {
                    address0Node,
                    address1Node,
                    enableNode,
                    output0Node,
                    output1Node,
                    output2Node,
                    output3Node,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                overrides);
        }

        /// <summary>
        /// Adds a differential comparator whose output is high when the positive
        /// input exceeds the negative input plus the optional VOFF parameter.
        /// </summary>
        public IReadOnlyList<IEntity> AddComparator(
            Circuit circuit,
            string instanceName,
            string positiveInputNode,
            string negativeInputNode,
            string outputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            return Library.AddInstance(
                circuit,
                "DIG_COMP",
                instanceName,
                new[]
                {
                    positiveInputNode,
                    negativeInputNode,
                    outputNode,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                parameters);
        }

        /// <summary>
        /// Adds an active-high, reset-dominant set-reset latch.
        /// </summary>
        public IReadOnlyList<IEntity> AddSetResetLatch(
            Circuit circuit,
            string instanceName,
            string setNode,
            string resetNode,
            string outputNode,
            string invertedOutputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            return Library.AddInstance(
                circuit,
                "DIG_SR_LATCH",
                instanceName,
                new[]
                {
                    setNode,
                    resetNode,
                    outputNode,
                    invertedOutputNode,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                parameters);
        }

        /// <summary>
        /// Adds an LTspice-compatible, reset-dominant set-reset flip-flop.
        /// This is an alias for <see cref="AddSetResetLatch"/>.
        /// </summary>
        public IReadOnlyList<IEntity> AddSetResetFlipFlop(
            Circuit circuit,
            string instanceName,
            string setNode,
            string resetNode,
            string outputNode,
            string invertedOutputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            return AddSetResetLatch(
                circuit,
                instanceName,
                setNode,
                resetNode,
                outputNode,
                invertedOutputNode,
                positiveSupplyNode,
                negativeSupplyNode,
                parameters);
        }

        /// <summary>
        /// Adds a positive-edge D flip-flop with active-high asynchronous PRE and CLR.
        /// CLR takes precedence when both asynchronous inputs are high.
        /// </summary>
        public IReadOnlyList<IEntity> AddDFlipFlop(
            Circuit circuit,
            string instanceName,
            string dataNode,
            string clockNode,
            string presetNode,
            string clearNode,
            string outputNode,
            string invertedOutputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            return Library.AddInstance(
                circuit,
                "DIG_DFF",
                instanceName,
                new[]
                {
                    dataNode,
                    clockNode,
                    presetNode,
                    clearNode,
                    outputNode,
                    invertedOutputNode,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                parameters);
        }

        /// <summary>
        /// Adds a type-II phase/frequency detector. A leading edge on A sources IOUT;
        /// a leading edge on B sinks IOUT; a matching edge returns the current to zero.
        /// </summary>
        public IReadOnlyList<IEntity> AddPhaseDetector(
            Circuit circuit,
            string instanceName,
            string firstInputNode,
            string secondInputNode,
            string outputNode,
            string commonNode,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            return Library.AddInstance(
                circuit,
                "DIG_PHASE_DETECTOR",
                instanceName,
                new[] { firstInputNode, secondInputNode, outputNode, commonNode },
                parameters);
        }

        /// <summary>
        /// Adds a rising-edge divide-by-N counter. The main output starts high and
        /// remains high for round(cycles*dutyCycle) input periods per cycle.
        /// </summary>
        public IReadOnlyList<IEntity> AddCounter(
            Circuit circuit,
            string instanceName,
            string clockNode,
            string resetNode,
            string outputNode,
            string invertedOutputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            int cycles,
            double dutyCycle = 0.5,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            if (cycles < 2)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cycles),
                    cycles,
                    "The counter cycle count must be at least two.");
            }

            if (double.IsNaN(dutyCycle)
                || double.IsInfinity(dutyCycle)
                || dutyCycle <= 0.0
                || dutyCycle >= 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dutyCycle),
                    dutyCycle,
                    "The counter duty cycle must be greater than zero and less than one.");
            }

            var overrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (parameters != null)
            {
                foreach (KeyValuePair<string, string> parameter in parameters)
                {
                    overrides[parameter.Key] = parameter.Value;
                }
            }

            overrides["CYCLES"] =
                cycles.ToString(System.Globalization.CultureInfo.InvariantCulture);
            overrides["DUTY"] =
                dutyCycle.ToString("R", System.Globalization.CultureInfo.InvariantCulture);

            return Library.AddInstance(
                circuit,
                "DIG_COUNTER",
                instanceName,
                new[]
                {
                    clockNode,
                    resetNode,
                    outputNode,
                    invertedOutputNode,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                overrides);
        }


        /// <summary>
        /// Adds an active-high open-drain pull-down driver.
        /// </summary>
        public IReadOnlyList<IEntity> AddOpenDrain(
            Circuit circuit,
            string instanceName,
            string inputNode,
            string outputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            return Library.AddInstance(
                circuit,
                "DIG_OPEN_DRAIN",
                instanceName,
                new[]
                {
                    inputNode,
                    outputNode,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                parameters);
        }

        /// <summary>
        /// Adds a functional 555 timer using standard package pin order.
        /// </summary>
        public IReadOnlyList<IEntity> AddTimer555(
            Circuit circuit,
            string instanceName,
            string groundNode,
            string triggerNode,
            string outputNode,
            string resetNode,
            string controlNode,
            string thresholdNode,
            string dischargeNode,
            string positiveSupplyNode,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            return Library.AddInstance(
                circuit,
                "TIMER555",
                instanceName,
                new[]
                {
                    groundNode,
                    triggerNode,
                    outputNode,
                    resetNode,
                    controlNode,
                    thresholdNode,
                    dischargeNode,
                    positiveSupplyNode,
                },
                parameters);
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
    }
}
