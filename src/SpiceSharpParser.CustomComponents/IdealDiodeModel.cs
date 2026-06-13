using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharpParser.CustomComponents.IdealDiodes;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Parser-side model container for LTspice-style ideal diode parameters.
    /// </summary>
    public class IdealDiodeModel : Entity<IdealDiodeParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdealDiodeModel"/> class.
        /// </summary>
        /// <param name="name">The model name.</param>
        public IdealDiodeModel(string name)
            : base(name)
        {
        }

        /// <inheritdoc />
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new BindingContext(this, simulation, behaviors);
            behaviors.Add(new IdealDiodeModelBehavior(context));

            simulation.EntityBehaviors.Add(behaviors);
        }

        private sealed class IdealDiodeModelBehavior : Behavior, IParameterized<IdealDiodeParameters>
        {
            public IdealDiodeModelBehavior(IBindingContext context)
                : base(context)
            {
                Parameters = context.GetParameterSet<IdealDiodeParameters>();
            }

            public IdealDiodeParameters Parameters { get; }
        }
    }
}
