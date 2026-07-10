using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharpMechanical2D.Core;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Base;
using System;

namespace SpiceSharpMechanical2D.Tests.Coordinates
{
    internal abstract class TestGeneralizedForceEntity : Entity
    {
        protected TestGeneralizedForceEntity(string name, string coordinateName)
            : base(name)
        {
            CoordinateName = coordinateName ?? throw new ArgumentNullException(nameof(coordinateName));
        }

        protected string CoordinateName { get; }

        public override void CreateBehaviors(ISimulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (!simulation.UsesBehaviors<IBiasingBehavior>())
            {
                return;
            }

            IMechanicalCoordinateBehavior coordinate =
                new Reference(CoordinateName).GetContainer(simulation)
                    .GetValue<IMechanicalCoordinateBehavior>();
            var behaviors = new BehaviorContainer(Name);
            var context = new BindingContext(this, simulation, behaviors);
            behaviors.Add(CreateBehavior(context, coordinate));
            simulation.EntityBehaviors.Add(behaviors);
        }

        protected abstract IBehavior CreateBehavior(
            IBindingContext context,
            IMechanicalCoordinateBehavior coordinate);
    }

    internal sealed class ConstantGeneralizedForce : TestGeneralizedForceEntity
    {
        public ConstantGeneralizedForce(
            string name,
            string coordinateName,
            double force)
            : base(name, coordinateName)
        {
            Force = force;
        }

        public double Force { get; }

        public override IEntity Clone() =>
            new ConstantGeneralizedForce(Name, CoordinateName, Force);

        protected override IBehavior CreateBehavior(
            IBindingContext context,
            IMechanicalCoordinateBehavior coordinate) =>
            new ConstantBehavior(context, coordinate, Force);

        private sealed class ConstantBehavior : TestGeneralizedForceBehavior
        {
            private readonly double _force;

            public ConstantBehavior(
                IBindingContext context,
                IMechanicalCoordinateBehavior coordinate,
                double force)
                : base(context, coordinate)
            {
                if (double.IsNaN(force) || double.IsInfinity(force))
                {
                    throw new SpiceSharpException($"Test force '{Name}' requires a finite force.");
                }

                _force = force;
            }

            protected override void Evaluate(
                double position,
                double velocity,
                out double force,
                out double derivativeByPosition,
                out double derivativeByVelocity)
            {
                force = _force;
                derivativeByPosition = 0.0;
                derivativeByVelocity = 0.0;
            }
        }
    }

    internal sealed class LinearGeneralizedDamping : TestGeneralizedForceEntity
    {
        public LinearGeneralizedDamping(
            string name,
            string coordinateName,
            double damping)
            : base(name, coordinateName)
        {
            Damping = damping;
        }

        public double Damping { get; }

        public override IEntity Clone() =>
            new LinearGeneralizedDamping(Name, CoordinateName, Damping);

        protected override IBehavior CreateBehavior(
            IBindingContext context,
            IMechanicalCoordinateBehavior coordinate) =>
            new DampingBehavior(context, coordinate, Damping);

        private sealed class DampingBehavior : TestGeneralizedForceBehavior
        {
            private readonly double _damping;

            public DampingBehavior(
                IBindingContext context,
                IMechanicalCoordinateBehavior coordinate,
                double damping)
                : base(context, coordinate)
            {
                if (damping < 0.0 || double.IsNaN(damping) || double.IsInfinity(damping))
                {
                    throw new SpiceSharpException(
                        $"Test damping '{Name}' requires finite nonnegative damping.");
                }

                _damping = damping;
            }

            protected override void Evaluate(
                double position,
                double velocity,
                out double force,
                out double derivativeByPosition,
                out double derivativeByVelocity)
            {
                force = -_damping * velocity;
                derivativeByPosition = 0.0;
                derivativeByVelocity = -_damping;
            }
        }
    }

    internal sealed class LinearGeneralizedSpringToReference : TestGeneralizedForceEntity
    {
        public LinearGeneralizedSpringToReference(
            string name,
            string coordinateName,
            double stiffness,
            double referencePosition = 0.0)
            : base(name, coordinateName)
        {
            Stiffness = stiffness;
            ReferencePosition = referencePosition;
        }

        public double Stiffness { get; }

        public double ReferencePosition { get; }

        public override IEntity Clone() =>
            new LinearGeneralizedSpringToReference(
                Name,
                CoordinateName,
                Stiffness,
                ReferencePosition);

        protected override IBehavior CreateBehavior(
            IBindingContext context,
            IMechanicalCoordinateBehavior coordinate) =>
            new SpringBehavior(context, coordinate, Stiffness, ReferencePosition);

        private sealed class SpringBehavior : TestGeneralizedForceBehavior
        {
            private readonly double _referencePosition;
            private readonly double _stiffness;

            public SpringBehavior(
                IBindingContext context,
                IMechanicalCoordinateBehavior coordinate,
                double stiffness,
                double referencePosition)
                : base(context, coordinate)
            {
                if (stiffness < 0.0 || double.IsNaN(stiffness) || double.IsInfinity(stiffness))
                {
                    throw new SpiceSharpException(
                        $"Test spring '{Name}' requires finite nonnegative stiffness.");
                }

                if (double.IsNaN(referencePosition) || double.IsInfinity(referencePosition))
                {
                    throw new SpiceSharpException(
                        $"Test spring '{Name}' requires a finite reference position.");
                }

                _stiffness = stiffness;
                _referencePosition = referencePosition;
            }

            protected override void Evaluate(
                double position,
                double velocity,
                out double force,
                out double derivativeByPosition,
                out double derivativeByVelocity)
            {
                force = -_stiffness * (position - _referencePosition);
                derivativeByPosition = -_stiffness;
                derivativeByVelocity = 0.0;
            }
        }
    }

    internal abstract class TestGeneralizedForceBehavior : Behavior, IBiasingBehavior
    {
        private readonly IMechanicalCoordinateBehavior _coordinate;
        private readonly ElementSet<double> _elements;
        private readonly double[] _elementValues = new double[3];
        private readonly ITimeSimulationState _time;

        protected TestGeneralizedForceBehavior(
            IBindingContext context,
            IMechanicalCoordinateBehavior coordinate)
            : base(context)
        {
            _coordinate = coordinate ?? throw new ArgumentNullException(nameof(coordinate));
            var biasing = context.GetState<IBiasingSimulationState>();
            _time = context.GetState<ITimeSimulationState>();

            int row = biasing.Map[coordinate.VelocityVariable];
            int position = biasing.Map[coordinate.PositionVariable];
            int velocity = biasing.Map[coordinate.VelocityVariable];
            _elements = new ElementSet<double>(
                biasing.Solver,
                new[]
                {
                    new MatrixLocation(row, position),
                    new MatrixLocation(row, velocity),
                },
                new[] { row });
        }

        void IBiasingBehavior.Load()
        {
            if (_time.UseDc)
            {
                return;
            }

            double position = _coordinate.Position;
            double velocity = _coordinate.Velocity;
            Evaluate(
                position,
                velocity,
                out double force,
                out double derivativeByPosition,
                out double derivativeByVelocity);

            // The dynamics residual is M*udot - Q(q, u) = 0. This behavior
            // contributes -dQ/dq and -dQ/du to the Newton matrix, and
            // Q - dQ/dq*q - dQ/du*u to the right-hand side.
            _elementValues[0] = -derivativeByPosition;
            _elementValues[1] = -derivativeByVelocity;
            _elementValues[2] = force
                - (derivativeByPosition * position)
                - (derivativeByVelocity * velocity);
            _elements.Add(_elementValues);
        }

        protected abstract void Evaluate(
            double position,
            double velocity,
            out double force,
            out double derivativeByPosition,
            out double derivativeByVelocity);
    }
}
