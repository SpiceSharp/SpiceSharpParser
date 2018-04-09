using System;
using System.Collections.Generic;
using System.IO;
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
        /// <param name="workingDirectoryPath">A full path to working directory of the netlist</param>
        /// <returns>
        /// A parsing result
        /// </returns>
        public ParserResult ParseNetlist(string netlist, ParserSettings settings, string workingDirectoryPath = null)
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

            ProcessIncludes(netlistModel, new List<string>(), workingDirectoryPath);

            Connector.ConnectorResult connectorResult = GetConnectorResult(netlistModel);
            return new ParserResult()
            {
                SpiceSharpModel = connectorResult,
                NetlistModel = netlistModel
            };
        }

        /// <summary>
        /// Processes .include statements
        /// </summary>
        /// <param name="netlistModel">Netlist model to seach for .include statements</param>
        /// <param name="loadedFullPaths">List of paths of loaded netlists</param>
        /// <param name="currentDirectoryPath">Current working directory path</param>
        private void ProcessIncludes(Netlist netlistModel, List<string> loadedFullPaths, string currentDirectoryPath = null)
        {
            var includes = netlistModel.Statements.Where(statement => statement is Control c && c.Name.ToLower() == "include");

            if (includes.Any())
            {
                foreach (Control include in includes.ToArray())
                {
                    // get full path of .include
                    string includePath = include.Parameters[0].Image.Trim('"');
                    bool isAbsolutePath = Path.IsPathRooted(includePath);
                    string includeFullPath = isAbsolutePath ? includePath : Path.Combine(currentDirectoryPath, includePath);

                    // check if path is not already loaded
                    if (!loadedFullPaths.Contains(includeFullPath))
                    {
                        // check if file exists
                        if (!File.Exists(includeFullPath))
                        {
                            throw new InvalidOperationException($"Netlist include at { includeFullPath}  is not found");
                        }

                        // mark includeFullPath as loaded (before loading all includes to avoid loops)
                        loadedFullPaths.Add(includeFullPath);

                        // get include content
                        string includeContent = FileReader.GetFileContent(includeFullPath);
                        if (includeContent != null)
                        {
                            // get include netlist model
                            Netlist includeModel = NetlistModelReader.GetNetlistModel(
                                includeContent,
                                new ParserSettings() { HasTitle = false, IsEndRequired = false });

                            // process includes of include netlist
                            ProcessIncludes(includeModel, loadedFullPaths, Path.GetDirectoryName(includeFullPath));

                            // merge include with netlist 
                            netlistModel.Statements.Merge(includeModel.Statements);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Netlist include at { includeFullPath} could not be loaded");
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
