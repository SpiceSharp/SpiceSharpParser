using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reades .STEP_R <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class StepRegisterControl : BaseControl
    {
        public override string SpiceName => "step_r";

        /// <summary>
        /// Reades <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            if (statement.Parameters == null)
            {
                throw new System.ArgumentNullException(nameof(statement.Parameters));
            }

            if (statement.Parameters.Count < 4)
            {
                throw new WrongParametersCountException();
            }

            string firstParam = statement.Parameters[0].Image;

            switch (firstParam.ToLower())
            {
                case "param":
                    RegisterParameter(statement.Parameters.Skip(1)[0], context);
                    break;
                case "lin":
                    ReadLin(statement.Parameters.Skip(1), context);
                    break;
                case "dec":
                    ReadDec(statement.Parameters.Skip(1), context);
                    break;
                case "oct":
                    ReadOct(statement.Parameters.Skip(1), context);
                    break;
                default:
                    ReadOtherCases(statement.Parameters, context);
                    break;
            }
        }

        private void ReadOtherCases(ParameterCollection parameters, IReadingContext context)
        {
            bool list = false;
            int index = 0;
            for (var i = 0; i <= 2; i++)
            {
                if (parameters[i].Image.ToLower() == "list")
                {
                    index = i;
                    list = true;
                }
            }

            if (list)
            {
                if (parameters[1] is BracketParameter bp)
                {
                    RegisterParameter(bp,  context); // model parameter
                }
                else
                {
                    RegisterParameter(parameters[0], context); // source
                }
            }
            else
            {
                // lin
                if (parameters[1] is BracketParameter bp)
                {
                    RegisterParameter(bp, context); // model parameter
                }
                else
                {
                    RegisterParameter(parameters[0], context); // source
                }
            }
        }

        private void ReadOct(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters[1] is BracketParameter bp)
            {
                RegisterParameter(bp, context); // model parameter
            }
            else
            {
                RegisterParameter(parameters[0], context); // source
            }
        }

        private void ReadDec(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters[1] is BracketParameter bp)
            {
                RegisterParameter(bp, context); // model parameter
            }
            else
            {
                RegisterParameter(parameters[0], context); // source
            }
        }

        private void ReadLin(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters[1] is BracketParameter bp)
            {
                RegisterParameter(bp, context); // model parameter
            }
            else
            {
                RegisterParameter(parameters[0], context); // source
            }
        }


        private void RegisterParameter(Parameter variableParameter, IReadingContext context)
        {
            context.Evaluator.SetParameter(variableParameter.Image, 0);
        }
    }
}
