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
            System.Windows.Application.Current.Shutdown();
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var netlist = SpiceHelper.GetNetList(this.txtEditor.Text);
                SpiceHelper.RunAllSimulations(netlist);

                MessageBox.Show("Netlist was successfully run", "Info", MessageBoxButton.OK);

                if (netlist.Plots.Count > 0)
                {
                    MessageBox.Show("Netlist has a plot");

                    PlotWindow window = new PlotWindow(netlist.Plots[0]);
                   
                    window.Show();
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show("Netlist was failed to run", "Info", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
    }
}
