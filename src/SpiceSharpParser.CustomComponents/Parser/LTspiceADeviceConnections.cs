using System;
using System.Collections.Generic;
using System.Globalization;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Resolves native A-device terminal positions into portable subcircuit nodes.
    /// </summary>
    internal sealed class LTspiceADeviceConnections
    {
        private readonly bool[] _unusedTerminals;
        private readonly string[] _terminals;

        private LTspiceADeviceConnections(
            string[] terminals,
            bool[] unusedTerminals)
        {
            _terminals = terminals;
            _unusedTerminals = unusedTerminals;
        }

        public static LTspiceADeviceConnections Create(
            LTspiceADeviceInstance instance,
            LTspiceADeviceDefinition definition,
            string portableInstanceName,
            bool nodeNamesAreCaseSensitive)
        {
            var terminals = (string[])instance.Terminals.Clone();
            var unused = new bool[LTspiceADeviceInstance.TerminalCount];
            StringComparison comparison = nodeNamesAreCaseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            string commonNode = terminals[(int)ADeviceTerminal.T8Common];

            for (int index = 0; index < terminals.Length; index++)
            {
                unused[index] = terminals[index].Equals(commonNode, comparison);
            }

            // Repeating terminal 8 means "not connected" in native A-device
            // syntax. Inputs may safely remain tied to common. Outputs cannot:
            // the portable macromodel has a real output stage, so give each
            // unused output a private node.
            foreach (ADeviceTerminal outputTerminal in definition.OutputTerminals)
            {
                int index = (int)outputTerminal;
                if (unused[index])
                {
                    terminals[index] = portableInstanceName
                        + ".__a_nc"
                        + (index + 1).ToString(CultureInfo.InvariantCulture);
                }
            }

            return new LTspiceADeviceConnections(terminals, unused);
        }

        public string CommonNode => Get(ADeviceTerminal.T8Common);

        public bool IsUnused(ADeviceTerminal terminal)
        {
            return _unusedTerminals[(int)terminal];
        }

        public IReadOnlyList<string> CreateSubcircuitNodes(
            LTspiceADeviceDefinition definition,
            string highRailNode,
            string lowRailNode)
        {
            int railCount = definition.CreatesDigitalRails ? 2 : 0;
            var result = new List<string>(
                definition.SubcircuitTerminals.Count + railCount);
            foreach (ADeviceTerminal terminal in definition.SubcircuitTerminals)
            {
                result.Add(Get(terminal));
            }

            if (definition.CreatesDigitalRails)
            {
                result.Add(highRailNode);
                result.Add(lowRailNode);
            }

            return result;
        }

        private string Get(ADeviceTerminal terminal)
        {
            return _terminals[(int)terminal];
        }
    }
}
