using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class WriterContext : IWriterContext
    {
        public const string RootCircuitName = "Circuit";
        private readonly Dictionary<string, string> _idCache = new ();
        private readonly Dictionary<string, string> _idCacheReversed = new ();
        private readonly Dictionary<string, int> _idCacheCounter = new ();
        private readonly Dictionary<string, List<string>> _subcircuitDependencies = new ();
        private readonly Dictionary<string, string> _models = new (StringComparer.OrdinalIgnoreCase);

        public SpiceNetlistCaseSensitivitySettings CaseSettings { get; set; }

        public EvaluationContext EvaluationContext { get; set; }

        public bool SubcircuitDictionaryPresent { get; set; }

        public List<(string, CSharpStatement)> SubcircuitCreateStatements { get; set; } = new ();

        public string WorkingDirectory { get; set; } = Environment.CurrentDirectory;

        public Encoding ExternalFilesEncoding { get; set; } = Encoding.Default;

        public string CurrentSubcircuitName { get; set; } = RootCircuitName;

        public SubCircuit ParentSubcircuit { get; set; } = null;

        public string GetNewIdentifier(string name)
        {
            if (name[0] >= '0' && name[0] <= '9')
            {
                name = "_" + name;
            }

            var idWithoutCounter = $@"{name.Substring(0, 1).ToLower()}{name.Substring(1)}";
            if (!_idCache.ContainsKey(idWithoutCounter))
            {
                _idCache[idWithoutCounter] = name;
                _idCacheCounter[name] = 0;
                _idCacheReversed[name] = idWithoutCounter;
                return idWithoutCounter;
            }

            var id = $@"{name.Substring(0, 1).ToLower()}{name.Substring(1)}_{_idCacheCounter[name]}";
            _idCache[id] = name;
            _idCacheReversed[name] = id;
            _idCacheCounter[name] = _idCacheCounter[name] + 1;

            return id;
        }

        public string GetIdentifier(string name)
        {
            if (name[0] >= '0' && name[0] <= '9')
            {
                name = "_" + name;
            }

            if (_idCacheReversed.ContainsKey(name))
            {
                return _idCacheReversed[name];
            }

            return GetNewIdentifier(name);
        }

        public string FindModelType(string modelName)
        {
            if (_models.ContainsKey(modelName))
            {
                return _models[modelName];
            }

            return null;
        }

        public void RegisterModelType(string modelName, string modelType)
        {
            _models[modelName] = modelType;
        }

        public void RegisterDependency(string currentSubcircuitName, string subCircuitDefinitonName)
        {
            if (!_subcircuitDependencies.ContainsKey(subCircuitDefinitonName))
            {
                _subcircuitDependencies[subCircuitDefinitonName] = new List<string>();
            }

            if (!_subcircuitDependencies[subCircuitDefinitonName].Contains(currentSubcircuitName))
            {
                _subcircuitDependencies[subCircuitDefinitonName].Add(currentSubcircuitName);
            }
        }

        public Dictionary<string, List<string>> GetDependencies()
        {
            return _subcircuitDependencies;
        }
    }
}
