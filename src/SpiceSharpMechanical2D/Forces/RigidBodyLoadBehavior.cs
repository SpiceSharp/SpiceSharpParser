using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Base;
using System;

namespace SpiceSharpMechanical2D.Forces
{
    /// <summary>
    /// Provides the shared transient lifecycle for direct rigid-body loads.
    /// </summary>
    internal abstract class RigidBodyLoadBehavior : Behavior, IBiasingBehavior
    {
        private readonly ITimeSimulationState _time;

        protected RigidBodyLoadBehavior(
            IBindingContext context,
            IRigidBody2DBehavior body)
            : base(context)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body));
            _time = context.GetState<ITimeSimulationState>();
        }

        protected IRigidBody2DBehavior Body { get; }

        void IBiasingBehavior.Load()
        {
            if (!_time.UseDc)
            {
                LoadTransient();
            }
        }

        protected abstract void LoadTransient();
    }

    internal static class RigidBodyLoadBinding
    {
        public static IRigidBody2DBehavior ResolveBody(
            string bodyName,
            ISimulation simulation)
        {
            if (string.IsNullOrWhiteSpace(bodyName))
            {
                throw new ArgumentException("A rigid-body entity name is required.", nameof(bodyName));
            }

            return new Reference(bodyName)
                .GetContainer(simulation)
                .GetValue<IRigidBody2DBehavior>();
        }
    }

    internal static class ForceValueValidation
    {
        public static bool IsFinite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);

        public static bool IsFinite(SpiceSharpMechanical2D.Mathematics.Vector2D value) =>
            IsFinite(value.X) && IsFinite(value.Y);
    }
}
