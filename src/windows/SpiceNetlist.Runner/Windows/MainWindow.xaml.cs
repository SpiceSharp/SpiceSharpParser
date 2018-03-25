using SpiceLexer;
using SpiceNetlist.Runner.Windows;
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
            SpiceNetlistResult netlistWindow = new SpiceNetlistResult(this.txtEditor.Text);
            netlistWindow.Show();
        }
    }
}
