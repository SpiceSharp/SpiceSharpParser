using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
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
        /// <param name="nodeNameGenerator"></param>
        /// <param name="componentNameGenerator"></param>
        /// <param name="modelNameGenerator"></param>
        /// <param name="result"></param>
        /// <param name="caseSettings"></param>
        /// <returns>
        /// A new export.
        /// </returns>
        public override Export CreateExport(string name, string type, ParameterCollection parameters, Simulation simulation, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator componentNameGenerator, IObjectNameGenerator modelNameGenerator, IResultService result, SpiceNetlistCaseSensitivitySettings caseSettings)
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

            var comparer = StringComparerProvider.Get(caseSettings.IsEntityParameterNameCaseSensitive);

            var entityName = (parameters[0] as VectorParameter)?.Elements[0].Image;
            var propertyName = (parameters[0] as VectorParameter)?.Elements[1].Image;

            if (result != null && entityName != null && propertyName != null)
            {
                string modelName = modelNameGenerator.Generate(entityName);

                if (result.FindObject(entityName, out var entity) && entity is SpiceSharp.Components.Model)
                {
                    if (simulation != null)
                    {
                        return new PropertyExport(name, simulation, entityName, propertyName, comparer);
                    }
                    else
                    {
                        return new ZeroExport();
                    }
                }
                else if (result.FindObject(modelName, out var model) && model is SpiceSharp.Components.Model)
                {
                    if (simulation != null)
                    {
                        return new PropertyExport(name, simulation, modelName, propertyName, comparer);
                    }
                    else
                    {
                        return new ZeroExport();
                    }
                }
                else
                {
                    string componentName = componentNameGenerator.Generate(entityName);
                    if (result.FindObject(componentName, out var component) && component is SpiceSharp.Components.Component)
                    {
                        if (simulation != null)
                        {
                            return new PropertyExport(name, simulation, componentName, propertyName, comparer);
                        }
                        else
                        {
                            return new ZeroExport();
                        }
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
                                    string modelName2 = modelNameGenerator.Generate(entityName) + "_" + simulation.Name;
                                    return new PropertyExport(name, simulation, modelName2, propertyName,
                                        comparer);
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
