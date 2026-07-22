using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Fourier;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .FOUR transient Fourier post-processing statements.
    /// </summary>
    public class FourControl : ExportControl
    {
        private readonly FourierAnalysisCalculator _fourierAnalysisCalculator;

        public FourControl(IMapper<Exporter> mapper, IExportFactory exportFactory)
            : this(mapper, exportFactory, new FourierAnalysisCalculator())
        {
        }

        public FourControl(
            IMapper<Exporter> mapper,
            IExportFactory exportFactory,
            FourierAnalysisCalculator fourierAnalysisCalculator)
            : base(mapper, exportFactory)
        {
            _fourierAnalysisCalculator = fourierAnalysisCalculator ?? throw new ArgumentNullException(nameof(fourierAnalysisCalculator));
        }

        public override void Read(Control statement, IReadingContext context)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (statement.Parameters.Count < 2)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    ".FOUR statement requires a fundamental frequency and at least one signal",
                    statement.LineInfo);
                return;
            }

            if (!TryEvaluateFrequency(statement.Parameters[0], context, out double fundamentalFrequency))
            {
                return;
            }

            var transientSimulations = context.Result.Simulations
                .Where(simulation => simulation is Transient)
                .ToList();

            if (transientSimulations.Count == 0)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    ".FOUR requires a .TRAN analysis",
                    statement.LineInfo);
                return;
            }

            for (int parameterIndex = 1; parameterIndex < statement.Parameters.Count; parameterIndex++)
            {
                var signalParameter = statement.Parameters[parameterIndex];
                foreach (var simulation in transientSimulations)
                {
                    SetupFourierAnalysis(signalParameter, fundamentalFrequency, simulation, context);
                }
            }
        }

        private static bool TryEvaluateFrequency(Parameter parameter, IReadingContext context, out double frequency)
        {
            frequency = double.NaN;

            try
            {
                frequency = context.Evaluator.EvaluateDouble(parameter.Value);
            }
            catch (Exception ex) when (ReaderExceptionClassifier.IsRecoverableInputException(ex))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $".FOUR fundamental frequency could not be evaluated: {ex.Message}",
                    parameter.LineInfo);
                return false;
            }

            if (double.IsNaN(frequency) || double.IsInfinity(frequency) || frequency <= 0.0)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    ".FOUR fundamental frequency must be positive and finite",
                    parameter.LineInfo);
                return false;
            }

            return true;
        }

        private void SetupFourierAnalysis(
            Parameter signalParameter,
            double fundamentalFrequency,
            ISimulationWithEvents simulation,
            IReadingContext context)
        {
            Export export;

            try
            {
                export = GenerateExport(signalParameter, context, simulation);
            }
            catch (Exception ex) when (ReaderExceptionClassifier.IsRecoverableInputException(ex))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $".FOUR signal '{signalParameter}': {ex.Message}",
                    signalParameter.LineInfo);
                return;
            }

            if (export == null)
            {
                return;
            }

            var transient = (Transient)simulation;
            var samples = new List<(double Time, double Value)>();

            simulation.EventExportData += (_, __) =>
            {
                double value;
                try
                {
                    value = export.Extract();
                }
                catch
                {
                    value = double.NaN;
                }

                samples.Add((transient.Time, value));
            };

            simulation.EventAfterExecute += (_, __) =>
            {
                var result = _fourierAnalysisCalculator.Analyze(
                    export.Name,
                    simulation.Name,
                    fundamentalFrequency,
                    samples);

                lock (context.Result.FourierAnalyses)
                {
                    context.Result.FourierAnalyses.Add(result);
                }

                if (!result.Success)
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $".FOUR {export.Name}: {result.ErrorMessage}",
                        signalParameter.LineInfo);
                }
            };
        }
    }
}
