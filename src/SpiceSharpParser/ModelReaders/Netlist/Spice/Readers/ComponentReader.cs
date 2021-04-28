using System;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads all supported <see cref="Component"/> from SPICE netlist object model.
    /// </summary>
    public class ComponentReader : StatementReader<Component>, IComponentReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentReader"/> class.
        /// </summary>
        /// <param name="mapper">A component mapper.</param>
        public ComponentReader(IMapper<IComponentGenerator> mapper)
        {
            Mapper = mapper ?? throw new NullReferenceException(nameof(mapper));
        }

        /// <summary>
        /// Gets the component mapper.
        /// </summary>
        public IMapper<IComponentGenerator> Mapper { get; }

        /// <summary>
        /// Reads a component statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Component statement, ICircuitContext context)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string componentName = statement.Name;
            IComponentGenerator generator = GetComponentGenerator(context, componentName, statement.LineInfo, out string componentType);

            var statementClone = statement.Clone() as Component;
            if (context.Parent != null)
            {
                // entity is part of subcircuit

                var evalContext = context.Evaluator.GetEvaluationContext();

                if (evalContext.Parameters.ContainsKey("m"))
                {
                    var mParameter = evalContext.Parameters["m"];
                    if (mParameter != null)
                    {
                        statementClone.PinsAndParameters.Add(new AssignmentParameter() { Name = "m", Value = mParameter.ValueExpression });
                    }
                }

                if (evalContext.Parameters.ContainsKey("n"))
                {
                    var nParameter = evalContext.Parameters["n"];

                    if (nParameter != null)
                    {
                        statementClone.PinsAndParameters.Add(new AssignmentParameter() { Name = "n", Value = nParameter.ValueExpression });
                    }
                }
            }

            IEntity entity = generator?.Generate(
                context.NameGenerator.GenerateObjectName(componentName),
                componentName,
                componentType,
                statementClone.PinsAndParameters,
                context);

            if (entity != null)
            {
                context.Result.AddEntity(entity);
            }
        }

        private IComponentGenerator GetComponentGenerator(ICircuitContext context, string componentName, SpiceLineInfo lineInfo, out string componentType)
        {
            foreach (var map in Mapper)
            {
                if (componentName.StartsWith(map.Key, context.CaseSensitivity.IsEntityNamesCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
                {
                    componentType = map.Key;
                    return map.Value;
                }
            }

            context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Unsupported component {componentName}", lineInfo));
            componentType = null;
            return null;
        }
    }
}