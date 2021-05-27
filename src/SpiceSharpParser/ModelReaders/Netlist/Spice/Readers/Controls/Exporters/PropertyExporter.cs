using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;

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
        /// <param name="name">Name of export.</param>
        /// <param name="type">A type of export.</param>
        /// <param name="parameters">A parameters of export.</param>
        /// <param name="context">Expression context.</param>
        /// <param name="caseSettings">Case settings.</param>
        /// <returns>
        /// A new export.
        /// </returns>
        public override Export CreateExport(string name, string type, ParameterCollection parameters, EvaluationContext context, SpiceNetlistCaseSensitivitySettings caseSettings)
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

            var entityName = (parameters[0] as VectorParameter)?.Elements[0].Value;
            var propertyName = (parameters[0] as VectorParameter)?.Elements[1].Value;

            if (entityName != null && propertyName != null)
            {
                if (entityName.Contains("#"))
                {
                    string objectName = $"{context.NameGenerator.GenerateObjectName(entityName)}_{context.Simulation.Name}";
                    return new PropertyExport(name, context.Simulation, objectName, propertyName);
                }
                else
                {
                    string objectName = context.NameGenerator.GenerateObjectName(entityName);

                    if (context.CircuitContext.FindObject(objectName, out _))
                    {
                        return new PropertyExport(name, context.Simulation, objectName, propertyName);
                    }
                    else
                    {
                        return new PropertyExport(name, context.Simulation, entityName, propertyName);
                    }
                }
            }

            throw new SpiceSharpParserException("Invalid property export", parameters.LineInfo);
        }
    }
}