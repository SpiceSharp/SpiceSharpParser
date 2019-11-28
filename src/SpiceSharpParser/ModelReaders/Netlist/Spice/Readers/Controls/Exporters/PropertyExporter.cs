using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;

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
        /// <param name="context">Expression context.</param>
        /// <param name="caseSettings"></param>
        /// <returns>
        /// A new export.
        /// </returns>
        public override Export CreateExport(string name, string type, ParameterCollection parameters, EvaluationContext context, ISpiceNetlistCaseSensitivitySettings caseSettings)
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

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var comparer = StringComparerProvider.Get(caseSettings.IsEntityParameterNameCaseSensitive);

            var entityName = (parameters[0] as VectorParameter)?.Elements[0].Image;
            var propertyName = (parameters[0] as VectorParameter)?.Elements[1].Image;

            if (entityName != null && propertyName != null)
            {
                if (entityName.Contains("#"))
                {
                    string objectName = $"{context.NameGenerator.GenerateObjectName(entityName)}_{context.Simulation.Name}";
                    return new PropertyExport(name, context.Simulation, objectName, propertyName, comparer);
                }
                else
                {
                    string objectName = context.NameGenerator.GenerateObjectName(entityName);

                    if (context.ResultService.FindObject(objectName, out _))
                    {
                        return new PropertyExport(name, context.Simulation, objectName, propertyName, comparer);
                    }
                    else
                    {
                        return new PropertyExport(name, context.Simulation, entityName, propertyName, comparer);
                    }
                }
            }

            throw new ReadingException("Invalid property export");
        }
    }
}