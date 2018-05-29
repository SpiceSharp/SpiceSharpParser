using SpiceSharpParser.Common;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Common;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls
{
    /// <summary>
    /// Base for all control processors.
    /// </summary>
    public abstract class BaseControl : StatementProcessor<Control>, IGenerator
    {
        /// <summary>
        /// Gets name of Spice element.
        /// </summary>
        public abstract string TypeName
        {
            get;
        }

        public override bool CanProcess(Statement statement)
        {
            return statement is Control;
        }

        /// <summary>
        /// Defines a new user function.
        /// </summary>
        /// <param name="context">Processing context.</param>
        protected static void DefineUserFunction(
            IProcessingContext context,
            string name,
            List<string> arguments,
            string value)
        {
            CustomFunction userFunction = new CustomFunction();
            userFunction.Name = name;
            userFunction.VirtualParameters = false;
            userFunction.ArgumentsCount = arguments.Count;

            userFunction.Logic = (args, simulation) =>
            {
                var evaluator = context.Evaluator.CreateChildEvaluator();

                for (var i = 0; i < arguments.Count; i++)
                {
                    evaluator.SetParameter(arguments[i], (double)args[arguments.Count - i - 1]);
                }

                return evaluator.EvaluateDouble(value);
            };

            context.Evaluator.CustomFunctions.Add(name, userFunction);
        }
    }
}
