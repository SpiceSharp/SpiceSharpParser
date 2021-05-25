using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelWriters.CSharp.Entities;
using SpiceSharpParser.ModelWriters.CSharp.Entities.Components;
using SpiceSharpParser.ModelWriters.CSharp.Controls;
using SpiceSharpParser.ModelWriters.CSharp.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.ModelWriters.CSharp.Graphs;
using SpiceSharpParser.ModelWriters.CSharp.Language;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class StatementsWriter
    {
        private Dictionary<string, IWriter<Model>> ModelWriters = new Dictionary<string, IWriter<Model>>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, IWriter<Component>> ComponentWriters = new Dictionary<string, IWriter<Component>>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, IWriter<Control>> ControlWriters = new Dictionary<string, IWriter<Control>>(StringComparer.OrdinalIgnoreCase);

        private SubCircuitDefintionWriter _subCircuitDefintionWriter;

        public StatementsWriter()
        {
            _subCircuitDefintionWriter = new SubCircuitDefintionWriter(this);

            ModelWriters["R"] = new ResistorModelWriter();
            ModelWriters["RES"] = ModelWriters["R"];
            ModelWriters["C"] = new CapacitorModelWriter();
            ModelWriters["D"] = new DiodeModelWriter();
            ModelWriters["NPN"] = new BipolarModelWriter();
            ModelWriters["PNP"] = ModelWriters["NPN"];
            ModelWriters["NJF"] = new JFETModelWriter();
            ModelWriters["PJF"] = ModelWriters["NJF"];
            ModelWriters["SW"] = new SwitchModelWriter();
            ModelWriters["CSW"] = ModelWriters["SW"];
            ModelWriters["PMOS"] = new MosfetModelWriter();
            ModelWriters["NMOS"] = ModelWriters["PMOS"];
            //ModelWriters["VSWITCH"] TODO
            //ModelWriters["ISWITCH"] TODO

            ComponentWriters["R"] = new ResistorWriter();
            ComponentWriters["L"] = new InductorWriter();
            ComponentWriters["K"] = new MutualInductanceWriter();
            ComponentWriters["C"] = new CapacitorWriter();

            ComponentWriters["V"] = new VoltageSourceWriter(new WaveformWriter());
            ComponentWriters["I"] = new CurrentSourceWriter(new WaveformWriter());
            ComponentWriters["Q"] = new BipolarJunctionTransistorWriter();
            ComponentWriters["D"] = new DiodeWriter();
            ComponentWriters["M"] = new MosfetWriter();
            ComponentWriters["J"] = new JFETWriter();
            ComponentWriters["X"] = new SubCircuitWriter();
            ComponentWriters["G"] = new VoltageControlledCurrentSourceWriter();
            ComponentWriters["F"] = new CurrentControlledCurrentSourceWriter();
            ComponentWriters["E"] = new VoltageControlledVoltageSourceWriter();
            ComponentWriters["H"] = new CurrentControlledVoltageSourceWriter();
            ComponentWriters["B"] = new ArbitraryBehavioralWriter();
            ComponentWriters["T"] = new LosslessTransmissionLineWriter();
            ComponentWriters["S"] = new VoltageSwitchWriter();
            ComponentWriters["W"] = new CurrentSwitchWriter();

            ControlWriters["DC"] = new DcWriter();
            ControlWriters["AC"] = new AcWriter();
            ControlWriters["TRAN"] = new TransientWriter();
            ControlWriters["IC"] = new ICWriter();
            ControlWriters["OP"] = new OpWriter();
            ControlWriters["PARAM"] = new ParamWriter();
            ControlWriters["FUNC"] = new FuncWriter();
            ControlWriters["OPTIONS"] = new OptionsWriter();
        }

        public List<CSharpStatement> Write(bool? @public, bool local, string methodName, Statements statements, List<AssignmentParameter> assignmentParameters, IWriterContext context, bool createSubCircuitDefinitions = true, bool optionalParameters = true)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            foreach (var asg in assignmentParameters)
            {
                context.EvaluationContext.Variables[asg.Name] = asg.Value;
            }

            var cSharpStatements = new List<CSharpStatement>();

            foreach (var statement in statements.OrderBy(GetOrder))
            {
                if (statement is SubCircuit subCircuitDefinition)
                {
                    foreach (var asg in subCircuitDefinition.DefaultParameters)
                    {
                        context.EvaluationContext.Variables[asg.Name] = asg.Value;
                    }

                    cSharpStatements.AddRange(_subCircuitDefintionWriter.Write(subCircuitDefinition, context));

                    foreach (var asg in subCircuitDefinition.DefaultParameters)
                    {
                        context.EvaluationContext.Variables.Remove(asg.Name);
                    }
                }

                if (statement is Model model)
                {
                    var type = ModelWriter.GetType(model);

                    if (ModelWriters.ContainsKey(type))
                    {
                        var writer = ModelWriters[type];
                        cSharpStatements.AddRange(writer.Write(model, context));
                    }
                    else
                    {
                        cSharpStatements.Add(new CSharpComment("Skipped, unsupported model:" + model));
                    }
                }

                if (statement is Component component)
                {
                    var type = component.Name.Substring(0, 1);
                    if (ComponentWriters.ContainsKey(type))
                    {
                        var writer = ComponentWriters[type];
                        cSharpStatements.AddRange(writer.Write(component, context));
                    }
                    else
                    {
                        cSharpStatements.Add(new CSharpComment("Skipped, unsupported component:" + component));
                    }
                }

                if (statement is Control control)
                {
                    var type = control.Name;
                    if (ControlWriters.ContainsKey(type))
                    {
                        var writer = ControlWriters[type];
                        cSharpStatements.AddRange(writer.Write(control, context));
                    }
                    else
                    {
                        cSharpStatements.Add(new CSharpComment("Skipped, unsupported control:" + control));
                    }
                }
            }

            var result = new List<CSharpStatement>();
            result.AddRange(cSharpStatements.OfType<CSharpFieldDeclaration>());
            result.AddRange(cSharpStatements.Where(s => s.Kind != CSharpStatementKind.CreateEntity && !(s is CSharpFieldDeclaration)));

            var localMethods = cSharpStatements.Where(s => (s is CSharpMethod r && r.Local) && s.Kind == CSharpStatementKind.CreateEntity);
            var externalMethods = cSharpStatements.Where(s => (s is CSharpMethod r && !r.Local));
            var methodStatements = cSharpStatements.Where(s => !(s is CSharpMethod) && !(s is CSharpFieldDeclaration) && s.Kind == CSharpStatementKind.CreateEntity).ToList();

            var collectionId = context.GetNewIdentifier("collection");

            result.AddRange(externalMethods);

            methodStatements.AddRange(
                methodStatements.OfType<CSharpNewStatement>()
                .Where(s => s.IncludeInCollection)
                .Select(@new => new CSharpCallStatement(collectionId, $"Add({@new.VariableName})")).ToList());
            methodStatements.Insert(0, new CSharpNewStatement(collectionId, "new EntityCollection()"));
            methodStatements.Add(new CSharpReturnStatement(collectionId));
            methodStatements.AddRange(localMethods);

            var parameters = new List<string>();
            var defaults = new List<string>();
            var types = new List<Type>();

            for (var i = 0; i < assignmentParameters.Count; i++)
            {
                parameters.Add(assignmentParameters[i].Name);
                defaults.Add(assignmentParameters[i].Value);
                types.Add(typeof(string));
            }

            if (createSubCircuitDefinitions)
            {
                var createCircuitDefinitionStatementsOrdered = OrderSubcircuitStatements(context);
                if (createCircuitDefinitionStatementsOrdered.Any())
                {
                    var createSubCircuitMethod = new CSharpMethod(
                        null,
                        "CreateSubCircuitDefinitions_" + context.CurrentSubcircuitName,
                        "void",
                        new string[0],
                        new string[0],
                        new Type[0],
                        createCircuitDefinitionStatementsOrdered.ToList(),
                        false)
                    {
                        Local = true
                    };

                    methodStatements.Add(createSubCircuitMethod);
                    methodStatements.Insert(0, new CSharpCallStatement(null, $"CreateSubCircuitDefinitions_{context.CurrentSubcircuitName}()"));
                }
            }

            result.Add(
                new CSharpMethod(@public, methodName, "EntityCollection", parameters.ToArray(), defaults.ToArray(), types.ToArray(), methodStatements, optionalParameters)
                {
                    Local = local,
                });

            foreach (var asg in assignmentParameters)
            {
                context.EvaluationContext.Variables.Remove(asg.Name);
            }

            return result;
        }

        private List<CSharpStatement> OrderSubcircuitStatements(IWriterContext context)
        {
            if (context.SubcircuitCreateStatements.Any())
            {
                var graph = new Graph<string>();
                graph.Edges = new HashSet<Edge<string>>(context.GetDependencies().SelectMany(p => p.Value, (parent, child) => new Edge<string>(parent.Key, child)));

                foreach (var edge in graph.Edges)
                {
                    if (!graph.Nodes.Contains(edge.From))
                    {
                        graph.Nodes.Add(edge.From);
                    }

                    if (!graph.Nodes.Contains(edge.To))
                    {
                        graph.Nodes.Add(edge.To);
                    }
                }

                var tSort = new TopologicalSort();
                var list = tSort.GetSorted(graph).ToList();

                var subcircuitCreateStatementsOrdered = context.SubcircuitCreateStatements.OrderBy(s => list.IndexOf(s.Item1));

                return subcircuitCreateStatementsOrdered.Select(r => r.Item2).ToList();
            }
            else
            {
                return new List<CSharpStatement>();
            }
        }

        private int GetOrder(Statement statement)
        {
            if (statement is SubCircuit)
            {
                return 1000;
            }

            if (statement is Control)
            {
                return 1500;
            }

            if (statement is Model)
            {
                return 2000;
            }

            if (statement is Component)
            {
                return 3000;
            }

            return int.MaxValue;
        }
    }
}
