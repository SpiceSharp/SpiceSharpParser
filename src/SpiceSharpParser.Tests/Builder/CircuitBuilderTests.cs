using SpiceSharpParser.Builder;
using Xunit;

namespace SpiceSharpParser.Tests.Builder
{
    public class CircuitBuilderTests
    {
        [Fact]
        public void ToNetlist_SimpleRC_GeneratesValidNetlist()
        {
            var netlist = CircuitBuilder.Create("RC Filter")
                .VoltageSource("V1", "in", "0", dc: 5)
                .Resistor("R1", "in", "out", 1000)
                .Capacitor("C1", "out", "0", 1e-6)
                .OP()
                .Save("V(out)")
                .ToNetlist();

            Assert.Contains("RC Filter", netlist);
            Assert.Contains("V1", netlist);
            Assert.Contains("R1 in out 1k", netlist);
            Assert.Contains("C1 out 0 1u", netlist);
            Assert.Contains(".OP", netlist);
            Assert.Contains(".SAVE V(out)", netlist);
            Assert.Contains(".END", netlist);
        }

        [Fact]
        public void Build_SimpleRC_ParsesSuccessfully()
        {
            var model = CircuitBuilder.Create("RC OP Test")
                .VoltageSource("V1", "in", "0", dc: 10)
                .Resistor("R1", "in", "out", 1000)
                .Resistor("R2", "out", "0", 1000)
                .OP()
                .Save("V(out)")
                .Build();

            Assert.NotNull(model);
            Assert.False(model.ValidationResult.HasError);
            Assert.NotNull(model.Circuit);
            Assert.True(model.Simulations.Count > 0);
        }

        [Fact]
        public void SetValue_ModifiesComponent()
        {
            var builder = CircuitBuilder.Create("Test")
                .VoltageSource("V1", "in", "0", dc: 5)
                .Resistor("R1", "in", "out", 1000)
                .Resistor("R2", "out", "0", 2000);

            var netlistBefore = builder.ToNetlist();
            Assert.Contains("R1 in out 1k", netlistBefore);

            builder.SetValue("R1", 2200);
            var netlistAfter = builder.ToNetlist();
            Assert.Contains("R1 in out 2.2k", netlistAfter);
        }

        [Fact]
        public void RemoveComponent_RemovesFromNetlist()
        {
            var builder = CircuitBuilder.Create("Test")
                .VoltageSource("V1", "in", "0", dc: 5)
                .Resistor("R1", "in", "out", 1000)
                .Resistor("R2", "out", "0", 2000);

            builder.RemoveComponent("R1");
            var netlist = builder.ToNetlist();
            Assert.DoesNotContain("R1", netlist);
            Assert.Contains("R2", netlist);
        }

        [Fact]
        public void AC_GeneratesCorrectStatement()
        {
            var netlist = CircuitBuilder.Create("AC Test")
                .VoltageSource("V1", "in", "0", ac: 1)
                .Resistor("R1", "in", "out", 1000)
                .Capacitor("C1", "out", "0", 1e-9)
                .AC("DEC", 10, 1, 1e6)
                .Save("VDB(out)")
                .ToNetlist();

            Assert.Contains(".AC DEC 10 1 1MEG", netlist);
        }

        [Fact]
        public void Tran_WithMaxStep_GeneratesCorrectStatement()
        {
            var netlist = CircuitBuilder.Create("Tran Test")
                .VoltageSourceSine("V1", "in", "0", 0, 1, 1000)
                .Resistor("R1", "in", "out", 1000)
                .Capacitor("C1", "out", "0", 1e-6)
                .Tran(1e-6, 1e-3, maxStep: 1e-5)
                .Save("V(out)")
                .ToNetlist();

            Assert.Contains(".TRAN 1u 1m 0 10u", netlist);
        }

        [Fact]
        public void BJT_GeneratesCorrectLine()
        {
            var netlist = CircuitBuilder.Create("BJT Test")
                .VoltageSource("VCC", "vcc", "0", dc: 12)
                .Resistor("RC", "vcc", "out", 4700)
                .Resistor("RB", "vcc", "base", 100000)
                .BJT("Q1", "out", "base", "0", "2N3904")
                .ModelRaw(".MODEL 2N3904 NPN(BF=200 IS=1e-14)")
                .OP()
                .Save("V(out)")
                .ToNetlist();

            Assert.Contains("Q1 out base 0 2N3904", netlist);
            Assert.Contains(".MODEL 2N3904 NPN(BF=200 IS=1e-14)", netlist);
        }

        [Fact]
        public void Meas_GeneratesCorrectStatement()
        {
            var netlist = CircuitBuilder.Create("Meas Test")
                .VoltageSource("V1", "in", "0", ac: 1)
                .Resistor("R1", "in", "out", 1000)
                .Capacitor("C1", "out", "0", 1e-9)
                .AC("DEC", 100, 1, 1e9)
                .Save("VDB(out)")
                .Meas("AC", "f3dB", "WHEN VDB(out) = -3")
                .ToNetlist();

            Assert.Contains(".MEAS AC f3dB WHEN VDB(out) = -3", netlist);
        }
    }
}
