using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    /// <summary>
    /// Current sources generator.
    /// </summary>
    public class CurrentSourceGenerator : SourceGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSourceGenerator"/> class.
        /// </summary>
        public CurrentSourceGenerator()
        {
        }

        /// <summary>
        /// Gets generated types.
        /// </summary>
        /// <returns>
        /// Generated types.
        /// </returns>
        public override IEnumerable<string> GeneratedTypes => new List<string>() { "I", "G", "F" };

        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type.ToLower())
            {
                case "i": return GenerateCurrentSource(componentIdentifier, parameters, context);
                case "g": return GenerateVoltageControlledCurrentSource(componentIdentifier, parameters, context);
                case "f": return GenerateCurrentControlledCurrentSource(componentIdentifier, parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Generates a new current controlled current source: FName
        /// </summary>
        /// <param name="name">Name of generated current controlled current source</param>
        /// <param name="parameters">Parameters for current source</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of current controlled current source
        /// </returns>
        protected SpiceSharp.Components.Component GenerateCurrentControlledCurrentSource(string name,  ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count == 4
                && parameters.IsValueString(0)
                && parameters.IsValueString(1)
                && parameters.IsValueString(2) && parameters[2].Image.ToLower() != "value"
                && parameters.IsValueString(3))
            {
                var cccs = new CurrentControlledCurrentSource(name);
                context.CreateNodes(cccs, parameters);
                cccs.ControllingName = context.ComponentNameGenerator.Generate(parameters.GetString(2));
                context.SetParameter(cccs, "gain", parameters.GetString(3));

                return cccs;
            }
            else
            {
                var cs = new CurrentSource(name);
                context.CreateNodes(cs, parameters);
                SetControlledSourceParameters(name, parameters.Skip(CurrentSource.CurrentSourcePinCount), context, cs, false);
                return cs;
            }
        }

        /// <summary>
        /// Generates a new voltage controlled current source: GName
        /// </summary>
        /// <param name="name">Name of generated voltage controlled current source</param>
        /// <param name="parameters">Parameters for current source</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of voltage controlled current source
        /// </returns>
        protected SpiceSharp.Components.Component GenerateVoltageControlledCurrentSource(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count == 5
                && parameters.IsValueString(0)
                && parameters.IsValueString(1)
                && parameters.IsValueString(2)
                && parameters.IsValueString(3)
                && parameters.IsValueString(4))
            {
                var vccs = new VoltageControlledCurrentSource(name);
                context.CreateNodes(vccs, parameters);
                context.SetParameter(vccs, "gain", parameters.GetValueString(4));
                return vccs;
            }
            else
            {
                if (parameters.Count == 3
                    && parameters[0] is PointParameter pp1 && pp1.Values.Count() == 2
                    && parameters[1] is PointParameter pp2 && pp2.Values.Count() == 2)
                {
                    var vccsNodes = new ParameterCollection();
                    vccsNodes.Add(pp1.Values.Items[0]);
                    vccsNodes.Add(pp1.Values.Items[1]);
                    vccsNodes.Add(pp2.Values.Items[0]);
                    vccsNodes.Add(pp2.Values.Items[1]);

                    var vccs = new VoltageControlledCurrentSource(name);
                    context.CreateNodes(vccs, vccsNodes);
                    context.SetParameter(vccs, "gain", parameters.GetString(2));
                    return vccs;
                }
                else
                {
                    var cs = new CurrentSource(name);
                    context.CreateNodes(cs, parameters);
                    SetControlledSourceParameters(name, parameters.Skip(CurrentSource.CurrentSourcePinCount), context, cs, true);
                    return cs;
                }
            }
        }

        /// <summary>
        /// Generates a new current source.
        /// </summary>
        /// <param name="name">Name of generated current source.</param>
        /// <param name="parameters">Parameters for current source.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of current source.
        /// </returns>
        protected SpiceSharp.Components.Component GenerateCurrentSource(string name,  ParameterCollection parameters, IReadingContext context)
        {
            CurrentSource cs = new CurrentSource(name);
            context.CreateNodes(cs, parameters);
            SetSourceParameters(name, parameters.Skip(CurrentSource.CurrentSourcePinCount), context, cs);
            return cs;
        }
    }
}
