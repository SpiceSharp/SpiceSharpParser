using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Reads .TRAN <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class TransientControl : SimulationControl
    {
        public TransientControl(IMapper<Exporter> mapper)
            : base(mapper)
        {
        }

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            CreateSimulations(statement, context, CreateTransientSimulation);
        }

        private Transient CreateTransientSimulation(string name, Control statement, IReadingContext context)
        {
            switch (statement.Parameters.Count)
            {
                case 0:
                    context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, ".tran control - Step expected", statement.LineInfo));
                    break;

                case 1:
                    context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, ".tran control - Maximum time expected", statement.LineInfo));
                    break;
            }

            bool useIc = false;
            var clonedParameters = (ParameterCollection)statement.Parameters.Clone();
            var lastParameter = clonedParameters[clonedParameters.Count - 1];
            if (lastParameter is WordParameter w && w.Value.ToLower() == "uic")
            {
                useIc = true;
                clonedParameters.RemoveAt(clonedParameters.Count - 1);
            }

            Transient tran = null;

            double? maxStep = null;
            double? step = null;
            double? final = null;
            double? start = null;
            double[] args;

            switch (clonedParameters.Count)
            {
                case 2:
                    step = context.Evaluator.EvaluateDouble(clonedParameters[0].Value);
                    final = context.Evaluator.EvaluateDouble(clonedParameters[1].Value);
                    args = new double[] { step.Value, final.Value };
                    break;
                case 3:
                    step = context.Evaluator.EvaluateDouble(clonedParameters[0].Value);
                    final = context.Evaluator.EvaluateDouble(clonedParameters[1].Value);
                    maxStep = context.Evaluator.EvaluateDouble(clonedParameters[2].Value);
                    args = new double[] { step.Value, final.Value, maxStep.Value };
                    break;
                case 4:
                    step = context.Evaluator.EvaluateDouble(clonedParameters[0].Value);
                    final = context.Evaluator.EvaluateDouble(clonedParameters[1].Value);
                    start = context.Evaluator.EvaluateDouble(clonedParameters[2].Value);
                    maxStep = context.Evaluator.EvaluateDouble(clonedParameters[3].Value);

                    args = new double[] { step.Value, final.Value, maxStep.Value, start.Value };
                    break;
                default:
                    context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, ".TRAN control - Too many parameters for .TRAN", statement.LineInfo));
                    return null;
            }

            var factory = context.SimulationConfiguration.TimeParametersFactory;

            if (factory != null)
            {
                var config = context.SimulationConfiguration.TransientConfiguration;
                config.Step = step;
                config.Final = final;
                config.MaxStep = maxStep;
                config.Start = start;
                config.UseIc = useIc;
                tran = new Transient(name, factory(config));
            }
            else
            {
                if (clonedParameters.Count == 2)
                {
                    tran = new Transient(name, step.Value, final.Value);
                }
                else
                {
                    if (clonedParameters.Count == 3)
                    {
                        tran = new Transient(name, step.Value, final.Value, maxStep.Value);
                    }
                    else
                    {
                        tran = new Transient(
                            name,
                            new Trapezoidal()
                            {
                                StartTime = start.Value,
                                StopTime = final.Value,
                                MaxStep = maxStep.Value,
                                InitialStep = step.Value
                            });
                    }
                }
            }
            tran.TimeParameters.UseIc = useIc;

            ConfigureCommonSettings(tran, context);

            tran.BeforeLoad += (truncateSender, truncateArgs) =>
            {
                context.Evaluator.SetParameter("TIME", ((IStateful<IIntegrationMethod>)tran).State.Time, tran);
            };
            context.Result.Simulations.Add(tran);

            return tran;
        }
    }
}