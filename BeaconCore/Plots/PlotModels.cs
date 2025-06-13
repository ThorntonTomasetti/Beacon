using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Beacon
{
    /// <summary>
    /// Column series plot by Category
    /// </summary>
    public class ColumnSeriesByCategory
    {
        public PlotModel MyModel { get; private set; }

        /// <summary>
        /// Column series plot by Category
        /// </summary>
        /// <param name="plotData">The PlotData to plot</param>
        public ColumnSeriesByCategory(Collection<PlotData> plotData)
        {
            // Create the plot model
            var tmp = new PlotModel { Title = "Embodied Carbon (kg CO2e)", LegendPlacement = LegendPlacement.Outside, LegendPosition = LegendPosition.RightBottom, LegendOrientation = LegendOrientation.Vertical };

            // Add the axes, note that MinimumPadding and AbsoluteMinimum should be set on the value axis.
            tmp.Axes.Add(new CategoryAxis { ItemsSource = plotData, LabelField = "Label", GapWidth = 0.2 });
            tmp.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MinimumPadding = 0, AbsoluteMinimum = 0 });

            // Add the series, note that the BarSeries are using the same ItemsSource as the CategoryAxis.
            tmp.Series.Add(new ColumnSeries { Title = "Total", ItemsSource = plotData, ValueField = "Total", FillColor = OxyColor.FromRgb(0, 0, 0) }); // black
            tmp.Series.Add(new ColumnSeries { Title = "Steel", ItemsSource = plotData, ValueField = "Steel", FillColor = OxyColor.FromRgb(0, 0, 255) }); // blue
            tmp.Series.Add(new ColumnSeries { Title = "Concrete", ItemsSource = plotData, ValueField = "Concrete", FillColor = OxyColor.FromRgb(128, 128, 128) }); // gray
            tmp.Series.Add(new ColumnSeries { Title = "Rebar", ItemsSource = plotData, ValueField = "Rebar", FillColor = OxyColor.FromRgb(255, 165, 0) }); // orange
            tmp.Series.Add(new ColumnSeries { Title = "Timber", ItemsSource = plotData, ValueField = "Timber", FillColor = OxyColor.FromRgb(0, 255, 255) }); // cyan
            tmp.Series.Add(new ColumnSeries { Title = "Unknown", ItemsSource = plotData, ValueField = "Unknown" , FillColor = OxyColor.FromRgb(255, 0, 0) }); // red

            this.MyModel = tmp;
        }
    }

    /// <summary>
    /// Bar series plot by level
    /// </summary>
    public class BarSeriesByLevel
    {
        public PlotModel MyModel { get; private set; }

        /// <summary>
        /// Bar series plot by level
        /// </summary>
        /// <param name="plotData">The PlotData to plot</param>
        public BarSeriesByLevel(Collection<PlotData> plotData)
        {
            // Create the plot model
            var tmp = new PlotModel { Title = "Embodied Carbon (kg CO2e)", LegendPlacement = LegendPlacement.Outside, LegendPosition = LegendPosition.RightBottom, LegendOrientation = LegendOrientation.Vertical };

            // Add the axes, note that MinimumPadding and AbsoluteMinimum should be set on the value axis.
            tmp.Axes.Add(new CategoryAxis { Position = AxisPosition.Left, ItemsSource = plotData, LabelField = "Label", GapWidth = 0.2 });
            tmp.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MinimumPadding = 0, AbsoluteMinimum = 0 });

            // Add the series, note that the BarSeries are using the same ItemsSource as the CategoryAxis.
            tmp.Series.Add(new BarSeries { Title = "Total", ItemsSource = plotData, ValueField = "Total", FillColor = OxyColor.FromRgb(0, 0, 0) }); // black
            tmp.Series.Add(new BarSeries { Title = "Steel", ItemsSource = plotData, ValueField = "Steel", FillColor = OxyColor.FromRgb(0, 0, 255) }); // blue
            tmp.Series.Add(new BarSeries { Title = "Concrete", ItemsSource = plotData, ValueField = "Concrete", FillColor = OxyColor.FromRgb(128, 128, 128) }); // gray
            tmp.Series.Add(new BarSeries { Title = "Rebar", ItemsSource = plotData, ValueField = "Rebar", FillColor = OxyColor.FromRgb(255, 165, 0) }); // orange
            tmp.Series.Add(new BarSeries { Title = "Timber", ItemsSource = plotData, ValueField = "Timber", FillColor = OxyColor.FromRgb(0, 255, 255) }); // cyan
            tmp.Series.Add(new BarSeries { Title = "Unknown", ItemsSource = plotData, ValueField = "Unknown", FillColor = OxyColor.FromRgb(255, 0, 0) }); // red

            this.MyModel = tmp;
        }
    }
}
