using SpiceSharpParser.ModelReaders.Netlist.Spice;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Simulations;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Stochastic
{
    public class RandomTests : BaseTests
    {
        [Fact]
        public void Basic()
        {
            var result = GetSpiceSharpModel(
                "Random - Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R+N}",
                ".OP",
                ".SAVE i(R1) @R1[i] @R1[v]",
                ".PARAM N=0",
                ".PARAM R={random() * 1000}",
                ".STEP PARAM N LIST 2 3",
                ".END");

            Assert.Equal(6, result.Exports.Count);
            Assert.Equal(2, result.Simulations.Count);

            RunSimulationsAndReturnExports(result);
        }

        [Fact]
        public void OptionsSeed()
        {
            SpiceSharpModel parseResult2223 = null;
            SpiceSharpModel parseResult2224 = null;

            var resultsSeed2223 = new List<double>();
            var resultsSeed2224 = new List<double>();

            int n = 2;

            for (var i = 0; i < n; i++)
            {
                parseResult2223 = GetSpiceSharpModel(
                    "Random - Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE @R1[i]",
                    ".PARAM N=0",
                    ".PARAM R={random() * 1000}",
                    ".STEP PARAM N LIST 2 3",
                    ".OPTIONS SEED = 2223",
                    ".END");

                var exports = RunSimulationsAndReturnExports(parseResult2223);
                resultsSeed2223.Add((double)exports[0]);
            }

            for (var i = 0; i < n; i++)
            {
                parseResult2224 = GetSpiceSharpModel(
                    "Random - Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE @R1[i]",
                    ".PARAM N=0",
                    ".PARAM R={random() * 1000}",
                    ".STEP PARAM N LIST 2 3",
                    ".OPTIONS SEED = 2224",
                    ".END");

                var exports = RunSimulationsAndReturnExports(parseResult2224);
                resultsSeed2224.Add((double)exports[0]);
            }

            for (var i = 0; i < n; i++)
            {
                Assert.Equal(resultsSeed2223[0], resultsSeed2223[i]);
            }

            for (var i = 0; i < n; i++)
            {
                Assert.Equal(resultsSeed2224[0], resultsSeed2224[i]);
            }

            Assert.NotEqual(resultsSeed2224[0], resultsSeed2223[0]);

            Assert.Equal(2223, parseResult2223?.Seed);
            Assert.Equal(2224, parseResult2224?.Seed);
        }

        [Fact]
        public void OptionsSeedOverridesParsingSeed()
        {
            SpiceSharpModel parseResult2223 = null;
            SpiceSharpModel parseResult2224 = null;

            var resultsSeed2223 = new List<double>();
            var resultsSeed2224 = new List<double>();

            int n = 5;

            for (var i = 0; i < n; i++)
            {
                parseResult2223 = GetSpiceSharpModel(
                    1111,
                    "Random - Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE i(R1) @R1[i]",
                    ".PARAM N=0",
                    ".PARAM R={random() * 1000}",
                    ".STEP PARAM N LIST 2 3",
                    ".OPTIONS SEED = 2223",
                    ".END");

                var exports = RunSimulationsAndReturnExports(parseResult2223);
                resultsSeed2223.Add((double)exports[0]);
            }

            for (var i = 0; i < n; i++)
            {
                parseResult2224 = GetSpiceSharpModel(
                    1111,
                    "Random - Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE i(R1) @R1[i]",
                    ".PARAM N=0",
                    ".PARAM R={random() * 1000}",
                    ".STEP PARAM N LIST 2 3",
                    ".OPTIONS SEED = 2224",
                    ".END");

                var exports = RunSimulationsAndReturnExports(parseResult2224);
                resultsSeed2224.Add((double)exports[0]);
            }

            for (var i = 0; i < n; i++)
            {
                Assert.Equal(resultsSeed2223[0], resultsSeed2223[i]);
            }

            for (var i = 0; i < n; i++)
            {
                Assert.Equal(resultsSeed2224[0], resultsSeed2224[i]);
            }

            Assert.NotEqual(resultsSeed2224[0], resultsSeed2223[0]);

            Assert.Equal(2223, parseResult2223?.Seed);
            Assert.Equal(2224, parseResult2224?.Seed);
        }

        [Fact]
        public void ParsingSeed()
        {
            for (var index = 0; index < 12; index++)
            {
                SpiceSharpModel parseResult2223 = null;
                SpiceSharpModel parseResult2224 = null;

                var resultsSeed2223 = new List<double>();
                var resultsSeed2224 = new List<double>();

                int n = 10;

                for (var i = 0; i < n; i++)
                {
                    parseResult2223 = GetSpiceSharpModel(
                        2223,
                        "Random - Seed test circuit",
                        "V1 0 1 100",
                        "R1 1 0 {R+N}",
                        ".OP",
                        ".SAVE i(R1) @R1[i]",
                        ".PARAM N=0",
                        ".PARAM R={random() * 1000}",
                        ".STEP PARAM N LIST 2 3",
                        ".END");

                    var exports = RunSimulationsAndReturnExports(parseResult2223);
                    resultsSeed2223.Add((double)exports[0]);
                }

                for (var i = 0; i < n; i++)
                {
                    parseResult2224 = GetSpiceSharpModel(
                        2224,
                        "Random - Seed test circuit",
                        "V1 0 1 100",
                        "R1 1 0 {R+N}",
                        ".OP",
                        ".SAVE i(R1) @R1[i]",
                        ".PARAM N=0",
                        ".PARAM R={random() * 1000}",
                        ".STEP PARAM N LIST 2 3",
                        ".END");

                    var exports = RunSimulationsAndReturnExports(parseResult2224);
                    resultsSeed2224.Add((double)exports[0]);
                }

                for (var i = 0; i < n; i++)
                {
                    Assert.Equal(resultsSeed2223[0], resultsSeed2223[i]);
                }

                for (var i = 0; i < n; i++)
                {
                    Assert.Equal(resultsSeed2224[0], resultsSeed2224[i]);
                }

                Assert.NotEqual(resultsSeed2224[0], resultsSeed2223[0]);

                Assert.Equal(2223, parseResult2223?.Seed);
                Assert.Equal(2224, parseResult2224?.Seed);
            }
        }
    }
}