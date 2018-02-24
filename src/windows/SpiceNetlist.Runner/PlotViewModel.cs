using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots;
using System;

namespace SpiceNetlist.Runner
{
    class PlotViewModel
    {
        public PlotViewModel(Plot plot, bool xLog= false, bool yLog = false)
        {
            var tmp = new PlotModel { Title = plot.Name };
          
            for (var i = 0; i < plot.Series.Count; i++)
            {
                var series = new LineSeries { Title = plot.Series[i].Name, MarkerType = MarkerType.None };
                tmp.Series.Add(series);

                for (var j = 0; j < plot.Series[i].Points.Count; j++)
                {
                    var y = plot.Series[i].Points[j].Y;
                    series.Points.Add(new DataPoint(plot.Series[i].Points[j].X, y));
                }
            }

            if (xLog)
            {
                tmp.Axes.Add(new LogarithmicAxis { Position = AxisPosition.Bottom });
                tmp.Axes.Add(new LinearAxis { Position = AxisPosition.Left });
            }

            if (yLog)
            {
                tmp.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
                tmp.Axes.Add(new LogarithmicAxis { Position = AxisPosition.Left });
            }

            Model = tmp;
        }

        /// <summary>
        /// Gets the plot model.
        /// </summary>
        public PlotModel Model { get; private set; }
    }
}
