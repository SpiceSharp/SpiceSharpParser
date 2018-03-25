using Microsoft.Win32;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots;
using System;
using System.Collections.Generic;
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

namespace SpiceNetlist.Runner.Controls
{
    /// <summary>
    /// Interaction logic for PlotControl.xaml
    /// </summary>
    public partial class PlotControl : UserControl
    {
        public PlotControl()
        {
            InitializeComponent();
        }

        public PlotControl(Plot plot, bool enableYLogAxis) : this()
        {
            Plot = plot;
            DataContext = new PlotViewModel(plot, false, false);
            this.y.IsEnabled = enableYLogAxis;
        }

        public Plot Plot { get; }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            PlotViewModel model = new PlotViewModel(Plot, this.x.IsChecked.Value, this.y.IsChecked.Value);
            this.DataContext = model;

        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            PlotViewModel model = new PlotViewModel(Plot, this.x.IsChecked.Value, this.y.IsChecked.Value);
            this.DataContext = model;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
            };

            if (dialog.ShowDialog() == true)
            {
                this.plot.SaveBitmap(dialog.FileName);
                MessageBox.Show("Zapisano");
            }
        }
    }
}
