using System;
using System.Collections.Generic;
using System.Globalization;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Custom;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Model = SpiceSharp.Components.Model;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public class SwitchGenerator : ComponentGenerator
    {
        /// <summary>
        /// Gets generated types.
        /// </summary>
        /// <returns>
        /// Generated types.
        /// </returns>
        public override IEnumerable<string> GeneratedTypes => new List<string> { "s", "w" };

        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
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
        protected SpiceSharp.Components.Component GenerateVoltageSwitch(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 5)
            {
                throw new WrongParametersCountException("Wrong parameter count for voltage switch");
            }

            string modelName = parameters.Get(4).Image;

            if (context.ModelsRegistry.FindModel<Model>(modelName) is VSwitchModel vmodel)
            {
                Resistor resistor = new Resistor(name);
                Model resistorModel = vmodel;
                context.CreateNodes(resistor, parameters.Take(2));
                context.SimulationPreparations.ExecuteTemperatureBehaviorBeforeLoad(resistor);

                context.SimulationPreparations.ExecuteActionBeforeSetup(
                    (simulation) =>
                    {
                        if (context.ModelsRegistry is StochasticModelsRegistry stochasticModelsRegistry)
                        {
                            resistorModel = stochasticModelsRegistry.ProvideStochasticModel(name, simulation, vmodel);

                            if (!context.Result.FindObject(resistorModel.Name, out _))
                            {
                                stochasticModelsRegistry.RegisterModelInstance(resistorModel);
                                context.Result.Circuit.Add(resistorModel);
                            }
                        }

                        double rOff = resistorModel.ParameterSets.GetParameter<double>("roff");

                        string resExpression =
                            $"pos(table(v({parameters.Get(2)}, {parameters.Get(3)}), @{resistorModel.Name}[voff], @{resistorModel.Name}[roff] , @{resistorModel.Name}[von], @{resistorModel.Name}[ron]), {rOff.ToString(CultureInfo.InvariantCulture)})";
                        context.SetParameter(resistor, "resistance", resExpression, beforeTemperature: true, onload: true);
                    });
                return resistor;
            }
            else
            {
                VoltageSwitch vsw = new VoltageSwitch(name);
                context.CreateNodes(vsw, parameters);

                context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
                {
                    context.ModelsRegistry.SetModel(
                        vsw,
                        simulation,
                        parameters.Get(4).Image,
                        $"Could not find model {parameters.Get(4)} for voltage switch {name}",
                        (VoltageSwitchModel model) => { vsw.Model = model.Name; },
                        context.Result);
                });

                // Optional ON or OFF
                if (parameters.Count == 6)
                {
                    switch (parameters.Get(5).Image.ToLower())
                    {
                        case "on":
                            vsw.ParameterSets.SetParameter("on");
                            break;
                        case "off":
                            vsw.ParameterSets.SetParameter("off");
                            break;
                        default:
                            throw new Exception("ON or OFF expected");
                    }
                }
                else if (parameters.Count > 6)
                {
                    throw new WrongParametersCountException("Too many parameters for voltage switch");
                }

                return vsw;
            }
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
        protected SpiceSharp.Components.Component GenerateCurrentSwitch(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 4)
            {
                throw new WrongParametersCountException("Wrong parameter count for current switch");
            }

            string modelName = parameters.Get(3).Image;

            if (context.ModelsRegistry.FindModel<Model>(modelName) is ISwitchModel s)
            {
                Resistor resistor = new Resistor(name);
                Model resistorModel = s;
                context.CreateNodes(resistor, parameters.Take(2));
                context.SimulationPreparations.ExecuteTemperatureBehaviorBeforeLoad(resistor);

                context.SimulationPreparations.ExecuteActionBeforeSetup(
                    (simulation) =>
                    {
                        if (context.ModelsRegistry is StochasticModelsRegistry stochasticModelsRegistry)
                        {
                            resistorModel = stochasticModelsRegistry.ProvideStochasticModel(name, simulation, s);

                            if (!context.Result.FindObject(resistorModel.Name, out _))
                            {
                                stochasticModelsRegistry.RegisterModelInstance(resistorModel);
                                context.Result.Circuit.Add(resistorModel);
                            }
                        }

                        double rOff = resistorModel.ParameterSets.GetParameter<double>("roff");

                        string resExpression = $"pos(table(i({parameters.Get(2)}), @{resistorModel.Name}[ioff], @{resistorModel.Name}[roff] , @{resistorModel.Name}[ion], @{resistorModel.Name}[ron]), {rOff.ToString(CultureInfo.InvariantCulture)})";
                        context.SetParameter(resistor, "resistance", resExpression, beforeTemperature: true, onload: true);
                    });
                return resistor;
            }
            else
            {
                CurrentSwitch csw = new CurrentSwitch(name);
                context.CreateNodes(csw, parameters);

                // Get the controlling voltage source
                if (parameters[2] is WordParameter || parameters[2] is IdentifierParameter)
                {
                    csw.ControllingName = context.ComponentNameGenerator.Generate(parameters.Get(2).Image);
                }
                else
                {
                    throw new WrongParameterTypeException("Voltage source name expected");
                }

                // Get the model
                context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
                {
                    context.ModelsRegistry.SetModel(
                        csw,
                        simulation,
                        parameters.Get(3).Image,
                        $"Could not find model {parameters.Get(3)} for current switch {name}",
                        (CurrentSwitchModel model) => csw.Model = model.Name,
                        context.Result);
                });

                // Optional on or off
                if (parameters.Count > 4)
                {
                    switch (parameters.Get(4).Image.ToLower())
                    {
                        case "on":
                            csw.ParameterSets.SetParameter("on");
                            break;
                        case "off":
                            csw.ParameterSets.SetParameter("off");
                            break;
                        default:
                            throw new GeneralReaderException("ON or OFF expected");
                    }
                }

                return csw;
            }
        }
    }
}
