using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Base;
using System;

namespace SpiceSharp.Physics2D.Tests.Bodies
{
    internal sealed class TestRigidBodyLoad2D : Entity
    {
        public TestRigidBodyLoad2D(
            string name,
            string bodyName,
            Vector2D worldForce,
            double torque)
            : base(name)
        {
            BodyName = bodyName ?? throw new ArgumentNullException(nameof(bodyName));
            WorldForce = worldForce;
            Torque = torque;
        }

        public string BodyName { get; }

        public Vector2D WorldForce { get; }

        public double Torque { get; }

        public override IEntity Clone() =>
            new TestRigidBodyLoad2D(Name, BodyName, WorldForce, Torque);

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

            IRigidBody2DBehavior body = new Reference(BodyName)
                .GetContainer(simulation)
                .GetValue<IRigidBody2DBehavior>();
            var behaviors = new BehaviorContainer(Name);
            var context = new BindingContext(this, simulation, behaviors);
            behaviors.Add(new LoadBehavior(context, body, WorldForce, Torque));
            simulation.EntityBehaviors.Add(behaviors);
        }

        private sealed class LoadBehavior : Behavior, IBiasingBehavior
        {
            private readonly ElementSet<double> _elements;
            private readonly double[] _values = new double[3];
            private readonly ITimeSimulationState _time;

            public LoadBehavior(
                IBindingContext context,
                IRigidBody2DBehavior body,
                Vector2D worldForce,
                double torque)
                : base(context)
            {
                if (body == null)
                {
                    throw new ArgumentNullException(nameof(body));
                }

                if (!IsFinite(worldForce.X)
                    || !IsFinite(worldForce.Y)
                    || !IsFinite(torque))
                {
                    throw new SpiceSharpException(
                        $"Test body load '{Name}' requires finite force and torque values.");
                }

                var biasing = context.GetState<IBiasingSimulationState>();
                _time = context.GetState<ITimeSimulationState>();
                _elements = new ElementSet<double>(
                    biasing.Solver,
                    new[]
                    {
                        biasing.Map[body.VelocityXVariable],
                        biasing.Map[body.VelocityYVariable],
                        biasing.Map[body.AngularVelocityVariable],
                    });
                _values[0] = worldForce.X;
                _values[1] = worldForce.Y;
                _values[2] = torque;
            }

            void IBiasingBehavior.Load()
            {
                if (!_time.UseDc)
                {
                    _elements.Add(_values);
                }
            }

            private static bool IsFinite(double value) =>
                !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
