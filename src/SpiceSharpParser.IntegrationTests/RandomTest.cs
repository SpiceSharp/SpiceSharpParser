using System.Collections.Generic;
using SpiceSharpParser.ModelsReaders.Netlist.Spice;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class RandomTest : BaseTest
    {

        [Fact]
        public void BasicTest()
        {
            var result = ParseNetlist(
                "Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R+N}",
                ".OP",
                ".SAVE i(R1) @R1[resistance]",
                ".PARAM N=0",
                ".PARAM R={random() * 1000}",
                ".STEP PARAM N LIST 2 3",
                ".END");

            Assert.Equal(4, result.Exports.Count);
            Assert.Equal(2, result.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(result);
        }

        [Fact]
        public void OptionsSeedTest()
        {
            SpiceNetlistReaderResult parseResult2223 = null;
            SpiceNetlistReaderResult parseResult2224 = null;

            var resultsSeed2223 = new List<double>();
            var resultsSeed2224 = new List<double>();

            int n = 10;

            for (var i = 0; i < n; i++)
            {
                parseResult2223 = ParseNetlist(
                    "Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE i(R1) @R1[resistance]",
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
                parseResult2224 = ParseNetlist(
                    "Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE i(R1) @R1[resistance]",
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

            Assert.Equal(2223, parseResult2223.UsedRandomSeed);
            Assert.Equal(2224, parseResult2224.UsedRandomSeed);
        }

        [Fact]
        public void OptionsSeedOverridesParsingSeedTest()
        {
            SpiceNetlistReaderResult parseResult2223 = null;
            SpiceNetlistReaderResult parseResult2224 = null;

            var resultsSeed2223 = new List<double>();
            var resultsSeed2224 = new List<double>();

            int n = 10;

            for (var i = 0; i < n; i++)
            {
                parseResult2223 = ParseNetlist(
                    1111,
                    "Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE i(R1) @R1[resistance]",
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
                parseResult2224 = ParseNetlist(
                    1111,
                    "Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE i(R1) @R1[resistance]",
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

            Assert.Equal(2223, parseResult2223.UsedRandomSeed);
            Assert.Equal(2224, parseResult2224.UsedRandomSeed);
        }

        [Fact]
        public void ParsingSeedTest()
        {
            SpiceNetlistReaderResult parseResult2223 = null;
            SpiceNetlistReaderResult parseResult2224 = null;

            var resultsSeed2223 = new List<double>();
            var resultsSeed2224 = new List<double>();

            int n = 10;

            for (var i = 0; i < n; i++)
            {
                parseResult2223 = ParseNetlist(
                    2223,
                    "Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE i(R1) @R1[resistance]",
                    ".PARAM N=0",
                    ".PARAM R={random() * 1000}",
                    ".STEP PARAM N LIST 2 3",
                    ".END");

                var exports = RunSimulationsAndReturnExports(parseResult2223);
                resultsSeed2223.Add((double)exports[0]);
            }

            for (var i = 0; i < n; i++)
            {
                parseResult2224 = ParseNetlist(
                    2224,
                    "Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE i(R1) @R1[resistance]",
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

            Assert.Equal(2223, parseResult2223.UsedRandomSeed);
            Assert.Equal(2224, parseResult2224.UsedRandomSeed);
        }
    }
}
