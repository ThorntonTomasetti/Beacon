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
using System.Reflection;
using System.IO;
using System.Diagnostics;

using System.Collections.ObjectModel;

using Beacon.Properties;

namespace Beacon
{
    /// <summary>
    /// Interaction logic for MainUI.xaml
    /// </summary>
    public partial class MainUI : Window
    {
        private RevitReader a_RevitReader;
        private List<string> a_buildingTypes;
        private List<GwpData> a_ConcreteGwpDataList = new List<GwpData>();
        private List<GwpData> a_SteelGwpDataList = new List<GwpData>();
        private List<GwpData> a_TimberGwpDataList = new List<GwpData>();
        private List<GwpData> a_UnknownGwpDataList = new List<GwpData>();
        private List<Totals> a_TotalsList = new List<Totals>();
        private Collection<PlotData> a_categoryPlotData = new Collection<PlotData>();
        private Collection<PlotData> a_levelPlotData = new Collection<PlotData>();
        private bool a_isCategoryPlot = true;
        private bool a_isInitialing = true;
        private double a_totalArea = 0.0;

        private bool a_SteelTabFirstFocus = true;
        private bool a_ConcreteTabFirstFocus = true;
        private bool a_TimberTabFirstFocus = true;
        private bool a_UnknownTabFirstFocus = true;

        /// <summary>
        /// Beacon main UI
        /// </summary>
        /// <param name="revitReader">Provide RevitReader instance</param>
        public MainUI(RevitReader revitReader)
        {
            InitializeComponent();
            a_RevitReader = revitReader;
            RatingLeft.Fill = new SolidColorBrush(Colors.White);
            RatingCenter.Fill = new SolidColorBrush(Colors.White);
            RatingRight.Fill = new SolidColorBrush(Colors.White);

            // Set Icon
            this.Icon = App.BeaconIcon;

            // Building Types
            a_buildingTypes = new List<string>()
            {
                "Commercial","Residential","Education","Health Care","Lodging","Mixed","Multi-Family","Office","Public Assembly","Other"
            };
            BuildingUseComboBox.ItemsSource = a_buildingTypes;
            BuildingUseComboBox.SelectedIndex = 0;

            // Initialize GWP
            InitializeGwpData();

            // Set DataGrid ToolTips
            SetDataGridColumnToolTips();

            // Parse Revit Data
            ParseByCategoryAndLevel();

            // Display Plot
            ColumnSeriesByCategory columnSeriesByCategory = new ColumnSeriesByCategory(a_categoryPlotData);
            MainPlotView.Model = columnSeriesByCategory.MyModel;
            // Change to display bar values on hover instead of click.
            OxyPlot.PlotController customController = new OxyPlot.PlotController();
            customController.Bind(new OxyPlot.OxyMouseEnterGesture(), OxyPlot.PlotCommands.HoverSnapTrack);
            MainPlotView.Controller = customController;

            CategoryButton.FontWeight = FontWeights.Bold;
            LevelButton.FontWeight = FontWeights.Regular;
            a_isCategoryPlot = true;
            a_isInitialing = false;

            this.VersionLabel.Content = "v" + typeof(BeaconCommand).Assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Initialize GWP data
        /// </summary>
        private void InitializeGwpData()
        {
            a_ConcreteGwpDataList.Clear();
            a_SteelGwpDataList.Clear();
            a_TimberGwpDataList.Clear();
            a_UnknownGwpDataList.Clear();
            a_ConcreteGwpDataList.Clear();
            BuildGwpData(true, true, true, true);
            RestoreGwpData();
            ConcreteGWPDataGrid.ItemsSource = a_ConcreteGwpDataList;
            SteelGWPDataGrid.ItemsSource = a_SteelGwpDataList;
            TimberGWPDataGrid.ItemsSource = a_TimberGwpDataList;
            UnknownGWPDataGrid.ItemsSource = a_UnknownGwpDataList;
            RebarGWPDataGrid.ItemsSource = a_ConcreteGwpDataList;
        }

        /// <summary>
        /// Set DataGrid ToolTips
        /// </summary>
        private void SetDataGridColumnToolTips()
        {
            var revitCatToolTip = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            revitCatToolTip.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "Revit Category"));
            SteelGWPDataGrid.Columns[0].HeaderStyle = revitCatToolTip;
            ConcreteGWPDataGrid.Columns[0].HeaderStyle = revitCatToolTip;
            TimberGWPDataGrid.Columns[0].HeaderStyle = revitCatToolTip;
            UnknownGWPDataGrid.Columns[0].HeaderStyle = revitCatToolTip;
            RebarGWPDataGrid.Columns[0].HeaderStyle = revitCatToolTip;

            var revitMaterialToolTip = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            revitMaterialToolTip.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "Revit Material Name"));
            SteelGWPDataGrid.Columns[1].HeaderStyle = revitMaterialToolTip;
            ConcreteGWPDataGrid.Columns[1].HeaderStyle = revitMaterialToolTip;
            TimberGWPDataGrid.Columns[1].HeaderStyle = revitMaterialToolTip;
            UnknownGWPDataGrid.Columns[1].HeaderStyle = revitMaterialToolTip;
            RebarGWPDataGrid.Columns[1].HeaderStyle = revitMaterialToolTip;

            var volumeFactorToolTip = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            volumeFactorToolTip.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "Use this factor to increase or decrease the volume"));
            SteelGWPDataGrid.Columns[2].HeaderStyle = volumeFactorToolTip;
            ConcreteGWPDataGrid.Columns[2].HeaderStyle = volumeFactorToolTip;
            TimberGWPDataGrid.Columns[2].HeaderStyle = volumeFactorToolTip;
            UnknownGWPDataGrid.Columns[2].HeaderStyle = volumeFactorToolTip;

            var volumeToolTip = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            volumeToolTip.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "Total volume for Category-Material group"));
            SteelGWPDataGrid.Columns[3].HeaderStyle = volumeToolTip;
            ConcreteGWPDataGrid.Columns[3].HeaderStyle = volumeToolTip;
            TimberGWPDataGrid.Columns[3].HeaderStyle = volumeToolTip;
            UnknownGWPDataGrid.Columns[3].HeaderStyle = volumeToolTip;

            var densityToolTip = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            densityToolTip.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "Density for Category-Material group"));
            SteelGWPDataGrid.Columns[4].HeaderStyle = densityToolTip;
            ConcreteGWPDataGrid.Columns[4].HeaderStyle = densityToolTip;
            TimberGWPDataGrid.Columns[4].HeaderStyle = densityToolTip;
            UnknownGWPDataGrid.Columns[4].HeaderStyle = densityToolTip;

            var gwpTypeToolTip = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            gwpTypeToolTip.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "Predefined GWP Types"));
            SteelGWPDataGrid.Columns[5].HeaderStyle = gwpTypeToolTip;
            ConcreteGWPDataGrid.Columns[5].HeaderStyle = gwpTypeToolTip;
            TimberGWPDataGrid.Columns[5].HeaderStyle = gwpTypeToolTip;
            UnknownGWPDataGrid.Columns[5].HeaderStyle = gwpTypeToolTip;

            var gwpValueToolTip = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            gwpValueToolTip.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "Predefined GWP Value or enter custom GWP value"));
            SteelGWPDataGrid.Columns[6].HeaderStyle = gwpValueToolTip;
            ConcreteGWPDataGrid.Columns[6].HeaderStyle = gwpValueToolTip;
            TimberGWPDataGrid.Columns[6].HeaderStyle = gwpValueToolTip;
            UnknownGWPDataGrid.Columns[6].HeaderStyle = gwpValueToolTip;
            RebarGWPDataGrid.Columns[6].HeaderStyle = gwpValueToolTip;

            var quantityToolTip = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            quantityToolTip.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "Total quantity of concrete in cubic yards or square feet"));
            RebarGWPDataGrid.Columns[2].HeaderStyle = quantityToolTip;

            var multiplierToolTip = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            multiplierToolTip.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "Multiply this value by Quantity to calculate a rebar weight"));
            RebarGWPDataGrid.Columns[3].HeaderStyle = multiplierToolTip;

            var multiplierUnitToolTip = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            multiplierUnitToolTip.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "Multiplier Unit"));
            RebarGWPDataGrid.Columns[4].HeaderStyle = multiplierUnitToolTip;

            var weightToolTip = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            weightToolTip.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "Calculated rebar weight from (Quantity * Multiplier) or enter custom weight"));
            RebarGWPDataGrid.Columns[5].HeaderStyle = weightToolTip;
        }

        /// <summary>
        /// Compare model EC to benchmark and show red, yellow, or green
        /// </summary>
        private void SetBeaconColor()
        {
            var total = a_TotalsList.Where(x => x.Name == "Total");
            if (total.Count() > 0)
            {
                var totalItem = total.First();
                double areaInMeters = a_totalArea / 10.764; // convert from ft2 to m2
                double rating = totalItem.Value / areaInMeters;
                double firstQuartile = 0.0, secondQuartile = 0.0, thirdQuartile = 0.0;
                switch (BuildingUseComboBox.SelectedItem.ToString())
                {
                    case "Commercial":
                        firstQuartile = 578; secondQuartile = 402; thirdQuartile = 263;
                        break;
                    case "Residential":
                        firstQuartile = 495; secondQuartile = 401; thirdQuartile = 304;
                        break;
                    case "Education":
                        firstQuartile = 624; secondQuartile = 381; thirdQuartile = 246;
                        break;
                    case "Health Care":
                        firstQuartile = 540; secondQuartile = 327; thirdQuartile = 243;
                        break;
                    case "Lodging":
                        firstQuartile = 533; secondQuartile = 368; thirdQuartile = 304;
                        break;
                    case "Mixed":
                        firstQuartile = 574; secondQuartile = 438; thirdQuartile = 311;
                        break;
                    case "Multi-Family":
                        firstQuartile = 472; secondQuartile = 404; thirdQuartile = 342;
                        break;
                    case "Office":
                        firstQuartile = 521; secondQuartile = 424; thirdQuartile = 297;
                        break;
                    case "Public Assembly":
                        firstQuartile = 614; secondQuartile = 434; thirdQuartile = 254;
                        break;
                    case "Other":
                        firstQuartile = 564; secondQuartile = 377; thirdQuartile = 266;
                        break;
                    default:
                        break;
                }
                SolidColorBrush baseColor = new SolidColorBrush(Colors.White);
                SolidColorBrush redColor = new SolidColorBrush(Colors.Red);
                SolidColorBrush yellowColor = new SolidColorBrush(Colors.Yellow);
                SolidColorBrush greenColor = new SolidColorBrush(Colors.Green);
                double topOfRange = secondQuartile + (secondQuartile * 0.1);
                double bottomOfRange = secondQuartile - (secondQuartile * 0.1);
                RatingLabel.Content = Math.Round(rating, 0).ToString() + " kg-CO2e/m2";
                string medianLabel = Math.Round(secondQuartile, 0).ToString() + " kg-CO2e/m2";
                if (rating > topOfRange)
                {
                    RatingCenter.Fill = redColor;
                    RatingCenter.Stroke = redColor;
                    RatingCenter.ToolTip = "Above +10% of median (" + medianLabel + ") for " + BuildingUseComboBox.SelectedItem.ToString();
                    RatingRight.Fill = yellowColor;
                    RatingRight.Stroke = yellowColor;
                    RatingRight.ToolTip = "Within +/-10% of median (" + medianLabel + ") for " + BuildingUseComboBox.SelectedItem.ToString();
                    RatingLeft.Fill = greenColor;
                    RatingLeft.Stroke = greenColor;
                    RatingLeft.ToolTip = "Below -10% of median (" + medianLabel + ") for " + BuildingUseComboBox.SelectedItem.ToString();
                }
                else if (rating <= topOfRange && rating >= bottomOfRange)
                {
                    RatingCenter.Fill = yellowColor;
                    RatingCenter.Stroke = yellowColor;
                    RatingCenter.ToolTip = "Within +/-10% of median (" + medianLabel + ") for " + BuildingUseComboBox.SelectedItem.ToString();
                    RatingRight.Fill = greenColor;
                    RatingRight.Stroke = greenColor;
                    RatingRight.ToolTip = "Below -10% of median (" + medianLabel + ") for " + BuildingUseComboBox.SelectedItem.ToString();
                    RatingLeft.Fill = redColor;
                    RatingLeft.Stroke = redColor;
                    RatingLeft.ToolTip = "Above +10% of median (" + medianLabel + ") for " + BuildingUseComboBox.SelectedItem.ToString();
                }
                else
                {
                    RatingCenter.Fill = greenColor;
                    RatingCenter.Stroke = greenColor;
                    RatingCenter.ToolTip = "Below -10% of median (" + medianLabel + ") for " + BuildingUseComboBox.SelectedItem.ToString();
                    RatingRight.Fill = redColor;
                    RatingRight.Stroke = redColor;
                    RatingRight.ToolTip = "Above +10% of median (" + medianLabel + ") for " + BuildingUseComboBox.SelectedItem.ToString();
                    RatingLeft.Fill = yellowColor;
                    RatingLeft.Stroke = yellowColor;
                    RatingLeft.ToolTip = "Within +/-10% of median (" + medianLabel + ") for " + BuildingUseComboBox.SelectedItem.ToString();
                }
            }
        }

        /// <summary>
        /// Restore saved user inputs.
        /// </summary>
        private void RestoreGwpData()
        {
            foreach (var gwpData in a_ConcreteGwpDataList)
            {
                GwpData savedGwpData = GetSavedGwpSetting(gwpData, Settings.Default.SavedGwpsConcrete);
                if (savedGwpData != null)
                {
                    gwpData.GwpSelectedIndex = savedGwpData.GwpSelectedIndex;
                    gwpData.Gwp = savedGwpData.Gwp;
                    gwpData.VolumeFactor = savedGwpData.VolumeFactor;
                    gwpData.RebarWeightMultiplier = savedGwpData.RebarWeightMultiplier;
                    gwpData.RebarWeight = savedGwpData.RebarWeight;
                    gwpData.RebarGwp = savedGwpData.RebarGwp;
                }
            }

            foreach (var gwpData in a_SteelGwpDataList)
            {
                GwpData savedGwpData = GetSavedGwpSetting(gwpData, Settings.Default.SavedGwpsSteel);
                if (savedGwpData != null)
                {
                    gwpData.GwpSelectedIndex = savedGwpData.GwpSelectedIndex;
                    gwpData.Gwp = savedGwpData.Gwp;
                    gwpData.VolumeFactor = savedGwpData.VolumeFactor;
                }
            }

            foreach (var gwpData in a_TimberGwpDataList)
            {
                GwpData savedGwpData = GetSavedGwpSetting(gwpData, Settings.Default.SavedGwpsTimber);
                if (savedGwpData != null)
                {
                    gwpData.GwpSelectedIndex = savedGwpData.GwpSelectedIndex;
                    gwpData.Gwp = savedGwpData.Gwp;
                    gwpData.VolumeFactor = savedGwpData.VolumeFactor;
                }
            }

            foreach (var gwpData in a_UnknownGwpDataList)
            {
                GwpData savedGwpData = GetSavedGwpSetting(gwpData, Settings.Default.SavedGwpsUnknown);
                if (savedGwpData != null)
                {
                    gwpData.GwpSelectedIndex = savedGwpData.GwpSelectedIndex;
                    gwpData.Gwp = savedGwpData.Gwp;
                    gwpData.VolumeFactor = savedGwpData.VolumeFactor;
                }
            }
        }

        /// <summary>
        /// Group RevitElements into GwpData by Category and Material Name
        /// </summary>
        /// <param name="setConcrete">Process Concrete?</param>
        /// <param name="setSteel">Process Steel?</param>
        /// <param name="setTimber">Process Timber?</param>
        /// <param name="setUnknown">Process Unknown?</param>
        private void BuildGwpData(bool setConcrete, bool setSteel, bool setTimber, bool setUnknown)
        {
            foreach (var revitElement in a_RevitReader.RevitElementData)
            {
                switch (revitElement.Material)
                {
                    case MaterialType.Concrete:
                        if (setConcrete)
                        {
                            var gwpConcreteDataList = a_ConcreteGwpDataList.Where(x => x.Category == revitElement.Category && x.MaterialName == revitElement.MaterialName);
                            if (gwpConcreteDataList.Count() == 0)
                            {
                                GwpData gwpData = new GwpData(revitElement.Category, revitElement.MaterialName);
                                gwpData.RevitElements.Add(revitElement);
                                if (revitElement.Category == RevitCategory.Floor)
                                {
                                    gwpData.RebarEstimateBasis += revitElement.Area;
                                }
                                else
                                {
                                    gwpData.RebarEstimateBasis += revitElement.Volume / 27;
                                }
                                gwpData.rebarLevelBreakdown.Add(revitElement.AssociatedLevel, gwpData.RebarEstimateBasis);
                                gwpData.PopulateConcreteGwpList();
                                switch (revitElement.Category)
                                {
                                    case RevitCategory.Framing:
                                        gwpData.RebarWeightMultiplier = 200;
                                        gwpData.GwpSelectedIndex = gwpData.ConcreteGwpList.FindIndex(x => x.Name == "6000-00-FA/SL");
                                        gwpData.GwpSelected = gwpData.ConcreteGwpList.First(x => x.Name == "6000-00-FA/SL");
                                        gwpData.Gwp = gwpData.GwpSelected.Value;
                                        break;
                                    case RevitCategory.Column:
                                        gwpData.RebarWeightMultiplier = 150;
                                        gwpData.GwpSelectedIndex = gwpData.ConcreteGwpList.FindIndex(x => x.Name == "8000-00-FA/SL");
                                        gwpData.GwpSelected = gwpData.ConcreteGwpList.First(x => x.Name == "8000-00-FA/SL");
                                        gwpData.Gwp = gwpData.GwpSelected.Value;
                                        break;
                                    case RevitCategory.Floor:
                                        gwpData.RebarWeightMultiplier = 6;
                                        gwpData.GwpSelectedIndex = gwpData.ConcreteGwpList.FindIndex(x => x.Name == "6000-00-FA/SL");
                                        gwpData.GwpSelected = gwpData.ConcreteGwpList.First(x => x.Name == "6000-00-FA/SL");
                                        gwpData.Gwp = gwpData.GwpSelected.Value;
                                        break;
                                    case RevitCategory.Wall:
                                        gwpData.RebarWeightMultiplier = 250;
                                        gwpData.GwpSelectedIndex = gwpData.ConcreteGwpList.FindIndex(x => x.Name == "8000-00-FA/SL");
                                        gwpData.GwpSelected = gwpData.ConcreteGwpList.First(x => x.Name == "8000-00-FA/SL");
                                        gwpData.Gwp = gwpData.GwpSelected.Value;
                                        break;
                                    case RevitCategory.Foundation:
                                        gwpData.RebarWeightMultiplier = 200;
                                        gwpData.GwpSelectedIndex = gwpData.ConcreteGwpList.FindIndex(x => x.Name == "4000-00-FA/SL");
                                        gwpData.GwpSelected = gwpData.ConcreteGwpList.First(x => x.Name == "4000-00-FA/SL");
                                        gwpData.Gwp = gwpData.GwpSelected.Value;
                                        break;
                                    default:
                                        gwpData.RebarWeightMultiplier = 0.0;
                                        break;
                                }
                                gwpData.Volume += revitElement.Volume;
                                gwpData.Density = revitElement.Density;
                                a_ConcreteGwpDataList.Add(gwpData);
                            }
                            else
                            {
                                var gwpConcreteData = gwpConcreteDataList.First();
                                gwpConcreteData.RevitElements.Add(revitElement);
                                gwpConcreteData.Volume += revitElement.Volume;
                                double localRebarEstimateBasis = 0.0;
                                if (revitElement.Category == RevitCategory.Floor)
                                {
                                    localRebarEstimateBasis = revitElement.Area;
                                    gwpConcreteData.RebarEstimateBasis += revitElement.Area;
                                }
                                else
                                {
                                    localRebarEstimateBasis = revitElement.Volume / 27;
                                    gwpConcreteData.RebarEstimateBasis += revitElement.Volume / 27;
                                }
                                double rebarEstimateValue;
                                if (gwpConcreteData.rebarLevelBreakdown.TryGetValue(revitElement.AssociatedLevel, out rebarEstimateValue))
                                {
                                    rebarEstimateValue += localRebarEstimateBasis;
                                    gwpConcreteData.rebarLevelBreakdown.Remove(revitElement.AssociatedLevel);
                                    gwpConcreteData.rebarLevelBreakdown.Add(revitElement.AssociatedLevel, rebarEstimateValue);
                                }
                                else
                                {
                                    gwpConcreteData.rebarLevelBreakdown.Add(revitElement.AssociatedLevel, localRebarEstimateBasis);
                                }
                            }
                        }
                        break;
                    case MaterialType.Steel:
                        if (setSteel)
                        {
                            var gwpSteelDataList = a_SteelGwpDataList.Where(x => x.Category == revitElement.Category && x.MaterialName == revitElement.MaterialName);
                            if (gwpSteelDataList.Count() == 0)
                            {
                                GwpData gwpData = new GwpData(revitElement.Category, revitElement.MaterialName);
                                gwpData.PopulateSteelGwpList();
                                if (revitElement.MaterialName.ToUpper().Contains("A992"))
                                {
                                    gwpData.GwpSelectedIndex = gwpData.SteelGwpList.FindIndex(x => x.Name == "Primary Steel");
                                    gwpData.GwpSelected = gwpData.SteelGwpList.First(x => x.Name == "Primary Steel");
                                    gwpData.Gwp = gwpData.GwpSelected.Value;
                                }
                                else if (revitElement.MaterialName.ToUpper().Contains("A500"))
                                {
                                    gwpData.GwpSelectedIndex = gwpData.SteelGwpList.FindIndex(x => x.Name == "HSS Steel");
                                    gwpData.GwpSelected = gwpData.SteelGwpList.First(x => x.Name == "HSS Steel");
                                    gwpData.Gwp = gwpData.GwpSelected.Value;
                                }
                                gwpData.Volume += revitElement.Volume;
                                gwpData.Density = revitElement.Density;
                                a_SteelGwpDataList.Add(gwpData);
                            }
                            else
                            {
                                GwpData foundGwpData = gwpSteelDataList.First();
                                foundGwpData.Volume += revitElement.Volume;
                            }
                        }
                        break;
                    case MaterialType.Timber:
                        if (setTimber)
                        {
                            var gwpTimberDataList = a_TimberGwpDataList.Where(x => x.Category == revitElement.Category && x.MaterialName == revitElement.MaterialName);
                            if (gwpTimberDataList.Count() == 0)
                            {
                                GwpData gwpData = new GwpData(revitElement.Category, revitElement.MaterialName);
                                gwpData.PopulateTimberGwpList();
                                if (revitElement.MaterialName.ToUpper().Contains("SOFTWOOD") && revitElement.MaterialName.ToUpper().Contains("LUMBER"))
                                {
                                    gwpData.GwpSelectedIndex = gwpData.TimberGwpList.FindIndex(x => x.Name == "Softwood Lumber");
                                    gwpData.GwpSelected = gwpData.TimberGwpList.First(x => x.Name == "Softwood Lumber");
                                    gwpData.Gwp = gwpData.GwpSelected.Value;
                                }
                                else if (revitElement.MaterialName.ToUpper().Contains("SOFTWOOD") && revitElement.MaterialName.ToUpper().Contains("PLYWOOD"))
                                {
                                    gwpData.GwpSelectedIndex = gwpData.TimberGwpList.FindIndex(x => x.Name == "Softwood Plywood");
                                    gwpData.GwpSelected = gwpData.TimberGwpList.First(x => x.Name == "Softwood Plywood");
                                    gwpData.Gwp = gwpData.GwpSelected.Value;
                                }
                                else if (revitElement.MaterialName.ToUpper().Contains("STRAND"))
                                {
                                    gwpData.GwpSelectedIndex = gwpData.TimberGwpList.FindIndex(x => x.Name == "Oriented Strand Board");
                                    gwpData.GwpSelected = gwpData.TimberGwpList.First(x => x.Name == "Oriented Strand Board");
                                    gwpData.Gwp = gwpData.GwpSelected.Value;
                                }
                                else if (revitElement.MaterialName.ToUpper().Contains("GLULAM"))
                                {
                                    gwpData.GwpSelectedIndex = gwpData.TimberGwpList.FindIndex(x => x.Name == "Glulam");
                                    gwpData.GwpSelected = gwpData.TimberGwpList.First(x => x.Name == "Glulam");
                                    gwpData.Gwp = gwpData.GwpSelected.Value;
                                }
                                else if (revitElement.MaterialName.ToUpper().Contains("LVL") || revitElement.MaterialName.ToUpper().Contains("LAMINATED VENEER LUMBER"))
                                {
                                    gwpData.GwpSelectedIndex = gwpData.TimberGwpList.FindIndex(x => x.Name == "Laminated Veneer Lumber");
                                    gwpData.GwpSelected = gwpData.TimberGwpList.First(x => x.Name == "Laminated Veneer Lumber");
                                    gwpData.Gwp = gwpData.GwpSelected.Value;
                                }
                                gwpData.Volume += revitElement.Volume;
                                gwpData.Density = revitElement.Density;
                                a_TimberGwpDataList.Add(gwpData);
                            }
                            else
                            {
                                GwpData foundGwpData = gwpTimberDataList.First();
                                foundGwpData.Volume += revitElement.Volume;
                            }
                        }
                        break;
                    case MaterialType.Unknown:
                        if (setUnknown)
                        {
                            var gwpUnknownDataList = a_UnknownGwpDataList.Where(x => x.Category == revitElement.Category && x.MaterialName == revitElement.MaterialName);
                            if (gwpUnknownDataList.Count() == 0)
                            {
                                GwpData gwpData = new GwpData(revitElement.Category, revitElement.MaterialName);
                                gwpData.PopulateAllGwpList();
                                gwpData.Volume += revitElement.Volume;
                                gwpData.Density = revitElement.Density;
                                a_UnknownGwpDataList.Add(gwpData);
                            }
                            else
                            {
                                GwpData foundGwpData = gwpUnknownDataList.First();
                                foundGwpData.Volume += revitElement.Volume;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            // Set unknown tab to Bold if not zero count. Only on initial time when all sets are true.
            if (a_UnknownGwpDataList.Count > 0 && setSteel && setConcrete && setTimber && setUnknown)
            {
                UnknownTab.FontWeight = FontWeights.Bold;
                UnknownTab.IsSelected = true;
            }
        }

        /// <summary>
        /// Calculate EC based on selected GwpData
        /// </summary>
        private void ParseByCategoryAndLevel()
        {
            a_categoryPlotData.Clear();
            a_levelPlotData.Clear();
            a_totalArea = 0.0;
            Dictionary<string, double> rebarECByCatDict = new Dictionary<string, double>();
            Dictionary<string, Tuple<string, double>> rebarECByLevelDict = new Dictionary<string, Tuple<string, double>>();
            foreach (var revitElement in a_RevitReader.RevitElementData)
            {
                if (revitElement.Area > 0) a_totalArea += revitElement.Area;
                double unknownEC = 0.0, steelEC = 0.0, concreteEC = 0.0, timberEC = 0.0;
                switch (revitElement.Material)
                {
                    case MaterialType.Steel:
                        var steelGwpList = a_SteelGwpDataList.Where(x => x.Category == revitElement.Category && x.MaterialName == revitElement.MaterialName);
                        if (steelGwpList.Count() == 1)
                        {
                            GwpData gwpData = steelGwpList.First();
                            revitElement.VolumeFactor = gwpData.VolumeFactor;
                            revitElement.FactoredVolume = revitElement.Volume * gwpData.VolumeFactor;
                            revitElement.FactoredWeight = revitElement.FactoredVolume * revitElement.Density;
                            steelEC = gwpData.Gwp * (revitElement.FactoredWeight / 2000);
                            revitElement.GwpType = gwpData.Gwp == gwpData.GwpSelected.Value ? gwpData.GwpSelected.Name : "User Input";
                            revitElement.Gwp = gwpData.Gwp;
                            revitElement.EmbodiedCarbon = steelEC;
                        }
                        break;
                    case MaterialType.Concrete:
                        var concreteGwpList = a_ConcreteGwpDataList.Where(x => x.Category == revitElement.Category && x.MaterialName == revitElement.MaterialName);
                        if (concreteGwpList.Count() == 1)
                        {
                            // Concrete
                            GwpData gwpData = concreteGwpList.First();
                            revitElement.VolumeFactor = gwpData.VolumeFactor;
                            revitElement.FactoredVolume = revitElement.Volume * gwpData.VolumeFactor;
                            revitElement.FactoredWeight = revitElement.FactoredVolume * revitElement.Density;
                            concreteEC = gwpData.Gwp * (revitElement.FactoredVolume / 27);
                            revitElement.GwpType = gwpData.Gwp == gwpData.GwpSelected.Value ? gwpData.GwpSelected.Name : "User Input";
                            revitElement.Gwp = gwpData.Gwp;
                            revitElement.EmbodiedCarbon = concreteEC;

                            // Element Rebar Estimate
                            revitElement.RebarMultiplier = gwpData.RebarWeightMultiplier;
                            if (revitElement.Category == RevitCategory.Floor)
                            {
                                double rebarWeight = gwpData.VolumeFactor * gwpData.RebarWeightMultiplier * revitElement.Area;
                                revitElement.RebarWeight = (rebarWeight / gwpData.GetTotalRebarWeight()) * gwpData.RebarWeight;
                            }
                            else
                            {
                                double rebarWeight = gwpData.VolumeFactor * gwpData.RebarWeightMultiplier * revitElement.Volume / 27;
                                revitElement.RebarWeight = (rebarWeight / gwpData.GetTotalRebarWeight()) * gwpData.RebarWeight;
                            }
                            revitElement.RebarGwp = gwpData.RebarGwp;
                            revitElement.RebarEmbodiedCarbon = gwpData.RebarGwp * (revitElement.RebarWeight / 2000);

                            // Category and Level Rebar Estimate
                            if (rebarECByCatDict.ContainsKey(revitElement.Category.ToString()) == false)
                            {
                                double rebarECValue = gwpData.RebarGwp * (gwpData.RebarWeight / 2000);
                                rebarECByCatDict.Add(revitElement.Category.ToString(), rebarECValue);

                                foreach (var levelRatio in gwpData.rebarLevelRatio)
                                {
                                    rebarECByLevelDict.Add(revitElement.Category.ToString() + levelRatio.Key, Tuple.Create(levelRatio.Key, levelRatio.Value * rebarECValue));
                                }
                            }
                        }
                        break;
                    case MaterialType.Timber:
                        var timberGwpList = a_TimberGwpDataList.Where(x => x.Category == revitElement.Category && x.MaterialName == revitElement.MaterialName);
                        if (timberGwpList.Count() == 1)
                        {
                            GwpData gwpData = timberGwpList.First();
                            revitElement.VolumeFactor = gwpData.VolumeFactor;
                            revitElement.FactoredVolume = revitElement.Volume * gwpData.VolumeFactor;
                            revitElement.FactoredWeight = revitElement.FactoredVolume * revitElement.Density;
                            timberEC = gwpData.Gwp * (revitElement.FactoredVolume / 27);
                            revitElement.GwpType = gwpData.Gwp == gwpData.GwpSelected.Value ? gwpData.GwpSelected.Name : "User Input";
                            revitElement.Gwp = gwpData.Gwp;
                            revitElement.EmbodiedCarbon = timberEC;
                        }
                        break;
                    case MaterialType.Unknown:
                        var unknownGwpList = a_UnknownGwpDataList.Where(x => x.Category == revitElement.Category && x.MaterialName == revitElement.MaterialName);
                        if (unknownGwpList.Count() == 1)
                        {
                            GwpData gwpData = unknownGwpList.First();
                            revitElement.VolumeFactor = gwpData.VolumeFactor;
                            revitElement.FactoredVolume = revitElement.Volume * gwpData.VolumeFactor;
                            revitElement.FactoredWeight = revitElement.FactoredVolume * revitElement.Density;
                            if (gwpData.GwpSelected.GwpType == MultiplierType.Volume)
                                unknownEC = gwpData.Gwp * (revitElement.FactoredVolume / 27);
                            else
                                unknownEC = gwpData.Gwp * (revitElement.FactoredWeight / 2000);
                            revitElement.GwpType = gwpData.Gwp == gwpData.GwpSelected.Value ? gwpData.GwpSelected.Name : "User Input";
                            revitElement.Gwp = gwpData.Gwp;
                            revitElement.EmbodiedCarbon = unknownEC;
                        }
                        break;
                    default:
                        break;
                }

                // Category
                var foundCatData = a_categoryPlotData.Where(c => c.Label == revitElement.Category.ToString());
                PlotData categoryPlotData = null;
                if (foundCatData.Count() == 0)
                {
                    categoryPlotData = new PlotData(revitElement.Category.ToString(), steelEC, concreteEC, timberEC, unknownEC, 0, 0);
                    a_categoryPlotData.Add(categoryPlotData);
                }
                else if (foundCatData.Count() > 0)
                {
                    categoryPlotData = foundCatData.First();
                    categoryPlotData.Steel += steelEC;
                    categoryPlotData.Concrete += concreteEC;
                    categoryPlotData.Timber += timberEC;
                    categoryPlotData.Unknown += unknownEC;
                }

                // Level
                var foundLevelData = a_levelPlotData.Where(c => c.Label == revitElement.AssociatedLevel);
                PlotData levelPlotData = null;
                if (foundLevelData.Count() == 0)
                {
                    levelPlotData = new PlotData(revitElement.AssociatedLevel, steelEC, concreteEC, timberEC, unknownEC, 0, revitElement.AssociatedElevation);
                    a_levelPlotData.Add(levelPlotData);
                }
                else if (foundLevelData.Count() > 0)
                {
                    levelPlotData = foundLevelData.First();
                    levelPlotData.Steel += steelEC;
                    levelPlotData.Concrete += concreteEC;
                    levelPlotData.Timber += timberEC;
                    levelPlotData.Unknown += unknownEC;
                }
            }

            // Category Rebar
            foreach (var catData in a_categoryPlotData)
            {
                double rebarEC = 0.0;
                if (rebarECByCatDict.TryGetValue(catData.Label, out rebarEC))
                {
                    catData.Rebar = rebarEC;
                }
            }
            // Level Rebar
            foreach (var levelRebarEC in rebarECByLevelDict)
            {
                var foundLevelData = a_levelPlotData.Where(c => c.Label == levelRebarEC.Value.Item1);
                if (foundLevelData.Count() > 0)
                {
                    PlotData levelPlotData = foundLevelData.First();
                    levelPlotData.Rebar += levelRebarEC.Value.Item2;
                }
            }

            // Sort Level by elevation
            a_levelPlotData = new Collection<PlotData>(a_levelPlotData.OrderBy(x => x.Elevation).ToList());

            // Calculate Totals
            a_TotalsList = new List<Totals>()
            {
                new Totals(MaterialType.Steel.ToString(), Math.Round(a_categoryPlotData.Sum(x => x.Steel))),
                new Totals(MaterialType.Concrete.ToString(), Math.Round(a_categoryPlotData.Sum(x => x.Concrete))),
                new Totals(MaterialType.Timber.ToString(), Math.Round(a_categoryPlotData.Sum(x => x.Timber))),
                new Totals(MaterialType.Rebar.ToString(), Math.Round(a_categoryPlotData.Sum(x => x.Rebar))),
                new Totals(MaterialType.Unknown.ToString(), Math.Round(a_categoryPlotData.Sum(x => x.Unknown))),
                new Totals("Total", Math.Round(a_categoryPlotData.Sum(x => x.Total))),
            };
            TotalDataGrid.ItemsSource = a_TotalsList;

            // Set Beacon Color
            SetBeaconColor();
        }

        /// <summary>
        /// Display plot by category
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
            a_isCategoryPlot = true;
            CategoryButton.FontWeight = FontWeights.Bold;
            LevelButton.FontWeight = FontWeights.Regular;
            ParseByCategoryAndLevel();
            ColumnSeriesByCategory columnSeriesByCategory = new ColumnSeriesByCategory(a_categoryPlotData);
            MainPlotView.Model = columnSeriesByCategory.MyModel;
        }

        /// <summary>
        /// Display plot by level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LevelButton_Click(object sender, RoutedEventArgs e)
        {
            a_isCategoryPlot = false;
            CategoryButton.FontWeight = FontWeights.Regular;
            LevelButton.FontWeight = FontWeights.Bold;
            ParseByCategoryAndLevel();
            BarSeriesByLevel barSeriesByLevel = new BarSeriesByLevel(a_levelPlotData);
            MainPlotView.Model = barSeriesByLevel.MyModel;
        }

        /// <summary>
        /// Refresh after change in concrete Gwp combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConcreteGwpTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (a_ConcreteTabFirstFocus == false)
            {
                ComboBox gwpComboBox = sender as ComboBox;
                string selectedGwp = gwpComboBox.SelectedItem as string;
                GwpData gwpData = gwpComboBox.DataContext as GwpData;
                GwpNameValue gwpNameValue = gwpData.ConcreteGwpList.First(x => x.Name == selectedGwp);
                gwpData.GwpSelected = gwpNameValue;
                gwpData.Gwp = gwpNameValue.Value;
                RefreshPlot();
            }
        }

        private void ConcreteTab_GotFocus(object sender, RoutedEventArgs e)
        {
            a_ConcreteTabFirstFocus = false;
        }

        /// <summary>
        /// Refresh after change in steel Gwp combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SteelGwpTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (a_SteelTabFirstFocus == false)
            {
                ComboBox gwpComboBox = sender as ComboBox;
                string selectedGwp = gwpComboBox.SelectedItem as string;
                GwpData gwpData = gwpComboBox.DataContext as GwpData;
                GwpNameValue gwpNameValue = gwpData.SteelGwpList.First(x => x.Name == selectedGwp);
                gwpData.GwpSelected = gwpNameValue;
                gwpData.Gwp = gwpNameValue.Value;
                RefreshPlot();
            }
        }

        private void SteelTab_GotFocus(object sender, RoutedEventArgs e)
        {
            a_SteelTabFirstFocus = false;
        }

        /// <summary>
        /// Refresh after change in timber combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimberGwpTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (a_TimberTabFirstFocus == false)
            {
                ComboBox gwpComboBox = sender as ComboBox;
                string selectedGwp = gwpComboBox.SelectedItem as string;
                GwpData gwpData = gwpComboBox.DataContext as GwpData;
                GwpNameValue gwpNameValue = gwpData.TimberGwpList.First(x => x.Name == selectedGwp);
                gwpData.GwpSelected = gwpNameValue;
                gwpData.Gwp = gwpNameValue.Value;
                RefreshPlot();
            }
        }

        private void TimberTab_GotFocus(object sender, RoutedEventArgs e)
        {
            a_TimberTabFirstFocus = false;
        }

        /// <summary>
        /// Refresh after change in unknown combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnknownGwpTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (a_UnknownTabFirstFocus == false)
            {
                ComboBox gwpComboBox = sender as ComboBox;
                string selectedGwp = gwpComboBox.SelectedItem as string;
                GwpData gwpData = gwpComboBox.DataContext as GwpData;
                GwpNameValue gwpNameValue = gwpData.AllGwpList.First(x => x.Name == selectedGwp);
                gwpData.GwpSelected = gwpNameValue;
                gwpData.Gwp = gwpNameValue.Value;
                RefreshPlot();
            }
        }

        private void UnknownTab_GotFocus(object sender, RoutedEventArgs e)
        {
            a_UnknownTabFirstFocus = false;
        }

        /// <summary>
        /// Refresh after updates in data grid cells
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RefreshPlot();
        }

        /// <summary>
        /// Refresh plot after data change
        /// </summary>
        private void RefreshPlot()
        {
            if (a_isInitialing == false)
            {
                if (a_isCategoryPlot)
                {
                    ParseByCategoryAndLevel();
                    ColumnSeriesByCategory columnSeriesByCategory = new ColumnSeriesByCategory(a_categoryPlotData);
                    MainPlotView.Model = columnSeriesByCategory.MyModel;
                }
                else
                {
                    ParseByCategoryAndLevel();
                    BarSeriesByLevel barSeriesByLevel = new BarSeriesByLevel(a_levelPlotData);
                    MainPlotView.Model = barSeriesByLevel.MyModel;
                }
            }
        }

        /// <summary>
        /// Update benchmark lights after change in Building Use combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BuildingUseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetBeaconColor();
        }

        /// <summary>
        /// Write provided list to CSV 
        /// </summary>
        /// <typeparam name="T">Type of object in list</typeparam>
        /// <param name="items">The list to write</param>
        /// <param name="path">The path for CSV file</param>
        public static void WriteCSV<T>(IEnumerable<T> items, string path)
        {
            Type itemType = typeof(T);
            var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance); //.OrderBy(p => p.Name);
            var propsFiltered = props.Where(x => x.Name != "AssociatedElevation");

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine(string.Join(", ", propsFiltered.Select(p =>
                    {
                        var header = p.Name.Replace(',', ';');
                        switch (header)
                        {
                            case "Volume":
                                header += " (CF)";
                                break;
                            case "Density":
                                header += " (PCF)";
                                break;
                            case "Weight":
                                header += " (LB)";
                                break;
                            case "Area":
                                header += " (SF)";
                                break;
                            case "EmbodiedCarbon":
                                header += " (KG)";
                                break;
                            case "RebarWeight":
                                header += " (LB)";
                                break;
                            case "RebarGwp":
                                header += " (KG/SHORT TON)";
                                break;
                            case "RebarEmbodiedCarbon":
                                header += " (KG)";
                                break;
                            default:
                                break;
                        }
                        return header;
                    })));

                foreach (var item in items)
                {
                    writer.WriteLine(string.Join(", ", propsFiltered.Select(p => {
                        var obj = p.GetValue(item, null);
                        if (obj is string) return ((string)obj).Replace(',', ';');
                        else return obj;
                    })));
                }
            }
        }

        /// <summary>
        /// Write to CSV
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveData_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var sorted = a_RevitReader.RevitElementData.OrderBy(x => x.Material.ToString()).ThenBy(y => y.MaterialName).ThenBy(z => z.Category.ToString());
                WriteCSV<RevitElement>(sorted, saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// Serialize user inputs for restore on next use
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string concreteGwps = Newtonsoft.Json.JsonConvert.SerializeObject(a_ConcreteGwpDataList);
            Settings.Default.SavedGwpsConcrete = concreteGwps;

            string steelGwps = Newtonsoft.Json.JsonConvert.SerializeObject(a_SteelGwpDataList);
            Settings.Default.SavedGwpsSteel = steelGwps;

            string timberGwps = Newtonsoft.Json.JsonConvert.SerializeObject(a_TimberGwpDataList);
            Settings.Default.SavedGwpsTimber = timberGwps;

            string unknownGwps = Newtonsoft.Json.JsonConvert.SerializeObject(a_UnknownGwpDataList);
            Settings.Default.SavedGwpsUnknown = unknownGwps;
        }

        /// <summary>
        /// Get saved GwpData selected from json
        /// </summary>
        /// <param name="gwpData"></param>
        /// <param name="savedGwpJson"></param>
        /// <returns></returns>
        private GwpData GetSavedGwpSetting(GwpData gwpData, string savedGwpJson)
        {
            GwpData onlySavedPropertiesGwp = null;
            List<GwpData> savedGwps = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GwpData>>(savedGwpJson);
            if (savedGwps != null)
            {
                var foundGwp = savedGwps.Where(x => x.Category == gwpData.Category && x.MaterialName == gwpData.MaterialName);
                if (foundGwp.Count() > 0)
                {
                    onlySavedPropertiesGwp = foundGwp.First();
                }
            }
            return onlySavedPropertiesGwp;
        }

        /// <summary>
        /// Reset Concrete GwpData
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetConcreteGwpButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string message = "Resetting Concrete will also reset Rebar. Continue?";
            if (button != null && button.Name == "ResetRebarGwpButton")
            {
                message = "Resetting Rebar will also reset Concrete. Continue?";
            }
            MessageBoxResult mResult = MessageBox.Show(message, "Reset Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (mResult == MessageBoxResult.Yes)
            {
                Settings.Default.SavedGwpsConcrete = string.Empty;
                ConcreteGWPDataGrid.ItemsSource = null;
                RebarGWPDataGrid.ItemsSource = null;
                a_ConcreteGwpDataList.Clear();
                BuildGwpData(true, false, false, false);
                ConcreteGWPDataGrid.ItemsSource = a_ConcreteGwpDataList;
                RebarGWPDataGrid.ItemsSource = a_ConcreteGwpDataList;
                RefreshPlot();
            }
        }

        /// <summary>
        /// Reset Steel GwpData
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetSteelGwpButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.SavedGwpsSteel = string.Empty;
            SteelGWPDataGrid.ItemsSource = null;
            a_SteelGwpDataList.Clear();
            BuildGwpData(false, true, false, false);
            SteelGWPDataGrid.ItemsSource = a_SteelGwpDataList;
            RefreshPlot();
        }

        /// <summary>
        /// Reset Tinber GwpData
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetTimberGwpButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.SavedGwpsTimber = string.Empty;
            TimberGWPDataGrid.ItemsSource = null;
            a_TimberGwpDataList.Clear();
            BuildGwpData(false, false, true, false);
            TimberGWPDataGrid.ItemsSource = a_TimberGwpDataList;
            RefreshPlot();
        }

        /// <summary>
        /// Reset Unknown GwpData
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetUnknownGwpButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.SavedGwpsUnknown = string.Empty;
            UnknownGWPDataGrid.ItemsSource = null;
            a_UnknownGwpDataList.Clear();
            BuildGwpData(false, false, false, true);
            UnknownGWPDataGrid.ItemsSource = a_UnknownGwpDataList;
            RefreshPlot();
        }

        /// <summary>
        /// Bring up detail element list UI on double click of data grid row
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            if (row != null)
            {
                GwpData gwpData = row.Item as GwpData;
                if (gwpData != null)
                {
                    var revitElements = a_RevitReader.RevitElementData.FindAll(x => x.Category == gwpData.Category && x.MaterialName == gwpData.MaterialName);
                    revitElements = revitElements.OrderBy(x => x.Name).ThenBy(y => y.AssociatedLevel).ToList();
                    string label = "Elements: " + gwpData.Category.ToString() + " - " + gwpData.MaterialName;
                    DataGridUI dataGridUI = new DataGridUI(revitElements, label);
                    dataGridUI.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    dataGridUI.ShowDialog();
                }

                Totals total = row.Item as Totals;
                if (total != null)
                {
                    var revitElements = a_RevitReader.RevitElementData.FindAll(x => x.Material.ToString() == total.Name);
                    if (total.Name == "Total")
                    {
                        revitElements = a_RevitReader.RevitElementData;
                    }
                    else if (total.Name == "Rebar")
                    {
                        revitElements = a_RevitReader.RevitElementData.FindAll(x => x.Material.ToString() == MaterialType.Concrete.ToString());
                    }
                    revitElements = revitElements.OrderBy(x => x.Material.ToString()).ThenBy(y => y.MaterialName).ThenBy(z => z.Category.ToString()).ToList();
                    string label = "Elements: " + total.Name;
                    DataGridUI dataGridUI = new DataGridUI(revitElements, label);
                    dataGridUI.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    dataGridUI.ShowDialog();
                }

            }
        }

        /// <summary>
        /// Navigate to Uri on link click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
