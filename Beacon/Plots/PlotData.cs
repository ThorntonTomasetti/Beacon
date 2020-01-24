using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using Newtonsoft.Json;

namespace Beacon
{
    /// <summary>
    /// Plotting data
    /// </summary>
    public class PlotData
    {
        /// <summary>
        /// Plotting data
        /// </summary>
        /// <param name="label">Name of category</param>
        /// <param name="steel">steel value</param>
        /// <param name="concrete">concrete value</param>
        /// <param name="timber">timber value</param>
        /// <param name="unknown">unknown value</param>
        /// <param name="rebar">rebar value</param>
        /// <param name="elevation">elevation value</param>
        public PlotData(string label, double steel, double concrete, double timber, double unknown, double rebar, double elevation)
        {
            Label = label;
            Steel = steel;
            Concrete = concrete;
            Timber = timber;
            Unknown = unknown;
            Rebar = rebar;
            Elevation = elevation;
        }

        public double Total {
            get
            {
                return Steel + Concrete + Rebar + Timber + Unknown;
            }
        }

        public string Label { get; set; }
        private double steel;
        public double Steel
        {
            get
            {
                return steel;
            }
            set
            {
                steel = Math.Round(value, 2);
            }
        }
        private double concrete;
        public double Concrete
        {
            get
            {
                return concrete;
            }
            set
            {
                concrete = Math.Round(value, 2);
            }
        }
        private double rebar;
        public double Rebar
        {
            get
            {
                return rebar;
            }
            set
            {
                rebar = Math.Round(value, 2);
            }
        }
        private double timber;
        public double Timber
        {
            get
            {
                return timber;
            }
            set
            {
                timber = Math.Round(value, 2);
            }
        }
        private double unknown;
        public double Unknown
        {
            get
            {
                return unknown;
            }
            set
            {
                unknown = Math.Round(value, 2);
            }
        }
        public double Elevation { get; set; }
    }

    /// <summary>
    /// Total data
    /// </summary>
    public class Totals : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Total data
        /// </summary>
        /// <param name="name">Name of data</param>
        /// <param name="value">Total value</param>
        public Totals(string name, double value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }
        private double totalValue;
        public double Value
        {
            get { return totalValue; }
            set
            {
                totalValue = value;
                OnPropertyChanged("Value");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    /// <summary>
    /// Enum to represent how to arrive at a embodied carbon value, by volume or weight.
    /// </summary>
    public enum MultiplierType
    {
        Volume,
        Weight
    }

    /// <summary>
    /// GWP name and value
    /// </summary>
    public class GwpNameValue
    {
        /// <summary>
        /// GWP name and value
        /// </summary>
        /// <param name="name">GWP name</param>
        /// <param name="value">GWP coefficient</param>
        /// <param name="type">GWP multiplier type, volume or weight</param>
        public GwpNameValue(string name, double value, MultiplierType type = MultiplierType.Volume)
        {
            Name = name;
            Value = value;
            GwpType = type;
        }
        public string Name { get; set; }
        public double Value { get; set; }
        public MultiplierType GwpType { get; set; }
    }

    /// <summary>
    /// GWP representation
    /// </summary>
    public class GwpData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// List of all RevitElements belonging to GwpData
        /// </summary>
        public List<RevitElement> RevitElements = new List<RevitElement>();

        /// <summary>
        /// Get total rebar weight of all RevitElements belonging to this GwpData
        /// </summary>
        /// <returns>Total Rebar Weight</returns>
        public double GetTotalRebarWeight()
        {
            double totalRebarWeight = 0.0;
            foreach (var revitElement in RevitElements)
            {
                if (revitElement.Category == RevitCategory.Floor)
                {
                    totalRebarWeight += VolumeFactor * RebarWeightMultiplier * revitElement.Area;
                }
                else
                {
                    totalRebarWeight += VolumeFactor * RebarWeightMultiplier * revitElement.Volume / 27;
                }
            }
            return totalRebarWeight;
        }

        /// <summary>
        /// GWP representation
        /// </summary>
        /// <param name="revitCategory">Revit Category</param>
        /// <param name="materialName">Revit material name</param>
        public GwpData(RevitCategory revitCategory, string materialName)
        {
            Category = revitCategory;
            MaterialName = materialName;
            volumeFactor = 1.00;
        }

        /// <summary>
        /// The RevitCategory of this GwpData
        /// </summary>
        public RevitCategory Category { get; set; }

        /// <summary>
        /// The material name of this GwpData
        /// </summary>
        public string MaterialName { get; set; }

        private double gwp;
        /// <summary>
        /// The assigned GWP coeffieient
        /// </summary>
        public double Gwp
        {
            get { return gwp; }
            set
            {
                gwp = value;
                OnPropertyChanged("Gwp");
            }
        }

        private double rebarGwp;
        /// <summary>
        /// Rebar GWP coefficient
        /// </summary>
        public double RebarGwp
        {
            get { return rebarGwp; }
            set
            {
                rebarGwp = value;
                OnPropertyChanged("RebarGwp");
            }
        }

        /// <summary>
        /// Selected GwpNameValue index
        /// </summary>
        public int GwpSelectedIndex { get; set; }

        /// <summary>
        /// Select GwpNameValue
        /// </summary>
        public GwpNameValue GwpSelected;

        private double rebarEstimateBasis;
        /// <summary>
        /// Concrete cubic yards or square feet for estimating rebar weight
        /// </summary>
        public double RebarEstimateBasis
        {
            get
            {
                return VolumeFactor * rebarEstimateBasis;
            }
            set
            {
                rebarEstimateBasis = value;
                RebarWeight = RebarEstimateBasis * RebarWeightMultiplier;
            }
        }

        /// <summary>
        /// Rebar estimate basis unit string
        /// </summary>
        public string RebarEstimateBasisString
        {
            get
            {
                string val = RebarEstimateBasis.ToString("N2");
                if (this.Category == RevitCategory.Floor) val = val + " SF";
                else val = val + " CY";
                return val;
            }
        }

        private double rebarWeightMultiplier;
        /// <summary>
        /// Rebar estimation multipier
        /// </summary>
        public double RebarWeightMultiplier
        {
            get { return rebarWeightMultiplier; }
            set
            {
                rebarWeightMultiplier = value;
                RebarWeight = RebarEstimateBasis * RebarWeightMultiplier;
                OnPropertyChanged("RebarWeight");
            }
        }

        /// <summary>
        /// Rebar weight multiplier unit string
        /// </summary>
        public string RebarWeightMultiplierUnit
        {
            get
            {
                string val;
                if (this.Category == RevitCategory.Floor) val="PSF";
                else val = "PCY";
                return val;
            }
        }

        private double rebarWeight;
        /// <summary>
        /// Estimated rebar weight
        /// </summary>
        public double RebarWeight
        {
            get
            {
                return rebarWeight;
            }
            set
            {
                rebarWeight = Math.Round(value, 2);
                OnPropertyChanged("RebarWeight");
            }
        }

        /// <summary>
        /// Used for distributing rebar EC to the appropriate levels
        /// </summary>
        public Dictionary<string, double> rebarLevelBreakdown = new Dictionary<string, double>();

        /// <summary>
        /// The ratio of rebar for levels
        /// </summary>
        public Dictionary<string, double> rebarLevelRatio
        {
            get
            {
                Dictionary<string, double> ratios = new Dictionary<string, double>();
                foreach (var level in rebarLevelBreakdown)
                {
                    ratios.Add(level.Key, level.Value / rebarEstimateBasis);
                }
                return ratios;
            }
        }

        private double volume;
        /// <summary>
        /// Volume for this GwpData
        /// </summary>
        public double Volume {
            get
            {
                return Math.Round(VolumeFactor * volume, 2);
            }
            set
            {
                volume = Math.Round(value, 2);
            }
        }

        private double volumeFactor;
        /// <summary>
        /// Factor for Volume
        /// </summary>
        public double VolumeFactor
        {
            get
            {
                return volumeFactor;
            }
            set
            {
                volumeFactor = value;
                OnPropertyChanged("VolumeString");
                OnPropertyChanged("RebarEstimateBasisString");
                RebarWeight = RebarEstimateBasis * RebarWeightMultiplier;
            }
        }

        /// <summary>
        /// Volume string with unit
        /// </summary>
        public string VolumeString
        {
            get
            {
                return Volume.ToString("N0") + " CF";
            }
        }

        private double density;
        /// <summary>
        /// The density for this GwpData
        /// </summary>
        public double Density {
            get
            {
                return density;
            }
            set
            {
                density = Math.Round(value, 2);
            }
        }

        /// <summary>
        /// The density string with unit
        /// </summary>
        public string DensityString
        {
            get
            {
                return Density.ToString("N0") + " PCF";
            }
        }

        /// <summary>
        /// Concrete GwpNameValue list
        /// </summary>
        public List<GwpNameValue> ConcreteGwpList;

        /// <summary>
        /// Initialize concrete GwpNameValue list
        /// </summary>
        public void PopulateConcreteGwpList()
        {
            ConcreteGwpList = new List<GwpNameValue>()
            {
                new GwpNameValue("2500-00-FA/SL", 231.1),
                new GwpNameValue("2500-20-FA", 199.7),
                new GwpNameValue("2500-30-FA", 182.7),
                new GwpNameValue("2500-40-FA", 164.9),
                new GwpNameValue("2500-30-SL", 178.1),
                new GwpNameValue("2500-40-SL", 160.5),
                new GwpNameValue("2500-50-SL", 142.9),
                new GwpNameValue("2500-50-FA/SL", 144.0),
                new GwpNameValue("3000-00-FA/SL", 257.7),
                new GwpNameValue("3000-20-FA", 222.1),
                new GwpNameValue("3000-30-FA", 202.9),
                new GwpNameValue("3000-40-FA", 182.6),
                new GwpNameValue("3000-30-SL", 197.7),
                new GwpNameValue("3000-40-SL", 177.7),
                new GwpNameValue("3000-50-SL", 157.8),
                new GwpNameValue("3000-50-FA/SL", 159.0),
                new GwpNameValue("4000-00-FA/SL", 318.1),
                new GwpNameValue("4000-20-FA", 273.0),
                new GwpNameValue("4000-30-FA", 248.6),
                new GwpNameValue("4000-40-FA", 223.0),
                new GwpNameValue("4000-30-SL", 242.1),
                new GwpNameValue("4000-40-SL", 216.8),
                new GwpNameValue("4000-50-SL", 191.4),
                new GwpNameValue("4000-50-FA/SL", 193.0),
                new GwpNameValue("5000-00-FA/SL", 389.2),
                new GwpNameValue("5000-20-FA", 333.0),
                new GwpNameValue("5000-30-FA", 302.7),
                new GwpNameValue("5000-40-FA", 270.6),
                new GwpNameValue("5000-30-SL", 294.4),
                new GwpNameValue("5000-40-SL", 262.8),
                new GwpNameValue("5000-50-SL", 231.2),
                new GwpNameValue("5000-50-FA/SL", 233.2),
                new GwpNameValue("6000-00-FA/SL", 409.9),
                new GwpNameValue("6000-20-FA", 350.5),
                new GwpNameValue("6000-30-FA", 318.4),
                new GwpNameValue("6000-40-FA", 284.5),
                new GwpNameValue("6000-30-SL", 309.7),
                new GwpNameValue("6000-40-SL", 276.4),
                new GwpNameValue("6000-50-SL", 243.0),
                new GwpNameValue("6000-50-FA/SL", 245.1),
                new GwpNameValue("8000-00-FA/SL", 477.2),
                new GwpNameValue("8000-20-FA", 407.2),
                new GwpNameValue("8000-30-FA", 369.4),
                new GwpNameValue("8000-40-FA", 329.5),
                new GwpNameValue("8000-30-SL", 359.2),
                new GwpNameValue("8000-40-SL", 319.8),
                new GwpNameValue("8000-50-SL", 280.5),
                new GwpNameValue("8000-50-FA/SL", 283.2)
            };
            GwpSelectedIndex = 0;
            GwpSelected = ConcreteGwpList[0];
            gwp = ConcreteGwpList[0].Value;
            RebarGwp = 714.2;
        }

        [JsonIgnore]
        public List<string> ConcreteGwpTypeNameList
        {
            get { return ConcreteGwpList.Select(x => x.Name).ToList(); }
        }

        /// <summary>
        /// Steel GwpNameValue list
        /// </summary>
        public List<GwpNameValue> SteelGwpList;

        /// <summary>
        /// Initialize steel GwpNameValue list
        /// </summary>
        public void PopulateSteelGwpList()
        {
            SteelGwpList = new List<GwpNameValue>()
            {
                new GwpNameValue("Primary Steel", 1350.8, MultiplierType.Weight),
                new GwpNameValue("HSS Steel", 2168.2, MultiplierType.Weight)
            };
            GwpSelectedIndex = 0;
            GwpSelected = SteelGwpList[0];
            gwp = SteelGwpList[0].Value;
        }

        [JsonIgnore]
        public List<string> SteelGwpTypeNameList
        {
            get { return SteelGwpList.Select(x => x.Name).ToList(); }
        }

        /// <summary>
        /// Timber GwpNameValue list
        /// </summary>
        public List<GwpNameValue> TimberGwpList;

        /// <summary>
        /// Initialize Timber GwpNameValue list
        /// </summary>
        public void PopulateTimberGwpList()
        {
            TimberGwpList = new List<GwpNameValue>()
            {
                new GwpNameValue("Softwood Lumber", 95.0),
                new GwpNameValue("Softwood Plywood", 169.7),
                new GwpNameValue("Oriented Strand Board", 324.8),
                new GwpNameValue("Glulam", 258.9),
                new GwpNameValue("Laminated Veneer Lumber", 263.9)
            };
            GwpSelectedIndex = 0;
            GwpSelected = TimberGwpList[0];
            gwp = TimberGwpList[0].Value;
        }

        [JsonIgnore]
        public List<string> TimberGwpTypeNameList
        {
            get { return TimberGwpList.Select(x => x.Name).ToList(); }
        }

        /// <summary>
        /// All GwpNameValue list
        /// </summary>
        public List<GwpNameValue> AllGwpList;

        /// <summary>
        /// Initialize All GwpNameValue list
        /// </summary>
        public void PopulateAllGwpList()
        {
            PopulateConcreteGwpList();
            PopulateSteelGwpList();
            PopulateTimberGwpList();
            AllGwpList = new List<GwpNameValue>()
            {
                new GwpNameValue("Unknown", 1.0)
            };
            AllGwpList.AddRange(SteelGwpList);
            AllGwpList.AddRange(TimberGwpList);
            AllGwpList.AddRange(ConcreteGwpList);
            GwpSelectedIndex = 0;
            GwpSelected = AllGwpList[0];
            gwp = AllGwpList[0].Value;
        }

        [JsonIgnore]
        public List<string> AllGwpTypeNameList
        {
            get { return AllGwpList.Select(x => x.Name).ToList(); }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
