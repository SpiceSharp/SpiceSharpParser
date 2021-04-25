using System;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Custom;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.Parsers.Expression;
using Model = SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models.Model;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public class SwitchGenerator : ComponentGenerator
    {
        public override IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
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
        protected IEntity GenerateVoltageSwitch(string name, ParameterCollection parameters, ICircuitContext context)
        {
            if (parameters.Count < 5)
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Wrong parameter count for voltage switch", parameters.LineInfo));
                return null;
            }

            string modelName = parameters.Get(4).Image;

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
                string resExpression = GetVSwitchExpression(vOff, rOff, vOn, rOn, vc, lm, lr, vm, vd);

                resistor.Parameters.Expression = resExpression;
                resistor.Parameters.ParseAction = (expression) =>
                {
                    var parser = new ExpressionParser(context.Evaluator.GetEvaluationContext(null), false, context.CaseSensitivity);
                    return parser.Resolve(expression);
                };

                context.SimulationPreparations.ExecuteActionAfterSetup(
                    (simulation) =>
                    {
                        if (context.ModelsRegistry is StochasticModelsRegistry stochasticModelsRegistry)
                        {
                            resistorModel = stochasticModelsRegistry.ProvideStochasticModel(name, simulation, model);

                            if (!context.Result.FindObject(resistorModel.Name, out _))
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
                        resExpression = GetVSwitchExpression(vOff, rOff, vOn, rOn, vc, lm, lr, vm, vd);
                        resistor.Parameters.Expression = resExpression;
                        resistor.Parameters.ParseAction = (expression) =>
                        {
                            var parser = new ExpressionParser(context.Evaluator.GetEvaluationContext(simulation), false, context.CaseSensitivity);
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
                    (Context.Models.Model model2) => { vsw.Model = model2.Name; },
                    context.Result);
            });

            // Optional ON or OFF
            if (parameters.Count == 6)
            {
                switch (parameters.Get(5).Image.ToLower())
                {
                    case "on":
                        vsw.SetParameter("on", true);
                        break;

                    case "off":
                        vsw.SetParameter("off", true);
                        break;

                    default:
                        context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "ON or OFF expected", parameters.LineInfo));
                        return vsw;
                }
            }
            else if (parameters.Count > 6)
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Too many parameters for voltage switch", parameters.LineInfo));
                return vsw;
            }

            return vsw;
        }

        private static string GetISwitchExpression(double iOff, double rOff, double iOn, double rOn, string ic, double lm, double lr, double im, double id)
        {
            string resExpression;
            if (iOn >= iOff)
            {
                resExpression = $"{ic} >= {iOn} ? {rOn} : ({ic} <= {iOff} ? {rOff} : (exp({lm} + 3 * {lr} * ({ic}-{im})/(2*{id}) - 2 * {lr} * pow({ic}-{im}, 3)/(pow({id},3)))))";
            }
            else
            {
                resExpression = $"{ic} < {iOn} ? {rOn} : ({ic} > {iOff} ? {rOff} : (exp({lm} - 3 * {lr} * ({ic}-{im})/(2*{id}) + 2 * {lr} * pow({ic}-{im}, 3)/(pow({id},3)))))";
            }

            return resExpression;
        }

        private static string GetVSwitchExpression(double vOff, double rOff, double vOn, double rOn, string vc, double lm, double lr, double vm, double vd)
        {
            string resExpression;
            if (vOn >= vOff)
            {
                resExpression = $"{vc} >= {vOn} ? {rOn} : ({vc} <= {vOff} ? {rOff} : (exp({lm} + 3 * {lr} * ({vc}-{vm})/(2*{vd}) - 2 * {lr} * pow({vc}-{vm}, 3)/(pow({vd},3)))))";
            }
            else
            {
                resExpression = $"{vc} < {vOn} ? {rOn} : ({vc} > {vOff} ? {rOff} : (exp({lm} - 3 * {lr} * ({vc}-{vm})/(2*{vd}) + 2 * {lr} * pow({vc}-{vm}, 3)/(pow({vd},3)))))";
            }

            return resExpression;
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
        private IEntity GenerateCurrentSwitch(string name, ParameterCollection parameters, ICircuitContext context)
        {
            if (parameters.Count < 4)
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Wrong parameter count for current switch", parameters.LineInfo));
                return null;
            }

            string modelName = parameters.Get(3).Image;
            var model = context.ModelsRegistry.FindModel(modelName);
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
                var resExpression = GetISwitchExpression(iOff, rOff, iOn, rOn, ic, lm, lr, im, id);
                resistor.Parameters.Expression = resExpression;
                resistor.Parameters.ParseAction = (expression) =>
                {
                    var parser = new ExpressionParser(context.Evaluator.GetEvaluationContext(null), false, context.CaseSensitivity);
                    return parser.Resolve(expression);
                };

                context.SimulationPreparations.ExecuteActionAfterSetup(
                    (simulation) =>
                    {
                        if (context.ModelsRegistry is StochasticModelsRegistry stochasticModelsRegistry)
                        {
                            resistorModel = stochasticModelsRegistry.ProvideStochasticModel(name, simulation, model);

                            if (!context.Result.FindObject(resistorModel.Name, out _))
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
                        resExpression = GetISwitchExpression(iOff, rOff, iOn, rOn, ic, lm, lr, im, id);
                        resistor.Parameters.Expression = resExpression;
                        resistor.Parameters.ParseAction = (expression) =>
                        {
                            var parser = new ExpressionParser(context.Evaluator.GetEvaluationContext(simulation), false, context.CaseSensitivity);
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
                csw.ControllingSource = context.NameGenerator.GenerateObjectName(parameters.Get(2).Image);
            }
            else
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Voltage source name expected", parameters.LineInfo));
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
                    context.Result);
            });

            // Optional on or off
            if (parameters.Count > 4)
            {
                switch (parameters.Get(4).Image.ToLower())
                {
                    case "on":
                        csw.SetParameter("on", true);
                        break;

                    case "off":
                        csw.SetParameter("off", true);
                        break;

                    default:
                        context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "ON or OFF expected", parameters.LineInfo));
                        return csw;
                }
            }

            return csw;
        }
    }
}