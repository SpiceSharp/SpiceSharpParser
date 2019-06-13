using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    /// <summary>
    /// Generates a property <see cref="Export"/>.
    /// </summary>
    public class PropertyExporter : Exporter
    {
        /// <summary>
        /// Gets supported voltage exports.
        /// </summary>
        /// <returns>
        /// A list of supported voltage exports.
        /// </returns>
        public override ICollection<string> CreatedTypes => new List<string>() { "@" };

        /// <summary>
        /// Creates a new current export.
        /// </summary>
        /// <paramref name="name">Name of export.</paramref>
        /// <param name="type">A type of export.</param>
        /// <param name="parameters">A parameters of export.</param>
        /// <param name="simulation">A simulation for export.</param>
        /// <returns>
        /// A new export.
        /// </returns>
        public override Export CreateExport(string name, string type, ParameterCollection parameters, Simulation simulation, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator componentNameGenerator, IObjectNameGenerator modelNameGenerator, IResultService result, SpiceNetlistCaseSensitivitySettings caseSettings)
        {
            if (parameters.Count != 2)
            {
                throw new WrongParameterException("Property exports should have two parameters: name of component and property name");
            }

            var comparer = StringComparerProvider.Get(caseSettings.IsEntityParameterNameCaseSensitive);

            var entityName = parameters[0].Image;
            if (result != null)
            {
                string modelName = modelNameGenerator.Generate(entityName);
                if (result.FindObject(modelName, out var model) && model is SpiceSharp.Components.Model)
                {
                    return new PropertyExport(name, simulation, modelName, parameters[1].Image, comparer);
                }
                else
                {
                    string componentName = componentNameGenerator.Generate(entityName);
                    if (result.FindObject(componentName, out var component) && component is SpiceSharp.Components.Component)
                    {
                        return new PropertyExport(name, simulation, componentName, parameters[1].Image, comparer);
                    }
                    else
                    {
                        if (entityName.Contains("#"))
                        {
                            string prefix = entityName.Substring(0, entityName.IndexOf("#"));

                            if (result.FindObject(prefix, out var obj2))
                            {
                                if (obj2 is SpiceSharp.Components.Model)
                                {
                                    string modelName2 = modelNameGenerator.Generate(entityName);
                                    return new PropertyExport(name, simulation, modelName2, parameters[1].Image, comparer);
                                }
                            }
                        }
                    }
                }
            }

            throw new Exception("Entity for property export not found");
        }
    }
}
