using SpiceSharp;
using SpiceSharp.Entities;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.CustomComponents.Digital
{
    public sealed partial class DigitalSubcircuitLibrary
    {
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

    }
}
