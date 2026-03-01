using SpiceSharp.Components;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    /// <summary>
    /// Integration tests for model selection (binning) based on instance parameters
    /// and model selection parameters (e.g. lmin, lmax, wmin, wmax).
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

            // M1 L=0.5u should match NMOS.0 (lmin=0.1u lmax=1u)
            var mosfet1 = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet1);

            // M2 L=5u should match NMOS.1 (lmin=1u lmax=10u)
            var mosfet2 = netlist.Circuit["m2"] as Mosfet1;
            Assert.NotNull(mosfet2);

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

            // M1 W=2u should match PMOS.0 (wmin=1u wmax=10u)
            var mosfet1 = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet1);

            // M2 W=20u should match PMOS.1 (wmin=10u wmax=100u)
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

            // M1 L=0.5u W=5u should match NMOS.0
            var mosfet1 = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet1);

            // M2 L=5u W=50u should match NMOS.1
            var mosfet2 = netlist.Circuit["m2"] as Mosfet1;
            Assert.NotNull(mosfet2);
        }

        [Fact]
        public void MosfetFallsBackToBaseModelWhenNoMatch()
        {
            // L=100u exceeds NMOS.0's lmax=1u, should fall back to base NMOS
            var netlist = GetSpiceSharpModel(
                "MOSFET falls back to base model",
                "M1 D G S B NMOS L=100u W=100u",
                ".model NMOS.0 NMOS level=1 lmin=0.1u lmax=1u",
                ".model NMOS NMOS level=1",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var mosfet = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet);
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
        public void ResistorModelWithOnlyLengthConstraints()
        {
            // Models only constrain L (no wmin/wmax), so W is irrelevant for selection
            var netlist = GetSpiceSharpModel(
                "Resistor model with only length constraints",
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
        public void ResistorModelWithOnlyWidthConstraints()
        {
            // Models only constrain W (no lmin/lmax), so L is irrelevant for selection
            var netlist = GetSpiceSharpModel(
                "Resistor model with only width constraints",
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
        public void BJTModelWithOnlyLengthConstraints()
        {
            // Models only constrain L, component has only L
            var netlist = GetSpiceSharpModel(
                "BJT model with only length constraints",
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
        public void DiodeModelWithOnlyWidthConstraints()
        {
            // Models only constrain W, component has only W
            var netlist = GetSpiceSharpModel(
                "Diode model with only width constraints",
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
        public void JFETModelWithOnlyLengthConstraints()
        {
            // Models only constrain L, component has only L
            var netlist = GetSpiceSharpModel(
                "JFET model with only length constraints",
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

        #region Non-Numeric Model Suffix Tests

        [Fact]
        public void MosfetSelectsModelWithNonNumericSuffix()
        {
            var netlist = GetSpiceSharpModel(
                "MOSFET model selection with non-numeric suffix",
                "M1 D G S B NMOS L=0.5u W=10u",
                "M2 D G S B NMOS L=5u W=10u",
                ".model NMOS.small NMOS level=1 lmin=0.1u lmax=1u",
                ".model NMOS.large NMOS level=1 lmin=1u lmax=10u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var mosfet1 = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet1);

            var mosfet2 = netlist.Circuit["m2"] as Mosfet1;
            Assert.NotNull(mosfet2);

            Assert.NotNull(netlist.Circuit["NMOS.small"]);
            Assert.NotNull(netlist.Circuit["NMOS.large"]);
        }

        [Fact]
        public void MosfetSelectsModelWithArbitrarySuffix()
        {
            var netlist = GetSpiceSharpModel(
                "MOSFET model selection with arbitrary suffix",
                "M1 D G S B NMOS L=0.5u W=10u",
                "M2 D G S B NMOS L=5u W=10u",
                ".model NMOS.hp NMOS level=1 lmin=0.1u lmax=1u",
                ".model NMOS.lp NMOS level=1 lmin=1u lmax=10u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var mosfet1 = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet1);

            var mosfet2 = netlist.Circuit["m2"] as Mosfet1;
            Assert.NotNull(mosfet2);
        }

        [Fact]
        public void ModelWithNonSequentialNumericSuffixes()
        {
            var netlist = GetSpiceSharpModel(
                "Model with non-sequential numeric suffixes",
                "M1 D G S B NMOS L=0.5u W=10u",
                "M2 D G S B NMOS L=5u W=10u",
                ".model NMOS.3 NMOS level=1 lmin=0.1u lmax=1u",
                ".model NMOS.7 NMOS level=1 lmin=1u lmax=10u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var mosfet1 = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet1);

            var mosfet2 = netlist.Circuit["m2"] as Mosfet1;
            Assert.NotNull(mosfet2);
        }

        [Fact]
        public void FallsBackToBaseModelWhenNoSuffixedModelMatches()
        {
            // L=100u exceeds both suffixed models' lmax, should fall back to base NMOS
            var netlist = GetSpiceSharpModel(
                "Falls back to base model when no suffixed model matches",
                "M1 D G S B NMOS L=100u W=100u",
                ".model NMOS.small NMOS level=1 lmin=0.1u lmax=1u",
                ".model NMOS.large NMOS level=1 lmin=1u lmax=10u",
                ".model NMOS NMOS level=1",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var mosfet = netlist.Circuit["m1"] as Mosfet1;
            Assert.NotNull(mosfet);
        }

        [Fact]
        public void DiodeSelectsModelWithNonNumericSuffix()
        {
            var netlist = GetSpiceSharpModel(
                "Diode model selection with non-numeric suffix",
                "D1 A K DMOD L=0.5u W=5u",
                "D2 A K DMOD L=5u W=50u",
                ".model DMOD.fast D lmin=0.1u lmax=1u wmin=1u wmax=10u",
                ".model DMOD.slow D lmin=1u lmax=10u wmin=10u wmax=100u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var diode1 = netlist.Circuit["d1"] as Diode;
            Assert.NotNull(diode1);

            var diode2 = netlist.Circuit["d2"] as Diode;
            Assert.NotNull(diode2);
        }

        [Fact]
        public void ResistorSelectsModelWithNonNumericSuffix()
        {
            var netlist = GetSpiceSharpModel(
                "Resistor model selection with non-numeric suffix",
                "R1 1 0 RMOD L=0.5u W=5u",
                "R2 1 0 RMOD L=5u W=50u",
                ".model RMOD.thin R RSH=1 lmin=0.1u lmax=1u wmin=1u wmax=10u",
                ".model RMOD.wide R RSH=1 lmin=1u lmax=10u wmin=10u wmax=100u",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var resistor1 = netlist.Circuit["r1"] as Resistor;
            Assert.NotNull(resistor1);

            var resistor2 = netlist.Circuit["r2"] as Resistor;
            Assert.NotNull(resistor2);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void MosfetWithoutLWParametersMatchesFirstSuffixedModel()
        {
            // Without L/W, predicate is null — first suffixed model is selected (not base model)
            var netlist = GetSpiceSharpModel(
                "MOSFET without L/W matches first suffixed model",
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
            // RMOD.0 has lmin=1u: R1 L=0.5u fails (below min), falls back to base RMOD
            // R2 L=5u passes (above min), matches RMOD.0
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
            // CMOD.0 has lmax=1u: C1 L=0.5u passes (below max), matches CMOD.0
            // C2 L=5u fails (above max), falls back to base CMOD
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
            // NMOS.0 has wmin=1u: M1 W=0.5u fails (below min), falls back to base NMOS
            // M2 W=5u passes (above min), matches NMOS.0
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
            // QMOD.0 has wmax=10u: Q1 W=5u passes (below max), matches QMOD.0
            // Q2 W=50u fails (above max), falls back to base QMOD
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
            // D1 matches both DMOD.0 and DMOD.1; first match wins
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
