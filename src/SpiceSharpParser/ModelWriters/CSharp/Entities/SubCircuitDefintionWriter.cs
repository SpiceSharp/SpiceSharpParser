using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities
{
    public class SubCircuitDefintionWriter : IWriter<SubCircuit>
    {
        public SubCircuitDefintionWriter(StatementsWriter statementsWriter)
        {
            StatementsWriter = statementsWriter;
        }

        public StatementsWriter StatementsWriter { get; }

        public List<CSharpStatement> Write(SubCircuit @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();
            var parameters = @object.DefaultParameters.ToList();
            parameters.Add(new SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters.AssignmentParameter() { Name = "internal_m", Value = "1" });
            var localParameters = parameters.Where(p => context.ParentSubcircuit == null || (!context.ParentSubcircuit.DefaultParameters.Any(dp => dp.Name == p.Name) && p.Name != "internal_m")).ToList();

            var parameterNames = localParameters.Select(p => p.Name).ToArray();
            var defaults = localParameters.Select(p => p.Name.ToLower() == "internal_m" ? "1" : p.Value);

            var oldSubcircuitName = context.CurrentSubcircuitName;
            var oldSubcircuitParent = context.ParentSubcircuit;
            context.CurrentSubcircuitName = @object.Name;
            context.ParentSubcircuit = @object;
            var currentStatements = context.SubcircuitCreateStatements.ToList();

            var methodName = "CreateSubcircuitCollection_" + @object.Name;
            result.AddRange(StatementsWriter.Write(
                context.ParentSubcircuit == null ? true : null,
                oldSubcircuitParent != null,
                methodName,
                @object.Statements,
                localParameters.ToList(),
                context,
                optionalParameters: false));

            context.CurrentSubcircuitName = oldSubcircuitName;
            context.ParentSubcircuit = oldSubcircuitParent;
            context.SubcircuitCreateStatements = currentStatements;

            var subDefStatements = new List<CSharpStatement>();
            subDefStatements.Add(new CSharpAssignmentStatement("var entities", $"{methodName}({string.Join(",", parameterNames)})"));

            var pinsString = string.Join(",", @object.Pins.Select(p => @$"""{p}"""));

            subDefStatements.Add(new CSharpNewStatement("definition", $"new SubcircuitDefinition(entities, {pinsString})"));
            subDefStatements.Add(new CSharpReturnStatement("definition"));

            var methodDefName = "CreateSubcircuitDefinition_" + @object.Name;
            result.Add(
                new CSharpMethod(
                    context.CurrentSubcircuitName != WriterContext.RootCircuitName ? null : true,
                    methodDefName,
                    "SubcircuitDefinition",
                    parameterNames,
                    defaults.ToArray(),
                    localParameters.Select(_ => typeof(string)).ToArray(),
                    subDefStatements,
                    true)
                {
                    Local = context.ParentSubcircuit != null,
                });

            return result;
        }
    }
}
