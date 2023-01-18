﻿using System;
using System.Globalization;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Custom;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Model = SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models.Model;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public class SwitchGenerator : ComponentGenerator
    {
        public override IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type.ToLower())
            {
                case "s": return GenerateVoltageSwitch(componentIdentifier, parameters, context);
                case "w": return GenerateCurrentSwitch(componentIdentifier, parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Generates a voltage switch.
        /// </summary>
        /// <param name="name">Name of voltage switch to generate.</param>
        /// <param name="parameters">Parameters for voltage switch.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new voltage switch.
        /// </returns>
        protected IEntity GenerateVoltageSwitch(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 5)
            {
                context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Wrong parameter count for voltage switch", parameters.LineInfo);
                return null;
            }

            string modelName = parameters.Get(4).Value;
            var mParameter = parameters.FirstOrDefault(p => p is AssignmentParameter p1 && p1.Name.ToLower() == "m");

            var model = context.ModelsRegistry.FindModel(modelName);
            if (model.Entity is VSwitchModel)
            {
                BehavioralResistor resistor = new BehavioralResistor(name);
                Model resistorModel = model;
                context.CreateNodes(resistor, parameters.Take(BehavioralResistor.BehavioralResistorPinCount));
                var vSwitchModelParameters = resistorModel.Parameters as VSwitchModelParameters;

                double vOff = vSwitchModelParameters.OffVoltage;
                double rOff = vSwitchModelParameters.OffResistance;
                double vOn = vSwitchModelParameters.OnVoltage;
                double rOn = vSwitchModelParameters.OnResistance;

                string vc = $"v({parameters.Get(2)}, {parameters.Get(3)})";
                double lm = Math.Log(Math.Sqrt(rOn * rOff));
                double lr = Math.Log(rOn / rOff);
                double vm = (vOn + vOff) / 2.0;
                double vd = vOn - vOff;
                string resExpression = GetVSwitchExpression(vOff, rOff, vOn, rOn, vc, lm, lr, vm, vd, mParameter?.Value);

                resistor.Parameters.Expression = resExpression;
                resistor.Parameters.ParseAction = (expression) =>
                {
                    var parser = context.CreateExpressionResolver(null);
                    return parser.Resolve(expression);
                };

                context.SimulationPreparations.ExecuteActionAfterSetup(
                    (simulation) =>
                    {
                        if (context.ModelsRegistry is StochasticModelsRegistry stochasticModelsRegistry)
                        {
                            resistorModel = stochasticModelsRegistry.ProvideStochasticModel(name, simulation, model);

                            if (!context.FindObject(resistorModel.Name, out _))
                            {
                                stochasticModelsRegistry.RegisterModelInstance(resistorModel);
                            }
                        }

                        vOff = vSwitchModelParameters.OffVoltage;
                        rOff = vSwitchModelParameters.OffResistance;
                        vOn = vSwitchModelParameters.OnVoltage;
                        rOn = vSwitchModelParameters.OnResistance;
                        lm = Math.Log(Math.Sqrt(rOn * rOff));
                        lr = Math.Log(rOn / rOff);
                        vm = (vOn + vOff) / 2.0;
                        vd = vOn - vOff;
                        resExpression = GetVSwitchExpression(vOff, rOff, vOn, rOn, vc, lm, lr, vm, vd, mParameter?.Value);
                        resistor.Parameters.Expression = resExpression;
                        resistor.Parameters.ParseAction = (expression) =>
                        {
                            var parser = context.CreateExpressionResolver(simulation);
                            return parser.Resolve(expression);
                        };
                    });
                return resistor;
            }

            VoltageSwitch vsw = new VoltageSwitch(name);
            context.CreateNodes(vsw, parameters);

            context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
            {
                context.ModelsRegistry.SetModel(
                    vsw,
                    simulation,
                    parameters.Get(4),
                    $"Could not find model {parameters.Get(4)} for voltage switch {name}",
                    (model2) => { vsw.Model = model2.Name; },
                    context);
            });

            // Optional ON or OFF
            if (parameters.Count >= 6)
            {
                switch (parameters.Get(5).Value.ToLower())
                {
                    case "on":
                        vsw.SetParameter("on", true);
                        break;

                    case "off":
                        vsw.SetParameter("off", true);
                        break;
                }
            }

            if (mParameter != null)
            {
                context.SetParameter(vsw, "m", mParameter, true);
            }

            return vsw;
        }

        private static string GetISwitchExpression(double iOff, double rOff, double iOn, double rOn, string ic, double lm, double lr, double im, double id, string m)
        {
            string resExpression;
            if (iOn >= iOff)
            {
                resExpression = $"{ic.ToString(CultureInfo.InvariantCulture)} >= {iOn.ToString(CultureInfo.InvariantCulture)} ? {rOn.ToString(CultureInfo.InvariantCulture)} : ({ic.ToString(CultureInfo.InvariantCulture)} <= {iOff.ToString(CultureInfo.InvariantCulture)} ? {rOff.ToString(CultureInfo.InvariantCulture)} : (exp({lm.ToString(CultureInfo.InvariantCulture)} + 3 * {lr.ToString(CultureInfo.InvariantCulture)} * ({ic.ToString(CultureInfo.InvariantCulture)}-{im.ToString(CultureInfo.InvariantCulture)})/(2*{id.ToString(CultureInfo.InvariantCulture)}) - 2 * {lr.ToString(CultureInfo.InvariantCulture)} * pow({ic.ToString(CultureInfo.InvariantCulture)}-{im.ToString(CultureInfo.InvariantCulture)}, 3)/(pow({id.ToString(CultureInfo.InvariantCulture)},3)))))";
            }
            else
            {
                resExpression = $"{ic.ToString(CultureInfo.InvariantCulture)} < {iOn.ToString(CultureInfo.InvariantCulture)} ? {rOn.ToString(CultureInfo.InvariantCulture)} : ({ic.ToString(CultureInfo.InvariantCulture)} > {iOff.ToString(CultureInfo.InvariantCulture)} ? {rOff.ToString(CultureInfo.InvariantCulture)} : (exp({lm.ToString(CultureInfo.InvariantCulture)} - 3 * {lr.ToString(CultureInfo.InvariantCulture)} * ({ic.ToString(CultureInfo.InvariantCulture)}-{im.ToString(CultureInfo.InvariantCulture)})/(2*{id.ToString(CultureInfo.InvariantCulture)}) + 2 * {lr.ToString(CultureInfo.InvariantCulture)} * pow({ic.ToString(CultureInfo.InvariantCulture)}-{im.ToString(CultureInfo.InvariantCulture)}, 3)/(pow({id.ToString(CultureInfo.InvariantCulture)},3)))))";
            }

            return MultiplyIfNeeded(resExpression, m);
        }

        private static string GetVSwitchExpression(double vOff, double rOff, double vOn, double rOn, string vc, double lm, double lr, double vm, double vd, string m)
        {
            string resExpression;
            if (vOn >= vOff)
            {
                resExpression = $"{vc.ToString(CultureInfo.InvariantCulture)} >= {vOn.ToString(CultureInfo.InvariantCulture)} ? {rOn.ToString(CultureInfo.InvariantCulture)} : ({vc.ToString(CultureInfo.InvariantCulture)} <= {vOff.ToString(CultureInfo.InvariantCulture)} ? {rOff.ToString(CultureInfo.InvariantCulture)} : (exp({lm.ToString(CultureInfo.InvariantCulture)} + 3 * {lr.ToString(CultureInfo.InvariantCulture)} * ({vc.ToString(CultureInfo.InvariantCulture)}-{vm.ToString(CultureInfo.InvariantCulture)})/(2*{vd.ToString(CultureInfo.InvariantCulture)}) - 2 * {lr.ToString(CultureInfo.InvariantCulture)} * pow({vc.ToString(CultureInfo.InvariantCulture)}-{vm.ToString(CultureInfo.InvariantCulture)}, 3)/(pow({vd.ToString(CultureInfo.InvariantCulture)},3)))))";
            }
            else
            {
                resExpression = $"{vc.ToString(CultureInfo.InvariantCulture)} < {vOn.ToString(CultureInfo.InvariantCulture)} ? {rOn.ToString(CultureInfo.InvariantCulture)} : ({vc.ToString(CultureInfo.InvariantCulture)} > {vOff.ToString(CultureInfo.InvariantCulture)} ? {rOff.ToString(CultureInfo.InvariantCulture)} : (exp({lm.ToString(CultureInfo.InvariantCulture)} - 3 * {lr.ToString(CultureInfo.InvariantCulture)} * ({vc.ToString(CultureInfo.InvariantCulture)}-{vm.ToString(CultureInfo.InvariantCulture)})/(2*{vd.ToString(CultureInfo.InvariantCulture)}) + 2 * {lr.ToString(CultureInfo.InvariantCulture)} * pow({vc.ToString(CultureInfo.InvariantCulture)}-{vm.ToString(CultureInfo.InvariantCulture)}, 3)/(pow({vd.ToString(CultureInfo.InvariantCulture)},3)))))";
            }

            return MultiplyIfNeeded(resExpression, m);
        }

        /// <summary>
        /// Generates a current switch.
        /// </summary>
        /// <param name="name">Name of current switch.</param>
        /// <param name="parameters">Parameters of current switch.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of current switch.
        /// </returns>
        private IEntity GenerateCurrentSwitch(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 4)
            {
                context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Wrong parameter count for current switch", parameters.LineInfo);
                return null;
            }

            string modelName = parameters.Get(3).Value;
            var model = context.ModelsRegistry.FindModel(modelName);
            var mParameter = parameters.FirstOrDefault(p => p is AssignmentParameter p1 && p1.Name.ToLower() == "m");

            if (model.Entity is ISwitchModel)
            {
                BehavioralResistor resistor = new BehavioralResistor(name);
                Model resistorModel = model;
                context.CreateNodes(resistor, parameters.Take(2));
                var ic = $"i({parameters.Get(2)})";
                var iswitchModelParameters = resistorModel.Parameters as ISwitchModelParameters;

                double iOff = iswitchModelParameters.OffCurrent;
                double rOff = iswitchModelParameters.OffResistance;
                double iOn = iswitchModelParameters.OnCurrent;
                double rOn = iswitchModelParameters.OnResistance;

                double lm = Math.Log(Math.Sqrt(rOn * rOff));
                double lr = Math.Log(rOn / rOff);
                double im = (iOn + iOff) / 2.0;
                double id = iOn - iOff;
                var resExpression = GetISwitchExpression(iOff, rOff, iOn, rOn, ic, lm, lr, im, id, mParameter?.Value);
                resistor.Parameters.Expression = resExpression;
                resistor.Parameters.ParseAction = (expression) =>
                {
                    var parser = context.CreateExpressionResolver(null);
                    return parser.Resolve(expression);
                };

                context.SimulationPreparations.ExecuteActionAfterSetup(
                    (simulation) =>
                    {
                        if (context.ModelsRegistry is StochasticModelsRegistry stochasticModelsRegistry)
                        {
                            resistorModel = stochasticModelsRegistry.ProvideStochasticModel(name, simulation, model);

                            if (!context.FindObject(resistorModel.Name, out _))
                            {
                                stochasticModelsRegistry.RegisterModelInstance(resistorModel);
                            }
                        }

                        iOff = iswitchModelParameters.OffCurrent;
                        rOff = iswitchModelParameters.OffResistance;
                        iOn = iswitchModelParameters.OnCurrent;
                        rOn = iswitchModelParameters.OnResistance;

                        lm = Math.Log(Math.Sqrt(rOn * rOff));
                        lr = Math.Log(rOn / rOff);
                        im = (iOn + iOff) / 2.0;
                        id = iOn - iOff;
                        resExpression = GetISwitchExpression(iOff, rOff, iOn, rOn, ic, lm, lr, im, id, mParameter?.Value);
                        resistor.Parameters.Expression = resExpression;
                        resistor.Parameters.ParseAction = (expression) =>
                        {
                            var parser = context.CreateExpressionResolver(simulation);
                            return parser.Resolve(expression);
                        };
                    });
                return resistor;
            }

            CurrentSwitch csw = new CurrentSwitch(name);
            context.CreateNodes(csw, parameters);

            // Get the controlling voltage source
            if (parameters[2] is WordParameter || parameters[2] is IdentifierParameter)
            {
                csw.ControllingSource = context.NameGenerator.GenerateObjectName(parameters.Get(2).Value);
            }
            else
            {
                context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Voltage source name expected", parameters.LineInfo);
                return null;
            }

            // Get the model
            context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
            {
                context.ModelsRegistry.SetModel(
                    csw,
                    simulation,
                    parameters.Get(3),
                    $"Could not find model {parameters.Get(3)} for current switch {name}",
                    (Context.Models.Model switchModel) => csw.Model = switchModel.Name,
                    context);
            });

            // Optional on or off
            if (parameters.Count > 4)
            {
                switch (parameters.Get(4).Value.ToLower())
                {
                    case "on":
                        csw.SetParameter("on", true);
                        break;

                    case "off":
                        csw.SetParameter("off", true);
                        break;
                }
            }

            if (mParameter != null)
            {
                context.SetParameter(csw, "m", mParameter, true);
            }

            return csw;
        }

        private static string MultiplyIfNeeded(string expression, string mExpression)
        {
            if (!string.IsNullOrEmpty(mExpression))
            {
                return $"({expression} / {mExpression})";
            }

            return expression;
        }
    }
}