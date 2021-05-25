using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms.Wave;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .WAVE <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class WaveControl : ExportControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveControl"/> class.
        /// </summary>
        /// <param name="mapper">The exporter mapper.</param>
        /// <param name="factory">Export factory.</param>
        public WaveControl(IMapper<Exporter> mapper, IExportFactory factory)
            : base(mapper, factory)
        {
        }

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            var transient = (Transient)context.Result.Simulations.FirstOrDefault(s => s is Transient);

            if (transient != null)
            {
                var timeParameters = transient.TimeParameters;
                if (statement.Parameters.Count < 3)
                {
                    throw new Exception("Too few parameters for WAV");
                }

                var filePath = statement.Parameters[0].Value;
                var bitPerSample = (int)context.Evaluator.EvaluateDouble(statement.Parameters[1]);
                var sampleRate = (int)context.Evaluator.EvaluateDouble(statement.Parameters[2]);

                if (statement.Parameters.Count == 4)
                {
                    var monoChannel = statement.Parameters[3];
                    var monoChannelExport = GenerateExport(monoChannel, context, transient);
                    var pwlData = new List<(double, double)>();
                    transient.ExportSimulationData += (sender, args) =>
                    {
                        pwlData.Add((args.Time, monoChannelExport.Extract()));
                    };

                    transient.AfterExecute += (sender, args) =>
                    {
                        var writer = new WaveFileWriter();
                        writer.Write(filePath, sampleRate, 1.0, bitPerSample, pwlData.ToArray());
                    };
                }
                else if (statement.Parameters.Count == 5)
                {
                    var leftChannel = statement.Parameters[3];
                    var rightChannel = statement.Parameters[4];
                    var leftChannelExport = GenerateExport(leftChannel, context, transient);
                    var rightChannelExport = GenerateExport(rightChannel, context, transient);

                    var leftData = new List<(double, double)>();
                    var rightData = new List<(double, double)>();
                    transient.ExportSimulationData += (sender, args) =>
                    {
                        leftData.Add((args.Time, leftChannelExport.Extract()));
                        rightData.Add((args.Time, rightChannelExport.Extract()));
                    };

                    transient.AfterExecute += (sender, args) =>
                    {
                        var writer = new WaveFileWriter();
                        writer.Write(
                            filePath,
                            sampleRate,
                            1.0,
                            bitPerSample,
                            leftData.ToArray(),
                            rightData.ToArray());
                    };
                }

                List<double> timeSteps = new List<double>();
                double step = 1.0 / sampleRate;

                for (var time = timeParameters.StartTime; time < timeParameters.StopTime; time += step)
                {
                    timeSteps.Add(time);
                }

                var sampler = new Sampler(".WAV sampler", timeSteps);
                context.ContextEntities.Add(sampler);
            }
        }
    }
}