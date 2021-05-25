using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public interface IWriterContext
    {
        SpiceNetlistCaseSensitivitySettings CaseSettings { get; }

        EvaluationContext EvaluationContext { get; }

        bool SubcircuitDictionaryPresent { get; set; }

        List<(string, CSharpStatement)> SubcircuitCreateStatements { get; set; }

        string WorkingDirectory { get; set; }

        Encoding ExternalFilesEncoding { get; set; }

        string CurrentSubcircuitName { get; set; }

        SubCircuit ParentSubcircuit { get; set; }

        void RegisterDependency(string currentSubcircuitName, string subCircuitDefinitonName);

        Dictionary<string, List<string>> GetDependencies();

        string FindModelType(string modelName);

        void RegisterModelType(string modelName, string modelType);

        string GetNewIdentifier(string name);

        string GetIdentifier(string name);
    }
}
