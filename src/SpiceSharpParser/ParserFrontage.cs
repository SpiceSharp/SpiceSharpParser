using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Model;
using SpiceSharpParser.Model.SpiceObjects;

namespace SpiceSharpParser
{
    /// <summary>
    /// SpiceSharpParser front
    /// </summary>
    public class ParserFrontage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserFrontage"/> class.
        /// </summary>
        /// <param name="netlistModelReader">Netlist model reader</param>
        /// <param name="fileProvider">File provider</param>
        public ParserFrontage(INetlistModelReader netlistModelReader, IFileReader fileProvider)
        {
            FileReader = fileProvider;
            NetlistModelReader = netlistModelReader ?? throw new System.ArgumentNullException(nameof(netlistModelReader));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserFrontage"/> class.
        /// </summary>
        public ParserFrontage()
        {
            NetlistModelReader = new NetlistModelReader();
            FileReader = new FileReader();
        }

        /// <summary>
        /// Gets the netlist model reader
        /// </summary>
        public INetlistModelReader NetlistModelReader { get; }

        /// <summary>
        /// Gets the file provider
        /// </summary>
        public IFileReader FileReader { get; }

        /// <summary>
        /// Parses the netlist
        /// </summary>
        /// <param name="netlist">Netlist to parse</param>
        /// <param name="settings">Setting for parser</param>
        /// <returns>
        /// A parsing result
        /// </returns>
        public ParserResult ParseNetlist(string netlist, ParserSettings settings)
        {
            if (settings == null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            if (netlist == null)
            {
                throw new System.ArgumentNullException(nameof(netlist));
            }

            Model.Netlist netlistModel = NetlistModelReader.GetNetlistModel(netlist, settings);

            ProcessIncludes(netlistModel, new List<string>());

            Connector.ConnectorResult connectorResult = GetConnectorResult(netlistModel);
            return new ParserResult()
            {
                SpiceSharpModel = connectorResult,
                NetlistModel = netlistModel
            };
        }

        /// <summary>
        /// Process includes
        /// </summary>
        private void ProcessIncludes(Netlist netlistModel, List<string> loadedPaths, string netlistPath = null)
        {
            var includes = netlistModel.Statements.Where(statement => statement is Control c && c.Name.ToLower() == "include");

            if (includes.Any())
            {
                foreach (Control include in includes.ToArray())
                {
                    string includePath = include.Parameters[0].Image.Trim('"');

                    if (!loadedPaths.Contains(includePath))
                    {
                        string includeContent = FileReader.GetFileContent(includePath);

                        if (includeContent != null)
                        {
                            Netlist includeModel = NetlistModelReader.GetNetlistModel(includeContent, 
                                new ParserSettings() { HasTitle = false, IsEndRequired = false });

                            ProcessIncludes(includeModel, loadedPaths, includePath);

                            netlistModel.Statements.Merge(includeModel.Statements);
                            loadedPaths.Add(includePath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the SpiceSharp model for the netlist
        /// </summary>
        /// <param name="netlist">Netlist model</param>
        /// <returns>
        /// A new SpiceSharp model for the netlist
        /// </returns>
        private Connector.ConnectorResult GetConnectorResult(SpiceSharpParser.Model.Netlist netlist)
        {
            var connector = new Connector.Connector();
            return connector.Translate(netlist);
        }
    }
}
