using SpiceSharp;
using SpiceSharp.Entities;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.CustomComponents.Digital
{
    public sealed partial class DigitalSubcircuitLibrary
    {
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
        /// Adds an active-high transparent D latch with active-high asynchronous
        /// PRE and CLR inputs.
        /// </summary>
        public IReadOnlyList<IEntity> AddDLatch(
            Circuit circuit,
            string instanceName,
            string dataNode,
            string enableNode,
            string presetNode,
            string clearNode,
            string outputNode,
            string invertedOutputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalSequentialParameters parameters = null)
        {
            IReadOnlyDictionary<string, string> overrides =
                parameters?.ToOverrides(1, supportsAsynchronousPriority: true, supportsDisabledOutput: false);
            return Library.AddInstance(
                circuit,
                "DIG_D_LATCH",
                instanceName,
                new[]
                {
                    dataNode,
                    enableNode,
                    presetNode,
                    clearNode,
                    outputNode,
                    invertedOutputNode,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                overrides);
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
        /// Adds a positive-edge D flip-flop using validated sequential parameters.
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
            DigitalSequentialParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            IReadOnlyDictionary<string, string> overrides =
                parameters.ToOverrides(1, supportsAsynchronousPriority: true, supportsDisabledOutput: false);
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
                overrides);
        }

        /// <summary>
        /// Adds a positive-edge T flip-flop. A high T input toggles the state;
        /// a low T input holds it. PRE and CLR are active high.
        /// </summary>
        public IReadOnlyList<IEntity> AddToggleFlipFlop(
            Circuit circuit,
            string instanceName,
            string toggleNode,
            string clockNode,
            string presetNode,
            string clearNode,
            string outputNode,
            string invertedOutputNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalSequentialParameters parameters = null)
        {
            IReadOnlyDictionary<string, string> overrides =
                parameters?.ToOverrides(1, supportsAsynchronousPriority: true, supportsDisabledOutput: false);
            return Library.AddInstance(
                circuit,
                "DIG_TFF",
                instanceName,
                new[]
                {
                    toggleNode,
                    clockNode,
                    presetNode,
                    clearNode,
                    outputNode,
                    invertedOutputNode,
                    positiveSupplyNode,
                    negativeSupplyNode,
                },
                overrides);
        }

        /// <summary>
        /// Adds a four-bit positive-edge register with active-high asynchronous
        /// clear and active-high output enable. Index zero is the least-significant bit.
        /// </summary>
        public IReadOnlyList<IEntity> AddRegister4(
            Circuit circuit,
            string instanceName,
            IEnumerable<string> dataNodes,
            string clockNode,
            string clearNode,
            string outputEnableNode,
            IEnumerable<string> outputNodes,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalSequentialParameters parameters = null)
        {
            List<string> data = MaterializeFourNodes(dataNodes, nameof(dataNodes));
            List<string> outputs = MaterializeFourNodes(outputNodes, nameof(outputNodes));
            IReadOnlyDictionary<string, string> overrides =
                parameters?.ToOverrides(15, supportsAsynchronousPriority: false, supportsDisabledOutput: true);

            var nodes = new List<string>(13);
            nodes.AddRange(data);
            nodes.Add(clockNode);
            nodes.Add(clearNode);
            nodes.Add(outputEnableNode);
            nodes.AddRange(outputs);
            nodes.Add(positiveSupplyNode);
            nodes.Add(negativeSupplyNode);

            return Library.AddInstance(
                circuit,
                "DIG_REG4",
                instanceName,
                nodes,
                overrides);
        }

        /// <summary>
        /// Adds a four-bit synchronous up counter with active-high enable and
        /// asynchronous clear. Index zero is the least-significant bit.
        /// </summary>
        public IReadOnlyList<IEntity> AddCounter4(
            Circuit circuit,
            string instanceName,
            string clockNode,
            string enableNode,
            string clearNode,
            IEnumerable<string> outputNodes,
            string carryNode,
            string positiveSupplyNode,
            string negativeSupplyNode,
            DigitalSequentialParameters parameters = null)
        {
            List<string> outputs = MaterializeFourNodes(outputNodes, nameof(outputNodes));
            IReadOnlyDictionary<string, string> overrides =
                parameters?.ToOverrides(15, supportsAsynchronousPriority: false, supportsDisabledOutput: false);

            var nodes = new List<string>(10)
            {
                clockNode,
                enableNode,
                clearNode,
            };
            nodes.AddRange(outputs);
            nodes.Add(carryNode);
            nodes.Add(positiveSupplyNode);
            nodes.Add(negativeSupplyNode);

            return Library.AddInstance(
                circuit,
                "DIG_COUNTER4_UP",
                instanceName,
                nodes,
                overrides);
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


    }
}
