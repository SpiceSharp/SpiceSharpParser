using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads all <see cref="Parallel"/> from SPICE netlist object model.
    /// </summary>
    public class ParallelReader : StatementReader<Parallel>
    {
        public ParallelReader(ISpiceStatementsReader statementsReader)
        {
            StatementsReader = statementsReader ?? throw new ArgumentNullException(nameof(statementsReader));
        }

        public ISpiceStatementsReader StatementsReader { get; }

        /// <summary>
        /// Reads a parallel statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A reading context.</param>
        public override void Read(Parallel statement, IReadingContext context)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var parallelContext = new ReadingContext(
                $"{statement.Name} Parallel context",
                context,
                context.EvaluationContext,
                context.SimulationPreparations,
                context.NameGenerator,
                context.StatementsReader,
                context.WaveformReader,
                context.Exporters,
                context.SimulationConfiguration,
                context.Result,
                context.ReaderSettings);

            foreach (var st in statement.Statements)
            {
                StatementsReader.Read(st, parallelContext);
            }

            string componentName = context.ReaderSettings.ExpandSubcircuits ? context.NameGenerator.GenerateObjectName(statement.Name) : statement.Name;

            var parallelEntity = new SpiceSharp.Components.Parallel(componentName, parallelContext.ContextEntities);

            context.ContextEntities.Add(parallelEntity);
        }
    }
}