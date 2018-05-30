using SpiceSharp.Simulations;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Reades .TRAN <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class TransientControl : SimulationControl
    {
        public override string SpiceName => "tran";

        /// <summary>
        /// Reades <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Read(Control statement, IReadingContext context)
        {
            CreateSimulations(statement, context, CreateTransientSimulation);
        }

        private Transient CreateTransientSimulation(string name, Control statement, IReadingContext context)
        {
            switch (statement.Parameters.Count)
            {
                case 0: throw new WrongParametersCountException(".tran control - Step expected");
                case 1: throw new WrongParametersCountException(".tran control - Maximum time expected");
            }

            bool useIc = false;
            var clonedParameters = (ParameterCollection)statement.Parameters.Clone();
            var lastParameter = clonedParameters[clonedParameters.Count - 1];
            if (lastParameter is WordParameter w && w.Image.ToLower() == "uic")
            {
                useIc = true;
                clonedParameters.Remove(clonedParameters.Count - 1);
            }

            Transient tran = null;

            switch (clonedParameters.Count)
            {
                case 2:
                    tran = new Transient(
                        name,
                        context.ParseDouble(clonedParameters[0].Image),
                        context.ParseDouble(clonedParameters[1].Image));
                    break;
                case 3:
                    tran = new Transient(
                        name,
                        context.ParseDouble(clonedParameters[0].Image),
                        context.ParseDouble(clonedParameters[1].Image),
                        context.ParseDouble(clonedParameters[2].Image));
                    break;
                case 4:
                    throw new WrongParametersCountException(".tran control - Too many parameters for .tran");
            }

            SetBaseConfiguration(tran.BaseConfiguration, context);
            SetTransientParamters(tran, context, useIc);

            context.Result.AddSimulation(tran);

            return tran;
        }

        private void SetTransientParamters(Transient tran, IReadingContext context, bool useIc)
        {
            if (context.Result.SimulationConfiguration.Method != null)
            {
                tran.ParameterSets.Get<TimeConfiguration>().Method = context.Result.SimulationConfiguration.Method;
            }

            if (context.Result.SimulationConfiguration.TranMaxIterations.HasValue)
            {
                tran.ParameterSets.Get<TimeConfiguration>().TranMaxIterations = context.Result.SimulationConfiguration.TranMaxIterations.Value;
            }

            tran.ParameterSets.Get<TimeConfiguration>().UseIc = useIc;
        }
    }
}
