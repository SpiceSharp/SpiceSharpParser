using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Evaluation;
using SpiceSharpParser.Connector.Exceptions;
using SpiceSharpParser.Model.SpiceObjects;

namespace SpiceSharpParser.Connector.Processors.Controls
{
    /// <summary>
    /// Processes .PARAM <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class ParamControl : BaseControl
    {
        public override string TypeName => "param";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, IProcessingContext context)
        {
            if (statement.Parameters == null)
            {
                throw new System.ArgumentNullException("paramters are null");
            }

            foreach (var param in statement.Parameters)
            {
                if (param is Model.SpiceObjects.Parameters.AssignmentParameter assigmentParameter)
                {
                    string name = assigmentParameter.Name;
                    string expression = assigmentParameter.Value;

                    if (assigmentParameter.Arguments.Count == 0)
                    {
                        context.Evaluator.SetParameter(name, expression);
                    }
                    else
                    {
                        DefineUserFunction(context, assigmentParameter, name, expression);
                    }
                }
                else
                {
                    throw new WrongParameterTypeException(".PARAM supports only assigments");
                }
            }
        }

        private static void DefineUserFunction(IProcessingContext context, Model.SpiceObjects.Parameters.AssignmentParameter a, string name, string expression)
        {
            SpiceFunction userFunction = new SpiceFunction();
            userFunction.Name = name;
            userFunction.VirtualParameters = false;
            userFunction.ArgumentsCount = a.Arguments.Count;

            userFunction.Logic = (args, simulation) =>
            {
                var evaluator = new Evaluator(context.Evaluator);

                for (var i = 0; i < a.Arguments.Count; i++)
                {
                    evaluator.SetParameter(a.Arguments[i], (double)args[a.Arguments.Count - i - 1]);
                }

                return evaluator.EvaluateDouble(expression);
            };

            context.Evaluator.ExpressionParser.CustomFunctions.Add(name, userFunction);
        }
    }
}
