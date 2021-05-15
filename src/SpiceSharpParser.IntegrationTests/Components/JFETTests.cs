using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class JFETTests : BaseTests
    {
        [Fact]
        public void IsAbleToParse()
        {
            var netlist = GetSpiceSharpModel(
               "JFET parse test circuit",
               "J1 D G S JModel1",
               "J2 D G S JModel2 off",
               "J3 D G S JModel2 12.3",
               "J4 D G S JModel2 area = 12.3",
               "J5 D G S JModel2 12.3 off",
               "J6 D G S JModel2 12.3 off IC=1.2,1.3 temp=123",
               "J7 D G S JModel2 temp = 123",
               "J8 D G S JModel2 IC=1.2,1.3",
               "J9 D G S JModel2 IC=1.2,1.3 m = 11",
               ".model JModel1 NJF",
               ".model JModel2 PJF",
               ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);
        }
    }
}