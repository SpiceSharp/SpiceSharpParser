using SpiceSharp;
using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Joints;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharpMechanical2D.Tests.Numerics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;
using System.Linq;
using Xunit;

namespace SpiceSharpMechanical2D.Tests.Joints
{
    public class JointEquationTests
    {
        private static readonly MechanicalAnchor2D AnchorA =
            MechanicalAnchor2D.Body("a", new Vector2D(0.31, -0.22));
        private static readonly MechanicalAnchor2D AnchorB =
            MechanicalAnchor2D.Body("b", new Vector2D(-0.27, 0.41));

        [Fact]
        public void RevoluteAnalyticJacobianMatchesFiniteDifference()
        {
            AssertJacobian(
                EvaluateRevolute,
                EvaluateRevoluteWithJacobian,
                "RevoluteJoint2D");
        }

        [Fact]
        public void WeldAnalyticJacobianMatchesFiniteDifference()
        {
            AssertJacobian(
                EvaluateWeld,
                EvaluateWeldWithJacobian,
                "WeldJoint2D");
        }

        [Fact]
        public void BodyAttachedRotatingPrismaticGuideJacobianMatchesFiniteDifference()
        {
            AssertJacobian(
                EvaluatePrismatic,
                EvaluatePrismaticWithJacobian,
                "PrismaticJoint2D rotating guide");
        }

        [Fact]
        public void JointLoadsPreserveLinearAndAngularActionReaction()
        {
            double[] state = CreateState();
            state[3] = state[4] = state[5] = 0.0;
            state[9] = state[10] = state[11] = 0.0;
            AssertInternalBalance(EvaluateRevolute(state), state, "revolute");
            AssertInternalBalance(EvaluateWeld(state), state, "weld");
            AssertInternalBalance(EvaluatePrismatic(state), state, "prismatic");
        }

        [Fact]
        public void PrismaticGuideIsFreeAlongAxisWhenAxialLawIsDisabled()
        {
            var loads = new double[6];
            var jacobian = new double[6, 12];
            PrismaticJoint2DEvaluation result = PrismaticJoint2DEquation.Evaluate(
                MechanicalAnchor2D.World(Vector2D.Zero),
                default,
                MechanicalAnchor2D.Body("slider", Vector2D.Zero),
                new ConnectionBodyState2D(
                    new Vector2D(2.5, 0.0),
                    0.0,
                    new Vector2D(-1.2, 0.0),
                    0.0),
                Vector2D.UnitX,
                900.0,
                12.0,
                0.0,
                300.0,
                8.0,
                0.0,
                0.0,
                0.0,
                loads,
                jacobian);

            NumericAssert.Equal(2.5, result.AxialTravel, 1e-12, 1e-12);
            NumericAssert.Equal(-1.2, result.AxialVelocity, 1e-12, 1e-12);
            Assert.InRange(Math.Abs(result.NormalError), 0.0, 1e-12);
            Assert.InRange(Math.Abs(loads[3]), 0.0, 1e-12);
            Assert.InRange(Math.Abs(loads[4]), 0.0, 1e-12);
            Assert.InRange(Math.Abs(loads[5]), 0.0, 1e-12);
        }

        [Fact]
        public void PrismaticAxialLawProducesConfiguredRestoringForce()
        {
            var loads = new double[6];
            var jacobian = new double[6, 12];
            PrismaticJoint2DEvaluation result = PrismaticJoint2DEquation.Evaluate(
                MechanicalAnchor2D.World(Vector2D.Zero),
                default,
                MechanicalAnchor2D.Body("slider", Vector2D.Zero),
                new ConnectionBodyState2D(new Vector2D(1.5, 0.0), 0.0, Vector2D.Zero, 0.0),
                Vector2D.UnitX,
                100.0,
                0.0,
                0.0,
                100.0,
                0.0,
                1.0,
                20.0,
                0.0,
                loads,
                jacobian);

            NumericAssert.Equal(-10.0, loads[3], 1e-12, 1e-12);
            NumericAssert.Equal(2.5, result.StoredElasticEnergy, 1e-12, 1e-12);
        }

        [Fact]
        public void PrismaticGeneralizedLoadsAreNegativeElasticEnergyGradient()
        {
            double[] state = CreateState();
            double[] loads = EvaluatePrismaticElastic(state, out double energy);
            int[] configurationColumns = { 0, 1, 2, 6, 7, 8 };
            double maximumMismatch = 0.0;
            for (int loadIndex = 0; loadIndex < configurationColumns.Length; loadIndex++)
            {
                int column = configurationColumns[loadIndex];
                double step = 1e-6 * Math.Max(1.0, Math.Abs(state[column]));
                double[] plus = (double[])state.Clone();
                double[] minus = (double[])state.Clone();
                plus[column] += step;
                minus[column] -= step;
                EvaluatePrismaticElastic(plus, out double plusEnergy);
                EvaluatePrismaticElastic(minus, out double minusEnergy);
                double negativeGradient = -(plusEnergy - minusEnergy) / (2.0 * step);
                maximumMismatch = Math.Max(
                    maximumMismatch,
                    Math.Abs(loads[loadIndex] - negativeGradient));
                NumericAssert.Equal(negativeGradient, loads[loadIndex], 2e-7, 2e-7);
            }

            Console.WriteLine(FormattableString.Invariant(
                $"Prismatic energy-gradient maximum absolute mismatch={maximumMismatch:R}; energy={energy:R} J."));
        }

        [Fact]
        public void ReportedJointDissipationIsNonnegative()
        {
            double[] state = CreateState();
            var loads = new double[6];
            var jacobian = new double[6, 12];
            RevoluteJoint2DEvaluation revolute = RevoluteJoint2DEquation.Evaluate(
                AnchorA, ToBodyState(state, 0), AnchorB, ToBodyState(state, 6),
                14.0, 0.7, loads, jacobian);
            WeldJoint2DEvaluation weld = WeldJoint2DEquation.Evaluate(
                AnchorA, ToBodyState(state, 0), AnchorB, ToBodyState(state, 6),
                14.0, 0.7, -0.18, 6.0, 0.4, loads, jacobian);
            PrismaticJoint2DEvaluation prismatic = EvaluatePrismaticResult(state, loads, jacobian);

            Assert.True(revolute.DissipatedPower >= 0.0);
            Assert.True(weld.DissipatedPower >= 0.0);
            Assert.True(prismatic.DissipatedPower >= 0.0);
        }

        [Fact]
        public void InvalidJointTopologyAndAxisAreRejectedAtSetup()
        {
            MechanicalAnchor2D world = MechanicalAnchor2D.World(Vector2D.Zero);
            MechanicalAnchor2D body = MechanicalAnchor2D.Body("body", Vector2D.Zero);
            Assert.Throws<ArgumentException>(() =>
                new RevoluteJoint2D("world-world", world, world, 1.0));
            Assert.Throws<ArgumentException>(() =>
                new WeldJoint2D("same-body", body, body, 0.0, 1.0, 0.0, 1.0, 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PrismaticJoint2D(
                    "zero-axis", world, body, Vector2D.Zero, 0.0,
                    1.0, 0.0, 1.0, 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RevoluteJoint2D("negative", world, body, -1.0));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new WeldJoint2D("nonfinite", world, body, double.NaN, 1.0, 0.0, 1.0, 0.0));
        }

        [Fact]
        public void MissingBodyFailsDuringSetupWithJointAndBodyNames()
        {
            var joint = new RevoluteJoint2D(
                "missing-body-joint",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body("absent-body", Vector2D.Zero),
                1.0);
            var simulation = new Transient(
                "tran",
                new Trapezoidal { InitialStep = 0.01, MaxStep = 0.01, StopTime = 0.01 });

            SpiceSharpException exception = Assert.Throws<SpiceSharpException>(() =>
                simulation.Run(new Circuit(joint)).ToArray());

            Assert.Contains("missing-body-joint", exception.Message);
            Assert.Contains("absent-body", exception.Message);
        }

        private static void AssertJacobian(
            Func<double[], double[]> evaluate,
            Action<double[], double[], double[,]> evaluateWithJacobian,
            string label)
        {
            double[] state = CreateState();
            var loads = new double[6];
            var analytic = new double[6, 12];
            evaluateWithJacobian(state, loads, analytic);
            double[,] numerical = FiniteDifferenceJacobian.Calculate(
                evaluate,
                state,
                relativeStep: 1e-6,
                minimumStep: 1e-7);
            NumericComparison comparison = NumericAssert.JacobianEqual(
                analytic,
                numerical,
                2e-7,
                1e-5,
                label);

            Console.WriteLine(FormattableString.Invariant(
                $"{label} Jacobian maximum absolute mismatch={comparison.MaximumAbsoluteMismatch:R}."));
        }

        private static void AssertInternalBalance(double[] loads, double[] state, string label)
        {
            double forceResidualX = loads[0] + loads[3];
            double forceResidualY = loads[1] + loads[4];
            double torqueResidual =
                (state[0] * loads[1]) - (state[1] * loads[0]) + loads[2]
                + (state[6] * loads[4]) - (state[7] * loads[3]) + loads[5];
            double maximum = Math.Max(
                Math.Max(Math.Abs(forceResidualX), Math.Abs(forceResidualY)),
                Math.Abs(torqueResidual));

            Console.WriteLine(FormattableString.Invariant(
                $"{label} maximum action/reaction residual={maximum:R}."));
            Assert.InRange(maximum, 0.0, 2e-12);
        }

        private static double[] CreateState() => new[]
        {
            -0.71, 0.53, 0.37, 0.82, -0.44, 0.69,
            1.28, -0.19, -0.48, -0.31, 1.07, -0.52,
        };

        private static double[] EvaluateRevolute(double[] state)
        {
            var loads = new double[6];
            var jacobian = new double[6, 12];
            EvaluateRevoluteWithJacobian(state, loads, jacobian);
            return loads;
        }

        private static void EvaluateRevoluteWithJacobian(
            double[] state,
            double[] loads,
            double[,] jacobian) =>
            RevoluteJoint2DEquation.Evaluate(
                AnchorA,
                ToBodyState(state, 0),
                AnchorB,
                ToBodyState(state, 6),
                14.0,
                0.7,
                loads,
                jacobian);

        private static double[] EvaluateWeld(double[] state)
        {
            var loads = new double[6];
            var jacobian = new double[6, 12];
            EvaluateWeldWithJacobian(state, loads, jacobian);
            return loads;
        }

        private static void EvaluateWeldWithJacobian(
            double[] state,
            double[] loads,
            double[,] jacobian) =>
            WeldJoint2DEquation.Evaluate(
                AnchorA,
                ToBodyState(state, 0),
                AnchorB,
                ToBodyState(state, 6),
                14.0,
                0.7,
                -0.18,
                6.0,
                0.4,
                loads,
                jacobian);

        private static double[] EvaluatePrismatic(double[] state)
        {
            var loads = new double[6];
            var jacobian = new double[6, 12];
            EvaluatePrismaticWithJacobian(state, loads, jacobian);
            return loads;
        }

        private static void EvaluatePrismaticWithJacobian(
            double[] state,
            double[] loads,
            double[,] jacobian) =>
            EvaluatePrismaticResult(state, loads, jacobian);

        private static PrismaticJoint2DEvaluation EvaluatePrismaticResult(
            double[] state,
            double[] loads,
            double[,] jacobian) =>
            PrismaticJoint2DEquation.Evaluate(
                AnchorA,
                ToBodyState(state, 0),
                AnchorB,
                ToBodyState(state, 6),
                new Vector2D(0.8, 0.6),
                19.0,
                0.8,
                -0.18,
                6.0,
                0.4,
                0.35,
                3.0,
                0.2,
                loads,
                jacobian);

        private static double[] EvaluatePrismaticElastic(double[] state, out double energy)
        {
            var loads = new double[6];
            var jacobian = new double[6, 12];
            PrismaticJoint2DEvaluation result = PrismaticJoint2DEquation.Evaluate(
                AnchorA,
                ToBodyState(state, 0),
                AnchorB,
                ToBodyState(state, 6),
                new Vector2D(0.8, 0.6),
                19.0,
                0.0,
                -0.18,
                6.0,
                0.0,
                0.35,
                3.0,
                0.0,
                loads,
                jacobian);
            energy = result.StoredElasticEnergy;
            return loads;
        }

        private static ConnectionBodyState2D ToBodyState(double[] state, int offset) =>
            new ConnectionBodyState2D(
                new Vector2D(state[offset], state[offset + 1]),
                state[offset + 2],
                new Vector2D(state[offset + 3], state[offset + 4]),
                state[offset + 5]);
    }
}
