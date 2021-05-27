using SpiceSharpParser.Common.Mathematics.Graphs;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelWriters.CSharp.Controls;
using SpiceSharpParser.ModelWriters.CSharp.Entities;
using SpiceSharpParser.ModelWriters.CSharp.Entities.Components;
using SpiceSharpParser.ModelWriters.CSharp.Entities.Models;
using SpiceSharpParser.ModelWriters.CSharp.Language;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class StatementsWriter
    {
        private readonly Dictionary<string, IWriter<Model>> _modelWriters = new (StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IWriter<Component>> _componentWriters = new (StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IWriter<Control>> _controlWriters = new (StringComparer.OrdinalIgnoreCase);
        private readonly SubCircuitDefintionWriter _subCircuitDefinitionWriter;

        public StatementsWriter()
        {
            _subCircuitDefinitionWriter = new SubCircuitDefintionWriter(this);

            _modelWriters["R"] = new ResistorModelWriter();
            _modelWriters["RES"] = _modelWriters["R"];
            _modelWriters["C"] = new CapacitorModelWriter();
            _modelWriters["D"] = new DiodeModelWriter();
            _modelWriters["NPN"] = new BipolarModelWriter();
            _modelWriters["PNP"] = _modelWriters["NPN"];
            _modelWriters["NJF"] = new JFETModelWriter();
            _modelWriters["PJF"] = _modelWriters["NJF"];
            _modelWriters["SW"] = new SwitchModelWriter();
            _modelWriters["CSW"] = _modelWriters["SW"];
            _modelWriters["PMOS"] = new MosfetModelWriter();
            _modelWriters["NMOS"] = _modelWriters["PMOS"];
            //ModelWriters["VSWITCH"] TODO
            //ModelWriters["ISWITCH"] TODO

            _componentWriters["R"] = new ResistorWriter();
            _componentWriters["L"] = new InductorWriter();
            _componentWriters["K"] = new MutualInductanceWriter();
            _componentWriters["C"] = new CapacitorWriter();

            _componentWriters["V"] = new VoltageSourceWriter(new WaveformWriter());
            _componentWriters["I"] = new CurrentSourceWriter(new WaveformWriter());
            _componentWriters["Q"] = new BipolarJunctionTransistorWriter();
            _componentWriters["D"] = new DiodeWriter();
            _componentWriters["M"] = new MosfetWriter();
            _componentWriters["J"] = new JFETWriter();
            _componentWriters["X"] = new SubCircuitWriter();
            _componentWriters["G"] = new VoltageControlledCurrentSourceWriter();
            _componentWriters["F"] = new CurrentControlledCurrentSourceWriter();
            _componentWriters["E"] = new VoltageControlledVoltageSourceWriter();
            _componentWriters["H"] = new CurrentControlledVoltageSourceWriter();
            _componentWriters["B"] = new ArbitraryBehavioralWriter();
            _componentWriters["T"] = new LosslessTransmissionLineWriter();
            _componentWriters["S"] = new VoltageSwitchWriter();
            _componentWriters["W"] = new CurrentSwitchWriter();

            _controlWriters["DC"] = new DcWriter();
            _controlWriters["AC"] = new AcWriter();
            _controlWriters["TRAN"] = new TransientWriter();
            _controlWriters["IC"] = new ICWriter();
            _controlWriters["OP"] = new OpWriter();
            _controlWriters["PARAM"] = new ParamWriter();
            _controlWriters["FUNC"] = new FuncWriter();
            _controlWriters["OPTIONS"] = new OptionsWriter();
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

                    cSharpStatements.AddRange(_subCircuitDefinitionWriter.Write(subCircuitDefinition, context));

                    foreach (var asg in subCircuitDefinition.DefaultParameters)
                    {
                        context.EvaluationContext.Variables.Remove(asg.Name);
                    }
                }

                if (statement is Model model)
                {
                    var type = ModelWriter.GetType(model);

                    if (_modelWriters.ContainsKey(type))
                    {
                        var writer = _modelWriters[type];
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
                    if (_componentWriters.ContainsKey(type))
                    {
                        var writer = _componentWriters[type];
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
                    if (_controlWriters.ContainsKey(type))
                    {
                        var writer = _controlWriters[type];
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
                        Local = true,
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
