using SpiceLexer;
using SpiceNetlist.SpiceSharpConnector;
using SpiceParser;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpiceNetlist.Runner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var netlist = SpiceHelper.GetNetList(this.txtEditor.Text);
                SpiceHelper.RunAllSimulations(netlist);

                if (netlist.Plots.Count > 0)
                {
                    foreach (var plot in netlist.Plots)
                    {
                        bool positive = SpiceHelper.IsPlotPositive(plot);
                        PlotWindow window = new PlotWindow(plot, positive);
                        window.Show();
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Netlist was failed to run: " + ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
    }
}
