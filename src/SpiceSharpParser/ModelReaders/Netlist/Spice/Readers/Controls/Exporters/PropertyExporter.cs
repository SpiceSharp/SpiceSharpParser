using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    /// <summary>
    /// Generates a property <see cref="Export"/>.
    /// </summary>
    public class PropertyExporter : Exporter
    {
        /// <summary>
        /// Creates a new current export.
        /// </summary>
        /// <paramref name="name">Name of export.</paramref>
        /// <param name="name"></param>
        /// <param name="type">A type of export.</param>
        /// <param name="parameters">A parameters of export.</param>
        /// <param name="simulation">A simulation for export.</param>
        /// <param name="nameGenerator">Name generator.</param>
        /// <param name="caseSettings"></param>
        /// <returns>
        /// A new export.
        /// </returns>
        public override Export CreateExport(string name, string type, ParameterCollection parameters, Simulation simulation, INameGenerator nameGenerator,  ISpiceNetlistCaseSensitivitySettings caseSettings)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            var comparer = StringComparerProvider.Get(caseSettings.IsEntityParameterNameCaseSensitive);

            var entityName = (parameters[0] as VectorParameter)?.Elements[0].Image;
            var propertyName = (parameters[0] as VectorParameter)?.Elements[1].Image;

            if (entityName != null && propertyName != null)
            {
                if (entityName.Contains("#"))
                {
                    string objectName = $"{nameGenerator.GenerateObjectName(entityName)}_{simulation.Name}";
                    return new PropertyExport(name, simulation, objectName, propertyName, comparer);
                }
                else
                {
                    string objectName = nameGenerator.GenerateObjectName(entityName);
                    return new PropertyExport(name, simulation, objectName, propertyName, comparer);
                }
            }

            throw new Exception("Invalid property export");
        }
    }
}
