﻿using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Reads .TRAN <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class TransientControl : SimulationControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context
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

            ConfigureCommonSettings(tran, context);
            ConfigureTransientSettings(tran, context, useIc);

            tran.ExportSimulationData += (object sender, ExportDataEventArgs e) => {
                context.Evaluators.SetParameter("TIME", e.Time);
            };

            context.Result.AddSimulation(tran);

            return tran;
        }

        private void ConfigureTransientSettings(Transient tran, IReadingContext context, bool useIc)
        {
            if (context.Result.SimulationConfiguration.Method != null)
            {
                tran.Configurations.Get<TimeConfiguration>().Method = context.Result.SimulationConfiguration.Method;
            }

            if (context.Result.SimulationConfiguration.TranMaxIterations.HasValue)
            {
                tran.Configurations.Get<TimeConfiguration>().TranMaxIterations = context.Result.SimulationConfiguration.TranMaxIterations.Value;
            }

            tran.Configurations.Get<TimeConfiguration>().UseIc = useIc;
        }
    }
}
