using SpiceSharp;
using SpiceSharp.Entities;
using System.Collections.Generic;

namespace SpiceSharpParser.CustomComponents.Digital
{
    public sealed partial class DigitalSubcircuitLibrary
    {
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

    }
}
