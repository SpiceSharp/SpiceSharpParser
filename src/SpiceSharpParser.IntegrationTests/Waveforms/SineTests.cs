using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Waveforms
{
    public class SineTests : BaseTests
    {
        [Theory]
        [InlineData(0, 5, 50, 0, 0, 90)]
        public void Test01(double offset, double amplitude, double frequency, double delay, double theta, double phase)
        {
            var netlist = GetSpiceSharpModel(
                "Sine tests",
                $"V1 1 0 SINE({offset} {amplitude} {frequency} {delay} {theta} {phase})",
                ".SAVE V(1)",
                ".TRAN 1e-8 1e-5",
                ".END");
            var simulation = netlist.Simulations.First(s => s is Transient);

            frequency = 2.0 * Math.PI;
            phase = phase * Math.PI / 180;

            simulation.ExportSimulationData += (sender, args) =>
            {
                var time = args.Time;
                time -= delay;

                // Calculate sine wave result (no offset)
                double result;
                if (time <= 0.0)
                    result = 0.0;
                else
                    result = amplitude * Math.Sin(frequency * time + phase);

                // Modify with theta
                if (!theta.Equals(0.0))
                    result *= Math.Exp(-time * theta);

                // Return result (with offset)
                var expected = offset + result;
                Assert.True(EqualsWithTol(expected, args.GetVoltage("1")));
            };

            simulation.Run(netlist.Circuit);

        }

        [Theory]
        [InlineData(0, 5, 50, 0, 0, 90)]
        public void Test02(double offset, double amplitude, double frequency, double delay, double theta, double phase)
        {
            var netlist = GetSpiceSharpModel(
                "Sine tests",
                $"V1 1 0 SINE({offset}, {amplitude}, {frequency}, {delay}, {theta}, {phase})",
                ".SAVE V(1)",
                ".TRAN 1e-8 1e-5",
                ".END");
            var simulation = netlist.Simulations.First(s => s is Transient);

            frequency = 2.0 * Math.PI;
            phase = phase * Math.PI / 180;

            simulation.ExportSimulationData += (sender, args) =>
            {
                var time = args.Time;
                time -= delay;

                // Calculate sine wave result (no offset)
                double result;
                if (time <= 0.0)
                    result = 0.0;
                else
                    result = amplitude * Math.Sin(frequency * time + phase);

                // Modify with theta
                if (!theta.Equals(0.0))
                    result *= Math.Exp(-time * theta);

                // Return result (with offset)
                var expected = offset + result;
                Assert.True(EqualsWithTol(expected, args.GetVoltage("1")));
            };

            simulation.Run(netlist.Circuit);

        }
    }
}
