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
                string netList = txtEditor.Text;

                var lexer = new SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = true });
                var tokensEnumerable = lexer.GetTokens(netList);
                var tokens = tokensEnumerable.ToArray();

                var parseTree = new SpiceParser.SpiceParser().GetParseTree(tokens);

                var eval = new ParseTreeEvaluator();
                var netlist = eval.Evaluate(parseTree) as SpiceNetlist.Netlist;

                var connector = new Connector();
                var n = connector.Translate(netlist);

                n.Simulations[0].Run(n.Circuit);
                MessageBox.Show("Netlist was run", "Info", MessageBoxButton.OK);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Netlist was failed to run", "Info", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
    }
}
