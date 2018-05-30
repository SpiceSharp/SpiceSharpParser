using SpiceSharpParser.Common;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Common;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Base for all control readers.
    /// </summary>
    public abstract class BaseControl : StatementReader<Control>, ISpiceObjectReader
    {
        /// <summary>
        /// Gets name of Spice element.
        /// </summary>
        public abstract string SpiceName
        {
            get;
        }

        public override bool CanRead(Statement statement)
        {
            return statement is Control;
        }

        /// <summary>
        /// Defines a new user function.
        /// </summary>
        /// <param name="context">Reading context.</param>
        protected static void DefineUserFunction(
            IReadingContext context,
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
