using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon
{
    /// <summary>
    /// A custom representation of a Revit Element.
    /// </summary>
    public class RevitElement
    {
        /// <summary>
        /// A custom representation of a Revit Element.
        /// </summary>
        public RevitElement()
        { }

        public RevitElement(RevitCategory category, string id, string name, string associatedLevel, double associatedLevelElevation, double volume, string materialName, MaterialType material, double density)
        {
            this.Category = category;
            this.Id = id;
            this.Name = name;
            this.AssociatedLevel = associatedLevel;
            this.AssociatedElevation = associatedLevelElevation;
            this.Volume = volume;
            this.MaterialName = materialName;
            this.Material = material;
            this.Density = density;
        }

        /// <summary>
        /// Revit Id of the element
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the element
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Level that the element is associated with
        /// </summary>
        public string AssociatedLevel { get; set; }

        /// <summary>
        /// Elevation os associated level
        /// </summary>
        public double AssociatedElevation { get; set; }

        /// <summary>
        /// Volume in ft^3.
        /// </summary>
        public double Volume { get; set; }

        private double density;
        /// <summary>
        /// Density in lbs per cubic foot
        /// </summary>
        public double Density {
            get { return density; }
            set
            {
                density = value;
                if (density == 0.0)
                {
                    Material = MaterialType.Unknown;
                }
            }
        }

        /// <summary>
        /// Weight in lbs
        /// </summary>
        public double Weight {
            get
            {
                return Volume * Density;
            }
        }

        /// <summary>
        /// Factor for volume
        /// </summary>
        public double VolumeFactor { get; set; }

        /// <summary>
        /// Factored Volume
        /// </summary>
        public double FactoredVolume { get; set; }

        /// <summary>
        /// Factored Weight
        /// </summary>
        public double FactoredWeight { get; set; }

        /// <summary>
        /// Area in ft^2
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// Material Type
        /// </summary>
        public MaterialType Material { get; set; }

        private string materialName;
        /// <summary>
        /// Primary structural material of the element
        /// </summary>
        public string MaterialName
        {
            get { return materialName; }
            set
            {
                materialName = value;
            }
        }

        /// <summary>
        /// Category of the element
        /// </summary>
        public RevitCategory Category { get; set; }

        /// <summary>
        /// GWP Type string
        /// </summary>
        public string GwpType { get; set; }

        /// <summary>
        /// Material GWP assigned
        /// </summary>
        public double Gwp { get; set; }

        /// <summary>
        /// GWP unit string
        /// </summary>
        public string GwpUnit
        {
            get
            {
                string val;
                if (this.Material == MaterialType.Steel || this.Material == MaterialType.Rebar) val = "KG CO2e PER SHORT TON";
                else val = "KG CO2e PER CUBIC YARD";
                return val;
            }
        }

        /// <summary>
        /// Element embodied carbon
        /// </summary>
        public double EmbodiedCarbon { get; set; }

        /// <summary>
        /// Multiplier for converting concrete quantity to rebar weight
        /// </summary>
        public double RebarMultiplier { get; set; }

        /// <summary>
        /// Rebar multiplier unit string
        /// </summary>
        public string RebarMultiplierUnit
        {
            get
            {
                string val;
                if (this.Category == RevitCategory.Floor) val = "PSF";
                else val = "PCY";
                return val;
            }
        }

        /// <summary>
        /// The weight of rebar
        /// </summary>
        public double RebarWeight { get; set; }

        /// <summary>
        /// Rebar GWP
        /// </summary>
        public double RebarGwp { get; set; }

        /// <summary>
        /// Rebar embodied carbon
        /// </summary>
        public double RebarEmbodiedCarbon { get; set; }
    }

    /// <summary>
    /// A cutom representation of a Revit Phase.
    /// </summary>
    public class RevitPhase
    {
        public RevitPhase(string name, bool export)
        {
            Name = name;
            Export = export;
        }
        public string Name { get; set; }
        public bool Export { get; set; }
    }

    /// <summary>
    /// A custom enum of supported Revit categories
    /// </summary>
    public enum RevitCategory
    {
        Framing,
        Column,
        Floor,
        Wall,
        Foundation
    }

    /// <summary>
    /// A custom enum of supported Revit Materials
    /// </summary>
    public enum MaterialType
    {
        Unknown,
        Steel,
        Concrete,
        Rebar,
        Timber
    }
}
