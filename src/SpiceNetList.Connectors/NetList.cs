using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Expressions;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;

namespace SpiceNetlist.SpiceSharpConnector
{
    public class NetList
    {
        public string Title { get; set; }

        public Circuit Circuit { get; set; }

        public List<SubCircuit> Definitions { get; set; } = new List<SubCircuit>();

        //TODO: Introduce better user parameters system
        public Dictionary<string, double> UserGlobalParameters = new Dictionary<string, double>();

        public List<Simulation> Simulations { get; set; } = new List<Simulation>();
      
        public List<string> Comments { get; set; }

        public List<string> Warnings { get; set; }

        public List<string> Errors { get; set; }

        internal BaseConfiguration BaseConfiguration { get; set; }

        internal FrequencyConfiguration FrequencyConfiguration { get; set; }

        internal TimeConfiguration TimeConfiguration { get; set; }

        internal DCConfiguration DCConfiguration { get; set; }
        

        internal Entity FindModel(Identifier modelId)
        {
            return Circuit.Objects[modelId];
        }

        internal double ParseDouble(string value)
        {
            if (UserGlobalParameters.ContainsKey(value))
            {
                return UserGlobalParameters[value];
            }

            SpiceExpression spiceExpressionParser = new SpiceExpression();
            return spiceExpressionParser.Parse(value);
        }

        internal T FindModel<T>(string modelName) where T : Entity
        {
            return (T)Circuit.Objects.FirstOrDefault(e => e.Name.Name == modelName && e is T);
        }
    }
}
