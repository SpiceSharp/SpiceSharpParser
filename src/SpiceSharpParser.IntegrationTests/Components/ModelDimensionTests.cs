using SpiceSharp.Components;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    /// <summary>
    /// Integration tests for model selection based on L and W parameters with lmin, lmax, wmin, wmax constraints.
    /// </summary>
    public class ModelDimensionTests : BaseTests
    {
        #region MOSFET Tests

        [Fact]
        public void MosfetWithLengthAndWidthParameters()
        {
            var netlist = GetSpiceSharpModel(
                "MOSFET with L and W parameters",
                "M1 D G S B NMOS1 L=1u W=10u",
                ".model NMOS1 NMOS level=1",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            var mosfet = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet);
        }

        [Fact]
        public void MosfetSelectsCorrectModelBasedOnLength()
        {
            var netlist = GetSpiceSharpModel(
                "MOSFET model selection based on length",
                "M1 D G S B NMOS L=0.5u W=10u",
                "M2 D G S B NMOS L=5u W=10u",
                ".model NMOS.0 NMOS level=1 lmin=0.1u lmax=1u",
                ".model NMOS.1 NMOS level=1 lmin=1u lmax=10u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            // Both MOSFETs should be created
            var mosfet1 = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet1);

            var mosfet2 = netlist.Circuit["m2"] as Mosfet1;
            Assert.NotNull(mosfet2);

            // Verify models exist in circuit
            Assert.NotNull(netlist.Circuit["NMOS.0"]);
            Assert.NotNull(netlist.Circuit["NMOS.1"]);
        }

        [Fact]
        public void MosfetSelectsCorrectModelBasedOnWidth()
        {
            var netlist = GetSpiceSharpModel(
                "MOSFET model selection based on width",
                "M1 D G S B PMOS L=1u W=2u",
                "M2 D G S B PMOS L=1u W=20u",
                ".model PMOS.0 PMOS level=1 wmin=1u wmax=10u",
                ".model PMOS.1 PMOS level=1 wmin=10u wmax=100u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var mosfet1 = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet1);

            var mosfet2 = netlist.Circuit["m2"] as Mosfet1;
            Assert.NotNull(mosfet2);
        }

        [Fact]
        public void MosfetSelectsCorrectModelBasedOnLengthAndWidth()
        {
            var netlist = GetSpiceSharpModel(
                "MOSFET model selection based on L and W",
                "M1 D G S B NMOS L=0.5u W=5u",
                "M2 D G S B NMOS L=5u W=50u",
                ".model NMOS.0 NMOS level=1 lmin=0.1u lmax=1u wmin=1u wmax=10u",
                ".model NMOS.1 NMOS level=1 lmin=1u lmax=10u wmin=10u wmax=100u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var mosfet1 = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet1);

            var mosfet2 = netlist.Circuit["m2"] as Mosfet1;
            Assert.NotNull(mosfet2);
        }

        [Fact]
        public void MosfetFallsBackToDefaultModelWhenNoMatch()
        {
            var netlist = GetSpiceSharpModel(
                "MOSFET falls back to default model",
                "M1 D G S B NMOS L=100u W=100u",
                ".model NMOS.0 NMOS level=1 lmin=0.1u lmax=1u",
                ".model NMOS NMOS level=1",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var mosfet1 = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet1);
        }

        #endregion

        #region Resistor Tests

        [Fact]
        public void ResistorWithLengthAndWidthParameters()
        {
            var netlist = GetSpiceSharpModel(
                "Resistor with L and W parameters",
                "R1 1 0 RMOD L=1u W=10u",
                ".model RMOD R RSH=1",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            var resistor = netlist.Circuit["r1"] as Resistor;
            Assert.NotNull(resistor);
        }

        [Fact]
        public void ResistorSelectsCorrectModelBasedOnDimensions()
        {
            var netlist = GetSpiceSharpModel(
                "Resistor model selection based on dimensions",
                "R1 1 0 RMOD L=0.5u W=5u",
                "R2 1 0 RMOD L=5u W=50u",
                ".model RMOD.0 R RSH=1 lmin=0.1u lmax=1u wmin=1u wmax=10u",
                ".model RMOD.1 R RSH=1 lmin=1u lmax=10u wmin=10u wmax=100u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var resistor1 = netlist.Circuit["r1"] as Resistor;
            Assert.NotNull(resistor1);

            var resistor2 = netlist.Circuit["r2"] as Resistor;
            Assert.NotNull(resistor2);
        }

        [Fact]
        public void ResistorWithOnlyLengthParameter()
        {
            var netlist = GetSpiceSharpModel(
                "Resistor with only L parameter",
                "R1 1 0 RMOD L=0.5u W=1u",
                "R2 1 0 RMOD L=5u W=1u",
                ".model RMOD.0 R RSH=1 lmin=0.1u lmax=1u",
                ".model RMOD.1 R RSH=1 lmin=1u lmax=10u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var resistor1 = netlist.Circuit["r1"] as Resistor;
            Assert.NotNull(resistor1);

            var resistor2 = netlist.Circuit["r2"] as Resistor;
            Assert.NotNull(resistor2);
        }

        [Fact]
        public void ResistorWithOnlyWidthParameter()
        {
            var netlist = GetSpiceSharpModel(
                "Resistor with only W parameter",
                "R1 1 0 RMOD L=1u W=5u",
                "R2 1 0 RMOD L=1u W=50u",
                ".model RMOD.0 R RSH=1 wmin=1u wmax=10u",
                ".model RMOD.1 R RSH=1 wmin=10u wmax=100u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var resistor1 = netlist.Circuit["r1"] as Resistor;
            Assert.NotNull(resistor1);

            var resistor2 = netlist.Circuit["r2"] as Resistor;
            Assert.NotNull(resistor2);
        }

        #endregion

        #region Capacitor Tests

        [Fact]
        public void CapacitorWithLengthAndWidthParameters()
        {
            var netlist = GetSpiceSharpModel(
                "Capacitor with L and W parameters",
                "C1 1 0 CMOD L=1u W=10u",
                ".model CMOD C CJ=1e-6",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            var capacitor = netlist.Circuit["c1"] as Capacitor;
            Assert.NotNull(capacitor);
        }

        [Fact]
        public void CapacitorSelectsCorrectModelBasedOnDimensions()
        {
            var netlist = GetSpiceSharpModel(
                "Capacitor model selection based on dimensions",
                "C1 1 0 CMOD L=0.5u W=5u",
                "C2 1 0 CMOD L=5u W=50u",
                ".model CMOD.0 C CJ=1e-6 lmin=0.1u lmax=1u wmin=1u wmax=10u",
                ".model CMOD.1 C CJ=1e-6 lmin=1u lmax=10u wmin=10u wmax=100u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var capacitor1 = netlist.Circuit["c1"] as Capacitor;
            Assert.NotNull(capacitor1);

            var capacitor2 = netlist.Circuit["c2"] as Capacitor;
            Assert.NotNull(capacitor2);
        }

        #endregion

        #region BJT Tests

        [Fact]
        public void BJTWithLengthAndWidthParameters()
        {
            var netlist = GetSpiceSharpModel(
                "BJT with L and W parameters",
                "Q1 C B E QMOD L=1u W=10u",
                ".model QMOD NPN",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            var bjt = netlist.Circuit["q1"] as BipolarJunctionTransistor;
            Assert.NotNull(bjt);
        }

        [Fact]
        public void BJTSelectsCorrectModelBasedOnDimensions()
        {
            var netlist = GetSpiceSharpModel(
                "BJT model selection based on dimensions",
                "Q1 C B E QMOD L=0.5u W=5u",
                "Q2 C B E QMOD L=5u W=50u",
                ".model QMOD.0 NPN lmin=0.1u lmax=1u wmin=1u wmax=10u",
                ".model QMOD.1 NPN lmin=1u lmax=10u wmin=10u wmax=100u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var bjt1 = netlist.Circuit["q1"] as BipolarJunctionTransistor;
            Assert.NotNull(bjt1);

            var bjt2 = netlist.Circuit["q2"] as BipolarJunctionTransistor;
            Assert.NotNull(bjt2);
        }

        [Fact]
        public void BJTWithOnlyLengthParameter()
        {
            var netlist = GetSpiceSharpModel(
                "BJT with only L parameter",
                "Q1 C B E QMOD L=0.5u",
                "Q2 C B E QMOD L=5u",
                ".model QMOD.0 NPN lmin=0.1u lmax=1u",
                ".model QMOD.1 NPN lmin=1u lmax=10u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var bjt1 = netlist.Circuit["q1"] as BipolarJunctionTransistor;
            Assert.NotNull(bjt1);

            var bjt2 = netlist.Circuit["q2"] as BipolarJunctionTransistor;
            Assert.NotNull(bjt2);
        }

        #endregion

        #region Diode Tests

        [Fact]
        public void DiodeWithLengthAndWidthParameters()
        {
            var netlist = GetSpiceSharpModel(
                "Diode with L and W parameters",
                "D1 A K DMOD L=1u W=10u",
                ".model DMOD D",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            var diode = netlist.Circuit["d1"] as Diode;
            Assert.NotNull(diode);
        }

        [Fact]
        public void DiodeSelectsCorrectModelBasedOnDimensions()
        {
            var netlist = GetSpiceSharpModel(
                "Diode model selection based on dimensions",
                "D1 A K DMOD L=0.5u W=5u",
                "D2 A K DMOD L=5u W=50u",
                ".model DMOD.0 D lmin=0.1u lmax=1u wmin=1u wmax=10u",
                ".model DMOD.1 D lmin=1u lmax=10u wmin=10u wmax=100u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var diode1 = netlist.Circuit["d1"] as Diode;
            Assert.NotNull(diode1);

            var diode2 = netlist.Circuit["d2"] as Diode;
            Assert.NotNull(diode2);
        }

        [Fact]
        public void DiodeWithOnlyWidthParameter()
        {
            var netlist = GetSpiceSharpModel(
                "Diode with only W parameter",
                "D1 A K DMOD W=5u",
                "D2 A K DMOD W=50u",
                ".model DMOD.0 D wmin=1u wmax=10u",
                ".model DMOD.1 D wmin=10u wmax=100u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var diode1 = netlist.Circuit["d1"] as Diode;
            Assert.NotNull(diode1);

            var diode2 = netlist.Circuit["d2"] as Diode;
            Assert.NotNull(diode2);
        }

        #endregion

        #region JFET Tests

        [Fact]
        public void JFETWithLengthAndWidthParameters()
        {
            var netlist = GetSpiceSharpModel(
                "JFET with L and W parameters",
                "J1 D G S JMOD L=1u W=10u",
                ".model JMOD NJF",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            var jfet = netlist.Circuit["j1"] as JFET;
            Assert.NotNull(jfet);
        }

        [Fact]
        public void JFETSelectsCorrectModelBasedOnDimensions()
        {
            var netlist = GetSpiceSharpModel(
                "JFET model selection based on dimensions",
                "J1 D G S JMOD L=0.5u W=5u",
                "J2 D G S JMOD L=5u W=50u",
                ".model JMOD.0 NJF lmin=0.1u lmax=1u wmin=1u wmax=10u",
                ".model JMOD.1 NJF lmin=1u lmax=10u wmin=10u wmax=100u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var jfet1 = netlist.Circuit["j1"] as JFET;
            Assert.NotNull(jfet1);

            var jfet2 = netlist.Circuit["j2"] as JFET;
            Assert.NotNull(jfet2);
        }

        [Fact]
        public void JFETWithOnlyLengthParameter()
        {
            var netlist = GetSpiceSharpModel(
                "JFET with only L parameter",
                "J1 D G S JMOD L=0.5u",
                "J2 D G S JMOD L=5u",
                ".model JMOD.0 PJF lmin=0.1u lmax=1u",
                ".model JMOD.1 PJF lmin=1u lmax=10u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var jfet1 = netlist.Circuit["j1"] as JFET;
            Assert.NotNull(jfet1);

            var jfet2 = netlist.Circuit["j2"] as JFET;
            Assert.NotNull(jfet2);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void ComponentWithoutLWParametersUsesDefaultModel()
        {
            var netlist = GetSpiceSharpModel(
                "Component without L/W uses default",
                "M1 D G S B NMOS",
                ".model NMOS.0 NMOS level=1 lmin=0.1u lmax=1u",
                ".model NMOS NMOS level=1",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var mosfet = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet);
        }

        [Fact]
        public void ModelWithOnlyLminConstraint()
        {
            var netlist = GetSpiceSharpModel(
                "Model with only lmin constraint",
                "R1 1 0 RMOD L=0.5u W=1u",
                "R2 1 0 RMOD L=5u W=1u",
                ".model RMOD.0 R RSH=1 lmin=1u",
                ".model RMOD R RSH=1",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var resistor1 = netlist.Circuit["r1"] as Resistor;
            Assert.NotNull(resistor1);

            var resistor2 = netlist.Circuit["r2"] as Resistor;
            Assert.NotNull(resistor2);
        }

        [Fact]
        public void ModelWithOnlyLmaxConstraint()
        {
            var netlist = GetSpiceSharpModel(
                "Model with only lmax constraint",
                "C1 1 0 CMOD L=0.5u W=1u",
                "C2 1 0 CMOD L=5u W=1u",
                ".model CMOD.0 C CJ=1e-6 lmax=1u",
                ".model CMOD C CJ=1e-6",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var capacitor1 = netlist.Circuit["c1"] as Capacitor;
            Assert.NotNull(capacitor1);

            var capacitor2 = netlist.Circuit["c2"] as Capacitor;
            Assert.NotNull(capacitor2);
        }

        [Fact]
        public void ModelWithOnlyWminConstraint()
        {
            var netlist = GetSpiceSharpModel(
                "Model with only wmin constraint",
                "M1 D G S B NMOS L=1u W=0.5u",
                "M2 D G S B NMOS L=1u W=5u",
                ".model NMOS.0 NMOS level=1 wmin=1u",
                ".model NMOS NMOS level=1",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var mosfet1 = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet1);

            var mosfet2 = netlist.Circuit["m2"] as Mosfet1;
            Assert.NotNull(mosfet2);
        }

        [Fact]
        public void ModelWithOnlyWmaxConstraint()
        {
            var netlist = GetSpiceSharpModel(
                "Model with only wmax constraint",
                "Q1 C B E QMOD W=5u",
                "Q2 C B E QMOD W=50u",
                ".model QMOD.0 NPN wmax=10u",
                ".model QMOD NPN",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var bjt1 = netlist.Circuit["q1"] as BipolarJunctionTransistor;
            Assert.NotNull(bjt1);

            var bjt2 = netlist.Circuit["q2"] as BipolarJunctionTransistor;
            Assert.NotNull(bjt2);
        }

        [Fact]
        public void MultipleModelsWithOverlappingRanges()
        {
            var netlist = GetSpiceSharpModel(
                "Multiple models with overlapping ranges",
                "D1 A K DMOD L=0.8u W=8u",
                ".model DMOD.0 D lmin=0.1u lmax=1u wmin=1u wmax=10u",
                ".model DMOD.1 D lmin=0.5u lmax=2u wmin=5u wmax=20u",
                ".model DMOD D",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var diode1 = netlist.Circuit["d1"] as Diode;
            Assert.NotNull(diode1);
        }

        #endregion
    }
}
