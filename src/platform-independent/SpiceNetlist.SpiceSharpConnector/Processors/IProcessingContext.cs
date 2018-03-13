﻿using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots;
using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using SpiceSharp.Circuits;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    //TODO: Refactor this more...

    public interface IProcessingContext
    {
        string ContextName { get; }

        Evaluator Evaluator { get; }

        Dictionary<string, double> AvailableParameters { get; }

        IEnumerable<Simulation> Simulations
        {
            get;
        }

        string Path { get; }

        IProcessingContext Parent { get; set; }

        Netlist Netlist { get; }

        List<SubCircuit> AvailableSubcircuits { get; }

        SimulationConfiguration SimulationConfiguration { get; }

        void AddWarning(string warning);

        void AddComment(CommentLine statement);

        void AddExport(Export export);

        void AddPlot(Plot plot);

        void AddEntity(Entity entity);

        void AddSimulation(BaseSimulation simulation);

        void SetICVoltage(string nodeName, string value);

        double ParseDouble(string expression);

        void SetProperty(Entity entity, string propertyName, string expression);

        T FindModel<T>(string modelName)
            where T : Entity;

        void SetEntityParameters(Entity entity, ParameterCollection parameters, int toSkip = 0);

        void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters);

        string GenerateObjectName(string objectName);

        string GenerateNodeName(string pinName);
    }
}