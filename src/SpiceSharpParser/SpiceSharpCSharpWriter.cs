using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.ModelWriters.CSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SpiceSharpParser
{
    public class SpiceSharpCSharpWriter
    {
        public SyntaxNode WriteCreateCircuitClass(string className, SpiceNetlist model, bool validateClass = true)
        {
            if (className is null)
            {
                throw new System.ArgumentNullException(nameof(className));
            }

            if (model is null)
            {
                throw new System.ArgumentNullException(nameof(model));
            }

            var writer = new NetlistWriter(new StatementsWriter(), new SimulationsWriter());
            var @class = writer.Write(className, model.Statements);
            var translator = new CSharpStatementTranslator();
            var code = translator.GetCSharpCode(@class);
            var rootNode = CSharpSyntaxTree.ParseText(code).GetRoot();
            var codeIssues = rootNode.GetDiagnostics();

            if (validateClass && codeIssues.Any())
            {
                throw new SpiceSharpException("Generated class has some code issues");
            }

            return rootNode;
        }

        public Assembly CreateCircuitAssembly(string className, SpiceNetlist model)
        {
            var rootNode = WriteCreateCircuitClass(className, model);
            var compilation
            = CSharpCompilation.Create(
                "Simulation.dll",
                new[] { rootNode.SyntaxTree },
                null,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            compilation = compilation.WithReferences(
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
                MetadataReference.CreateFromFile(typeof(Circuit).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(SpiceSharpBehavioral.Parsers.Lexer).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location));

            using (var memoryStream = new MemoryStream())
            {
                EmitResult result = compilation.Emit(memoryStream);
                if (result.Success)
                {
                    var assembly = Assembly.Load(memoryStream.GetBuffer());
                    return assembly;
                }

                return null;
            }
        }

        public EntityCollection CreateCircuit(SpiceNetlist model)
        {
            string className = "Experiment";

            var assembly = CreateCircuitAssembly(className, model);
            if (assembly == null)
            {
                throw new SpiceSharpException("Generation of circuit assembly failed");
            }

            var factory = assembly.CreateInstance(className);

            if (factory == null)
            {
                throw new SpiceSharpException("Creating instance of class failed");
            }

            MethodInfo magicMethod = factory.GetType().GetMethod("CreateCircuit");
            if (magicMethod == null)
            {
                throw new SpiceSharpException("Getting handle to CreateCircuit method failed");
            }

            return (EntityCollection)magicMethod.Invoke(factory, new object[0]);
        }

        public List<Simulation> CreateSimulations(SpiceNetlist model)
        {
            string className = "Experiment";
            var assembly = CreateCircuitAssembly(className, model);
            if (assembly == null)
            {
                throw new SpiceSharpException("Generation of circuit assembly failed");
            }

            var factory = assembly.CreateInstance(className);

            if (factory == null)
            {
                throw new SpiceSharpException("Creating instance of class failed");
            }

            MethodInfo magicMethod = factory.GetType().GetMethod("CreateSimulations");

            if (magicMethod == null)
            {
                throw new SpiceSharpException("Getting handle to CreateSimulations method failed");
            }

            return (List<Simulation>)magicMethod.Invoke(factory, new object[0]);
        }
    }
}
