using SpiceSharpParser.Common;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Common;

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
        /// <param name="context">Processing context</param>
        /// <param name="parameter">An assigment parameter.</param>
        /// <param name="name">Name of custom function.</param>
        /// <param name="expression">Expression of custom function.</param>
        protected static void DefineUserFunction(IProcessingContext context, Model.Netlist.Spice.Objects.Parameters.AssignmentParameter parameter)
        {
            CustomFunction userFunction = new CustomFunction();
            userFunction.Name = parameter.Name;
            userFunction.VirtualParameters = false;
            userFunction.ArgumentsCount = parameter.Arguments.Count;

            userFunction.Logic = (args, simulation) =>
            {
                var evaluator = context.Evaluator.CreateChildEvaluator();

                for (var i = 0; i < parameter.Arguments.Count; i++)
                {
                    evaluator.SetParameter(parameter.Arguments[i], (double)args[parameter.Arguments.Count - i - 1]);
                }

                return evaluator.EvaluateDouble(parameter.Value);
            };

            context.Evaluator.CustomFunctions.Add(parameter.Name, userFunction);
        }
    }
}
