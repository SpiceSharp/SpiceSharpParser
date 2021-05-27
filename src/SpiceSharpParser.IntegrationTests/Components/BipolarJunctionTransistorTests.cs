using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class BipolarJunctionTransistorTests : BaseTests
    {
        [Fact]
        public void ParseNetlistTest()
        {
            var netlist = GetSpiceSharpModel(
             "BJT parse test circuit",
             "Q23 10 24 13 QMOD IC=0.6, 5.0",
             "Q24 10 24 13 QMOD IC=0.6, 5.0 temp=1",
             "Q25 10 24 13 QMOD temp=1",
             "Q26 10 24 13 QMOD 1.34 IC=1",
             "Q27 10 24 13 QMOD 1.34 IC=1 m=2",
             "Q28 10 24 13 QMOD 1.34 IC=1 230 m=2",
             ".MODEL QMOD NPN(BF=200 CJC=20pf CJE=20pf IS=1E-16)",
             ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);
        }
    }
}
