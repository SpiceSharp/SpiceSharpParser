using SpiceNetlist.Runner.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpiceNetlist.Runner.Windows
{
    /// <summary>
    /// Refactor !!!!!!!!
    /// </summary>
    public partial class SpiceNetlistResult : Window
    {
        public SpiceNetlistResult()
        {
            InitializeComponent();
        }

        public string Netlist { get; }

        public SpiceNetlistResult(string netlist) : this()
        {
            Netlist = netlist;

            this.lblStatus.Text = "Status: Running ...";
            this.txtNetlist.Text = netlist;
            PlotsTab.IsEnabled = false;
            LogsTab.IsEnabled = false;
            Task.Run(() => Init());
        }

        private void Init()
        {
            Stopwatch mainWatch = new Stopwatch();
            Stopwatch secondaryWatch = new Stopwatch();
            try
            {
                mainWatch.Start();

                secondaryWatch.Start();
                var tokens = SpiceHelper.GetTokens(Netlist);
                secondaryWatch.Stop();
                

                this.txtStats.Dispatcher.Invoke(() =>
               {
                   this.txtStats.Text += $"Tokenization time: { secondaryWatch.ElapsedMilliseconds} ms \n";
               });

                secondaryWatch.Reset();
                secondaryWatch.Start();
                var parseTree = SpiceHelper.GetParseTree(tokens);
                secondaryWatch.Stop();
                this.txtStats.Dispatcher.Invoke(() =>
                {
                    this.txtStats.Text += $"Parse tree generation time: { secondaryWatch.ElapsedMilliseconds} ms \n";
                });

                secondaryWatch.Reset();
                secondaryWatch.Start();
                var netlist = SpiceHelper.GetNetlist(parseTree);
                secondaryWatch.Stop();
                this.txtStats.Dispatcher.Invoke(() =>
                {
                    this.txtStats.Text += $"Netlist object model generation time: { secondaryWatch.ElapsedMilliseconds} ms \n";
                });

                secondaryWatch.Reset();
                secondaryWatch.Start();
                var sNetlist = SpiceHelper.GetSpiceSharpNetlist(netlist);
                secondaryWatch.Stop();
                this.txtStats.Dispatcher.Invoke(() =>
                {
                    this.txtStats.Text += $"Translating Netlist object model to Spice# time: { secondaryWatch.ElapsedMilliseconds} ms \n";
                });

                this.txtStats.Dispatcher.Invoke(() =>
                {
                    this.txtStats.Text += $"--- \n";
                    this.txtStats.Text += $"Simulations found: {sNetlist.Simulations.Count}\n";
                });

                foreach (var simulation in sNetlist.Simulations)
                {
                    secondaryWatch.Reset();
                    secondaryWatch.Start();
                    simulation.Run(sNetlist.Circuit);
                    secondaryWatch.Stop();
                    this.txtStats.Dispatcher.Invoke(() =>
                    {
                        this.txtStats.Text += $"Finished executing simulation {simulation.Name} ({simulation.GetType()}) in {secondaryWatch.ElapsedMilliseconds} ms\n";
                    });
                }

                this.txtStats.Dispatcher.Invoke(() =>
                {
                    this.txtStats.Text += $"Plots found: {sNetlist.Plots.Count}\n";
                });

                if (sNetlist.Plots.Count > 0)
                {
                    foreach (var plot in sNetlist.Plots)
                    {
                        bool positive = SpiceHelper.IsPlotPositive(plot);
                        
                        this.PlotsTabs.Dispatcher.Invoke(() =>
                        {
                            this.PlotsTab.IsEnabled = true;
                            PlotControl control = new PlotControl(plot, positive);

                            var item = new TabItem() { Header = plot.Name };
                            item.Content = control;
                            this.PlotsTabs.Items.Add(item);
                        });
                    }
                }

                foreach (var warning in sNetlist.Warnings)
                {
                    this.LogsTab.Dispatcher.Invoke(() =>
                    {
                        txtLogs.Text += ("Warning: " + warning + "\n");
                    });
                }

                mainWatch.Stop();

                this.txtStats.Dispatcher.Invoke(() =>
                {
                    this.LogsTab.IsEnabled = true;
                    this.lblStatus.Text = "Status: Finished";
                    this.txtStats.Text += $"---\nFinished executing netlist in {mainWatch.ElapsedMilliseconds} ms\n";
                });

            }
            catch (Exception ex)
            {
                this.txtLogs.Dispatcher.Invoke(() =>
                {
                    txtLogs.Text += "Exception occurred: " + ex.ToString();

                });

                this.txtStats.Dispatcher.Invoke(() =>
                {
                    this.lblStatus.Text = "Status: Error (see 'Logs' tab)";
                    this.txtStats.Text += $"---\nFinished executing netlist in {mainWatch.ElapsedMilliseconds} ms\n";
                    this.LogsTab.IsSelected = true;
                    this.LogsTab.IsEnabled = true;
                });
            }
        }
    }
}
