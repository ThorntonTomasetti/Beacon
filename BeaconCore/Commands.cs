using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using OxyPlot.Wpf;

namespace Beacon
{
    [Transaction(TransactionMode.Manual)]
    public class BeaconCommand : IExternalCommand
    {
        /// <summary>
        /// Overload method to implement Beacon external command within Revit.
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="msg"></param>
        /// <param name="elems"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string msg, ElementSet elems)
        {
            // Get the environment variables
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            RevitReader revitReader = new RevitReader(doc);

            // Create PlotView to load OxyPlot.Wpf assembly before calling wpf, plotView is not specifically used here.
            PlotView plotView = new PlotView();

            // Select Phases and Map Levels
            SettingUI settingUI = new SettingUI(revitReader);
            settingUI.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            bool? settingResult = settingUI.ShowDialog();

            if (settingResult == true)
            {
                revitReader.ReadElements();
                MainUI mainUI = new MainUI(revitReader);
                mainUI.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                mainUI.ShowDialog();
            }

            return Result.Succeeded;
        }
    }
}
