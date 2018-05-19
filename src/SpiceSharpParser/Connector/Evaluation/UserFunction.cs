using System;

namespace SpiceSharpParser.Connector.Evaluation
{
    public class UserFunction
    {
        /// <summary>
        /// Gets or sets the name of user function
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets logic for user function
        /// Function:
        /// args => simulation => result.
        /// </summary>
        public Func<object[], object, object> Logic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether function has virtual parameters.
        /// </summary>
        public bool VirtualParameters { get; set; } = true;

        // TODO: future add validation

        /// <summary>
        /// Gets or sets the return type of user function .
        /// </summary>
        public Type ReturnType { get; set; }
    }
}
