using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

namespace Beacon
{
    /// <summary>
    /// Class responsible for reading and interpreting data from a Revit model.
    /// </summary>
    public class RevitReader
    {
        private List<RevitPhase> a_revitPhases = new List<RevitPhase>();
        private List<Tuple<double, string>> a_levelData = new List<Tuple<double, string>>();
        private List<RevitElement> a_revitElementData = new List<RevitElement>();
        private Document a_doc;
        private string a_unknownLevelName = "Unknown Level";
        /// <summary>
        /// Set below the lowest level elevation
        /// </summary>
        private double a_unknownLevelElevation = 0.0;

        public Dictionary<string, List<string>> a_LevelMap = new Dictionary<string, List<string>>();

        /// <summary>
        /// Class responsible for reading and interpreting data from a Revit model.
        /// </summary>
        /// <param name="doc">Provide Revit DB Document</param>
        public RevitReader(Document doc)
        {
            a_doc = doc;
            this.ReadPhases();
            this.ReadLevels();
        }

        /// <summary>
        /// Read all supported elements from a Revit model.
        /// </summary>
        public void ReadElements()
        {
            this.ReadFramings();
            this.ReadColumns();
            this.ReadFloors();
            this.ReadWalls();
            this.ReadFoundations();
        }

        /// <summary>
        /// Returns a list of RevitElements.
        /// </summary>
        public List<RevitElement> RevitElementData
        {
            get { return a_revitElementData; }
        }

        /// <summary>
        /// Read all Revit model phases.
        /// </summary>
        private void ReadPhases()
        {
            var collector = new FilteredElementCollector(a_doc);
            var filterCategory = new ElementCategoryFilter(BuiltInCategory.OST_Phases);
            var filterNotSymbol = new ElementClassFilter(typeof(FamilySymbol), true);
            var filter = new LogicalAndFilter(filterCategory, filterNotSymbol);
            var elements = collector.WherePasses(filter).ToElements();
            foreach (var element in elements)
            {
                try
                {
                    if (element is Phase)
                    {
                        bool export = false;
                        if (element.Name.ToUpper().Contains("NEW")) export = true;
                        this.a_revitPhases.Add(new RevitPhase(element.Name, export));
                    }
                }
                catch (Exception e)
                {
                    ShowElementErrorMessage(element, e);
                }
            }
        }

        /// <summary>
        /// Returns a list of RevitPhases.
        /// </summary>
        public List<RevitPhase> RevitPhaseData
        {
            get { return a_revitPhases; }
        }


        /// <summary>
        /// Read all Revit model levels.
        /// </summary>
        private void ReadLevels()
        {
            var collector = new FilteredElementCollector(a_doc);
            var filterCategory = new ElementCategoryFilter(BuiltInCategory.OST_Levels);
            var filterNotSymbol = new ElementClassFilter(typeof(FamilySymbol), true);
            var filter = new LogicalAndFilter(filterCategory, filterNotSymbol);
            var elements = collector.WherePasses(filter).ToElements();
            var unSortedLevels = new List<Tuple<double, string>>();
            foreach (var element in elements)
            {
                try
                {
                    if (element is Level)
                    {
                        var level = element as Level;
                        var levelWork = new Tuple<double, string>(level.ProjectElevation, level.Name);
                        unSortedLevels.Add(levelWork);
                    }
                }
                catch (Exception e)
                {
                    ShowElementErrorMessage(element, e);
                }
            }
            // Sort level data by elevation, bottom to top
            a_levelData = unSortedLevels.OrderBy(el => el.Item1).ToList();
            a_unknownLevelElevation = a_levelData.Count > 0 ? a_levelData[0].Item1 - 100 : 0.0;
        }

        /// <summary>
        /// Returns a list of Revit level names.
        /// </summary>
        public List<string> RevitLevelNames
        {
            get { return a_levelData.Select(x => x.Item2).ToList(); }
        }

        /// <summary>
        /// Read all BuiltInCategory.OST_StructuralFraming elements from a Revit model.
        /// </summary>
        private void ReadFramings()
        {
            var collector = new FilteredElementCollector(a_doc);
            var filterCategory = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
            var filterNotSymbol = new ElementClassFilter(typeof(FamilySymbol), true);
            var filter = new LogicalAndFilter(filterCategory, filterNotSymbol);
            var elements = collector.WherePasses(filter).ToElements();
            foreach (var element in elements)
            {
                try
                {
                    if (element is FamilyInstance && CheckElementPhase(element))
                    {
                        var revitElement = new RevitElement();
                        revitElement.Category = RevitCategory.Framing;
                        revitElement.Id = element.Id.ToString();
                        revitElement.Name = element.Name;

                        var elementLevel = element.GetParameters("Reference Level");
                        Level levelElement = elementLevel.Count() > 0 ? a_doc.GetElement(elementLevel[0].AsElementId()) as Level : null;
                        revitElement.AssociatedLevel = levelElement != null ? levelElement.Name : a_unknownLevelName;
                        revitElement.AssociatedElevation = levelElement != null ? levelElement.ProjectElevation : a_unknownLevelElevation;
                        revitElement.AssociatedLevel = GetMappedLevel(revitElement.AssociatedLevel);

                        var elementVolume = element.GetParameters("Volume");
                        revitElement.Volume = elementVolume.Count() > 0 ? elementVolume[0].AsDouble() : 0.0;
                        var framingStructuralMaterial = this.GetStructuralMaterialFromElement(element, a_doc);
                        revitElement.MaterialName = framingStructuralMaterial != null ? framingStructuralMaterial.Name : "";
                        revitElement.Material = framingStructuralMaterial != null ? GetMaterialType(framingStructuralMaterial) : MaterialType.Unknown;
                        double density = framingStructuralMaterial != null ? GetDensity(framingStructuralMaterial) : 0.0;
                        revitElement.Density = density;

                        a_revitElementData.Add(revitElement);
                    }
                }
                catch (Exception e)
                {
                    ShowElementErrorMessage(element, e);
                }
            }

        }

        /// <summary>
        /// Read all BuiltInCategory.OST_StructuralColumns elements from a Revit model.
        /// </summary>
        private void ReadColumns()
        {
            var collector = new FilteredElementCollector(a_doc);
            var filterCategory = new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns);
            var filterNotSymbol = new ElementClassFilter(typeof(FamilySymbol), true);
            var filter = new LogicalAndFilter(filterCategory, filterNotSymbol);
            var elements = collector.WherePasses(filter).ToElements();
            foreach (var element in elements)
            {
                try
                {
                    if (element is FamilyInstance && CheckElementPhase(element))
                    {
                        var revitElement = new RevitElement();
                        revitElement.Category = RevitCategory.Column;
                        revitElement.Id = element.Id.ToString();
                        revitElement.Name = element.Name;

                        var baseLevelParam = element.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
                        Level baseLevel = baseLevelParam != null ? a_doc.GetElement(baseLevelParam.AsElementId()) as Level : null;
                        revitElement.AssociatedLevel = baseLevel != null ? baseLevel.Name : a_unknownLevelName;
                        revitElement.AssociatedElevation = baseLevel != null ? baseLevel.ProjectElevation : a_unknownLevelElevation;
                        revitElement.AssociatedLevel = GetMappedLevel(revitElement.AssociatedLevel);

                        var elementVolume = element.GetParameters("Volume");
                        revitElement.Volume = elementVolume.Count() > 0 ? elementVolume[0].AsDouble() : 0.0;
                        var framingStructuralMaterial = this.GetStructuralMaterialFromElement(element, a_doc);
                        revitElement.MaterialName = framingStructuralMaterial != null ? framingStructuralMaterial.Name : "";
                        revitElement.Material = framingStructuralMaterial != null ? GetMaterialType(framingStructuralMaterial) : MaterialType.Unknown;
                        double density = framingStructuralMaterial != null ? GetDensity(framingStructuralMaterial) : 0.0;
                        revitElement.Density = density;

                        // Split Column by Levels
                        var colCurve = CreateColumnCurve(element as FamilyInstance);
                        if (colCurve != null && a_levelData.Count > 0)
                        {
                            var basePt = colCurve.GetEndPoint(0);
                            var topPt = colCurve.GetEndPoint(1);
                            var trueBel = basePt.Z;
                            var trueTel = topPt.Z;
                            var colHeight = trueTel - trueBel;
                            SplitByLevels(revitElement, trueBel, trueTel, colHeight);
                        }
                        else
                        {
                            a_revitElementData.Add(revitElement);
                        }
                    }
                }
                catch (Exception e)
                {
                    ShowElementErrorMessage(element, e);
                }
            }

        }

        /// <summary>
        /// Read all BuiltInCategory.OST_Floors elements from a Revit model.
        /// </summary>
        private void ReadFloors()
        {
            var collector = new FilteredElementCollector(a_doc);
            List<ElementFilter> filters = new List<ElementFilter>();
            var filterCategory = new ElementCategoryFilter(BuiltInCategory.OST_Floors);
            var filterNotSymbol = new ElementClassFilter(typeof(FamilySymbol), true);
            var filterNotFloorType = new ElementClassFilter(typeof(FloorType), true);
            filters.Add(filterCategory);
            filters.Add(filterNotSymbol);
            filters.Add(filterNotFloorType);
            var filter = new LogicalAndFilter(filters);
            var elements = collector.WherePasses(filter).ToElements();
            foreach (var element in elements)
            {
                try
                {
                    if (element is Floor && CheckElementPhase(element))
                    {
                        Floor floor = element as Floor;
                        var revitElement = new RevitElement();
                        revitElement.Category = RevitCategory.Floor;
                        revitElement.Id = element.Id.ToString();
                        revitElement.Name = element.Name;

                        var floorLevelid = floor.LevelId;
                        Level levelElement = a_doc.GetElement(floorLevelid) as Level;
                        revitElement.AssociatedLevel = levelElement != null ? levelElement.Name : a_unknownLevelName;
                        revitElement.AssociatedElevation = levelElement != null ? levelElement.ProjectElevation : a_unknownLevelElevation;
                        revitElement.AssociatedLevel = GetMappedLevel(revitElement.AssociatedLevel);

                        var floorArea = element.GetParameters("Area");
                        revitElement.Area = floorArea.Count() > 0 ? floorArea[0].AsDouble() : 0.0;

                        var elementVolume = element.GetParameters("Volume");
                        revitElement.Volume = elementVolume.Count() > 0 ? elementVolume[0].AsDouble() : 0.0;

                        // Look for metal deck
                        var floorTypeId = element.GetTypeId();
                        var floorType = a_doc.GetElement(floorTypeId) as HostObjAttributes;
                        if (floorType != null)
                        {
                            var elementCompoundStructure = floorType.GetCompoundStructure();
                            var compoundStructureLayers = elementCompoundStructure.GetLayers();
                            foreach (var elementLayer in compoundStructureLayers)
                            {
                                if (elementLayer.Function == MaterialFunctionAssignment.StructuralDeck)
                                {
                                    var elementLayerDeck = a_doc.GetElement(elementLayer.DeckProfileId) as FamilySymbol;
                                    var elementLayerMaterial = a_doc.GetElement(elementLayer.MaterialId) as Material;
                                    if (elementLayerDeck != null && elementLayerMaterial != null)
                                    {
                                        // hr = height of rib
                                        var hrParam = elementLayerDeck.GetParameters("hr");
                                        double hr = hrParam.Count() > 0 ? hrParam[0].AsDouble() : 0.0;
                                        // wr = width of rib
                                        var wrParam = elementLayerDeck.GetParameters("wr");
                                        double wr = wrParam.Count() > 0 ? wrParam[0].AsDouble() : 0.0;
                                        // rr = root of rib
                                        var rrParam = elementLayerDeck.GetParameters("rr");
                                        double rr = rrParam.Count() > 0 ? rrParam[0].AsDouble() : 0.0;
                                        // Sr = width of rib
                                        var srParam = elementLayerDeck.GetParameters("Sr");
                                        double Sr = srParam.Count() > 0 ? srParam[0].AsDouble() : 0.0;
                                        // Thickness
                                        var thicknessParam = elementLayerDeck.GetParameters("Thickness");
                                        double thickness = thicknessParam.Count() > 0 ? thicknessParam[0].AsDouble() : 0.0;
                                        // Layer Density
                                        double layerDensity = GetDensity(elementLayerMaterial);

                                        if (revitElement.Area > 0 &&
                                            Sr > 0 && hr > 0 && wr > 0 && rr > 0 && thickness > 0 && layerDensity > 0)
                                        {
                                            revitElement.Volume = revitElement.Volume - (revitElement.Area * hr / 2);

                                            double flange = Sr - wr;
                                            double rib = rr;
                                            double web = Math.Sqrt(Math.Pow(hr, 2) + Math.Pow(((wr - rr) / 2), 2));
                                            double deckWidthFlatten = flange + rib + (2 * web);

                                            double weightOfOneFootOfDeck = deckWidthFlatten * thickness * 1 * layerDensity;
                                            double deckPsf = weightOfOneFootOfDeck / (Sr * 1);
                                            double weightOfDeck = revitElement.Area * deckPsf;
                                            double volumeOfDeck = weightOfDeck / layerDensity;

                                            string deckName = elementLayerDeck.FamilyName + " - " + elementLayerDeck.Name;
                                            RevitElement floorDeck = new RevitElement(
                                                revitElement.Category, revitElement.Id, deckName, revitElement.AssociatedLevel, revitElement.AssociatedElevation, volumeOfDeck,
                                                elementLayerMaterial.Name, GetMaterialType(elementLayerMaterial), layerDensity);
                                            floorDeck.Area = revitElement.Area;
                                            a_revitElementData.Add(floorDeck);
                                        }
                                    }
                                }
                            }
                        }

                        var framingStructuralMaterial = this.GetStructuralMaterialFromElement(element, a_doc);
                        revitElement.MaterialName = framingStructuralMaterial != null ? framingStructuralMaterial.Name : "";
                        revitElement.Material = framingStructuralMaterial != null ? GetMaterialType(framingStructuralMaterial) : MaterialType.Unknown;
                        double density = framingStructuralMaterial != null ? GetDensity(framingStructuralMaterial) : 0.0;
                        revitElement.Density = density;

                        a_revitElementData.Add(revitElement);
                    }
                }
                catch (Exception e)
                {
                    ShowElementErrorMessage(element, e);
                }
            }

        }

        /// <summary>
        /// Read all BuiltInCategory.OST_Walls elements from a Revit model.
        /// </summary>
        private void ReadWalls()
        {
            var collector = new FilteredElementCollector(a_doc);
            List<ElementFilter> filters = new List<ElementFilter>();
            var filterCategory = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            var filterNotSymbol = new ElementClassFilter(typeof(FamilySymbol), true);
            var filterNotWallType = new ElementClassFilter(typeof(WallType), true);
            filters.Add(filterCategory);
            filters.Add(filterNotSymbol);
            filters.Add(filterNotWallType);
            var filter = new LogicalAndFilter(filters);
            var elements = collector.WherePasses(filter).ToElements();
            foreach (var element in elements)
            {
                try
                {
                    if (element is Wall && CheckElementPhase(element))
                    {
                        var revitElement = new RevitElement();
                        revitElement.Category = RevitCategory.Wall;
                        revitElement.Id = element.Id.ToString();
                        revitElement.Name = element.Name;

                        var wallBaseConstraintParam = element.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT);
                        Level wallBaseConstraintLevel = wallBaseConstraintParam != null ? a_doc.GetElement(wallBaseConstraintParam.AsElementId()) as Level : null;
                        revitElement.AssociatedLevel = wallBaseConstraintLevel != null ? wallBaseConstraintLevel.Name : a_unknownLevelName;
                        revitElement.AssociatedElevation = wallBaseConstraintLevel != null ? wallBaseConstraintLevel.ProjectElevation : a_unknownLevelElevation;
                        revitElement.AssociatedLevel = GetMappedLevel(revitElement.AssociatedLevel);

                        var elementVolume = element.GetParameters("Volume");
                        revitElement.Volume = elementVolume.Count() > 0 ? elementVolume[0].AsDouble() : 0.0;
                        var framingStructuralMaterial = this.GetStructuralMaterialFromElement(element, a_doc);
                        revitElement.MaterialName = framingStructuralMaterial != null ? framingStructuralMaterial.Name : "";
                        revitElement.Material = framingStructuralMaterial != null ? GetMaterialType(framingStructuralMaterial) : MaterialType.Unknown;
                        double density = framingStructuralMaterial != null ? GetDensity(framingStructuralMaterial) : 0.0;
                        revitElement.Density = density;

                        // Split Wall by Levels
                        var unconnectedHeight = element.GetParameters("Unconnected Height"); // accounts for walls that have no top constraint assigned
                        double wallHeight = unconnectedHeight.Count > 0 ? unconnectedHeight[0].AsDouble() : 0;
                        if (wallBaseConstraintLevel != null && wallHeight > 0 && a_levelData.Count > 0)
                        {
                            var wallBottomElevation = wallBaseConstraintLevel.ProjectElevation;
                            var wallBottomOffset = element.GetParameters("Base Offset");
                            var baseOffset = wallBottomOffset.Count > 0 ? wallBottomOffset[0].AsDouble() : 0;
                            double wtrueBel = wallBottomElevation + baseOffset; // true bottom elevation of wall with model offset applied
                            double wtrueTel = wtrueBel + wallHeight; // true top elevation of wall
                            SplitByLevels(revitElement, wtrueBel, wtrueTel, wallHeight);
                        }
                        else
                        {
                            a_revitElementData.Add(revitElement);
                        }
                    }
                }
                catch (Exception e)
                {
                    ShowElementErrorMessage(element, e);
                }
            }

        }

        /// <summary>
        /// Read all BuiltInCategory.OST_StructuralFoundation elements from a Revit model.
        /// </summary>
        private void ReadFoundations()
        {
            var collector = new FilteredElementCollector(a_doc);
            List<ElementFilter> filters = new List<ElementFilter>();
            var filterCategory = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation);
            var filterNotSymbol = new ElementClassFilter(typeof(FamilySymbol), true);
            var filterNotFloorType = new ElementClassFilter(typeof(FloorType), true);
            var filterNotWallFoundationType = new ElementClassFilter(typeof(WallFoundationType), true);
            filters.Add(filterCategory);
            filters.Add(filterNotSymbol);
            filters.Add(filterNotFloorType);
            filters.Add(filterNotWallFoundationType);
            var filter = new LogicalAndFilter(filters);
            var elements = collector.WherePasses(filter).ToElements();
            foreach (var element in elements)
            {
                try
                {
                    if ((element is FamilyInstance || element is WallFoundation || element is Floor) && CheckElementPhase(element))
                    {
                        var revitElement = new RevitElement();
                        revitElement.Category = RevitCategory.Foundation;
                        revitElement.Id = element.Id.ToString();
                        revitElement.Name = element.Name;

                        IList<Parameter> level = null;
                        if (element is WallFoundation)
                        {
                            var wallFoundation = element as WallFoundation;
                            var wallElement = a_doc.GetElement(wallFoundation.WallId);
                            var temp = wallElement.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT);
                            IList<Parameter> first = new List<Parameter>();
                            if (temp != null) first.Add(temp);
                            level = first;
                        }
                        else
                        {
                            level = element.GetParameters("Level");
                        }
                        Level levelElement = level.Count() > 0 ? a_doc.GetElement(level[0].AsElementId()) as Level : null;
                        revitElement.AssociatedLevel = levelElement != null ? levelElement.Name : a_unknownLevelName;
                        revitElement.AssociatedElevation = levelElement != null ? levelElement.ProjectElevation : a_unknownLevelElevation;
                        revitElement.AssociatedLevel = GetMappedLevel(revitElement.AssociatedLevel);

                        var elementVolume = element.GetParameters("Volume");
                        revitElement.Volume = elementVolume.Count() > 0 ? elementVolume[0].AsDouble() : 0.0;
                        var framingStructuralMaterial = this.GetStructuralMaterialFromElement(element, a_doc);
                        revitElement.MaterialName = framingStructuralMaterial != null ? framingStructuralMaterial.Name : "";
                        revitElement.Material = framingStructuralMaterial != null ? GetMaterialType(framingStructuralMaterial) : MaterialType.Unknown;
                        double density = framingStructuralMaterial != null ? GetDensity(framingStructuralMaterial) : 0.0;
                        revitElement.Density = density;

                        a_revitElementData.Add(revitElement);
                    }
                }
                catch (Exception e)
                {
                    ShowElementErrorMessage(element, e);
                }
            }

        }

        /// <summary>
        /// Splits RevitElement by level elevations
        /// </summary>
        /// <param name="revitElement">The RevitElement to split</param>
        /// <param name="bottomElev">The bottom elevation of the RevitElement</param>
        /// <param name="topElev">The top elevation of the RevitElement</param>
        /// <param name="height">The height of the RevitElement</param>
        private void SplitByLevels(RevitElement revitElement, double bottomElev, double topElev, double height)
        {
            try
            {
                // if the bottom of the column is below the lowest level, assign it to that level
                if (a_levelData.Count > 0 && a_levelData[0].Item1 > bottomElev && a_levelData[0].Item1 <= topElev)
                {
                    string associatedLevel = GetMappedLevel(a_levelData[0].Item2);
                    double length = a_levelData[0].Item1 - bottomElev;
                    double volume = (length / height) * revitElement.Volume;
                    a_revitElementData.Add(new RevitElement(revitElement.Category, revitElement.Id, revitElement.Name, associatedLevel, a_levelData[0].Item1, volume, revitElement.MaterialName, revitElement.Material, revitElement.Density));
                }
                var x = 1;
                while (x < this.a_levelData.Count)
                {
                    // column base is below level, above or at level below, and column top is above or at level
                    if (bottomElev < a_levelData[x].Item1 && bottomElev >= a_levelData[x - 1].Item1 && topElev >= a_levelData[x].Item1)
                    {
                        string associatedLevel = GetMappedLevel(a_levelData[x].Item2);
                        double length = a_levelData[x].Item1 - bottomElev;
                        double volume = (length / height) * revitElement.Volume;
                        a_revitElementData.Add(new RevitElement(revitElement.Category, revitElement.Id, revitElement.Name, associatedLevel, a_levelData[x].Item1, volume, revitElement.MaterialName, revitElement.Material, revitElement.Density));
                    }
                    else // column base is below or at level below and column top is above or at level
                    {
                        if (bottomElev <= a_levelData[x - 1].Item1 && topElev >= a_levelData[x].Item1)
                        {
                            string associatedLevel = GetMappedLevel(a_levelData[x].Item2);
                            double length = a_levelData[x].Item1 - a_levelData[x - 1].Item1;
                            double volume = (length / height) * revitElement.Volume;
                            a_revitElementData.Add(new RevitElement(revitElement.Category, revitElement.Id, revitElement.Name, associatedLevel, a_levelData[x].Item1, volume, revitElement.MaterialName, revitElement.Material, revitElement.Density));
                        }
                        else // column base is below or at level below, column top is above level below, column top is below or at level
                        {
                            if (bottomElev <= a_levelData[x - 1].Item1 && topElev > a_levelData[x - 1].Item1 && topElev <= a_levelData[x].Item1)
                            {
                                string associatedLevel = GetMappedLevel(a_levelData[x].Item2);
                                double length = topElev - a_levelData[x - 1].Item1;
                                double volume = (length / height) * revitElement.Volume;
                                a_revitElementData.Add(new RevitElement(revitElement.Category, revitElement.Id, revitElement.Name, associatedLevel, a_levelData[x].Item1, volume, revitElement.MaterialName, revitElement.Material, revitElement.Density));
                            }
                        }
                    }
                    x++;
                }
                //if the top of the column is above the highest level, assign it to that level
                if (a_levelData.Count > 0 && a_levelData[a_levelData.Count - 1].Item1 < topElev)
                {
                    string associatedLevel = GetMappedLevel(a_levelData[a_levelData.Count - 1].Item2);
                    double length = topElev - a_levelData[a_levelData.Count - 1].Item1;
                    double volume = (length / height) * revitElement.Volume;
                    a_revitElementData.Add(new RevitElement(revitElement.Category, revitElement.Id, revitElement.Name, associatedLevel, a_levelData[a_levelData.Count - 1].Item1, volume, revitElement.MaterialName, revitElement.Material, revitElement.Density));
                }
            }
            catch (Exception e)
            {
                string m = e.Message;
            }
        }

        /// <summary>
        /// Check to see if element is in a selected phase.
        /// </summary>
        /// <param name="element">The element to check</param>
        /// <returns>True if found, False otherwise</returns>
        private bool CheckElementPhase(Element element)
        {
            var elementPhaseCreatedId = element.CreatedPhaseId;
            var phaseCreated = a_doc.GetElement(elementPhaseCreatedId);
            if (this.a_revitPhases.Where(x => x.Name == phaseCreated.Name && x.Export == true).Count() > 0) { return true; }
            else { return false; }
        }

        /// <summary>
        /// Get mapped level name.
        /// </summary>
        /// <param name="revitLevel">The level name to map.</param>
        /// <returns>The mapped level name or the original level name if no map is found</returns>
        private string GetMappedLevel(string revitLevel)
        {
            string retLevel = revitLevel;
            foreach (var pair in a_LevelMap)
            {
                if (pair.Value.Contains(revitLevel))
                {
                    retLevel = pair.Key;
                    break;
                }
            }
            return retLevel;
        }

        /// <summary>
        /// Get the Revit Material for an element.
        /// </summary>
        /// <param name="element">The Revit element</param>
        /// <param name="doc">The Revit DB Doc</param>
        /// <returns>The found Revit Material, null if not found</returns>
        private Material GetStructuralMaterialFromElement(Element element, Document doc)
        {
            if (element is WallFoundation)
            {
                var elementTypeId = element.GetTypeId();
                var elementType = doc.GetElement(elementTypeId);
                var typeMatParam = elementType.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                if (typeMatParam != null)
                {
                    var typeMatId = typeMatParam.AsElementId();
                    return doc.GetElement(typeMatId) as Material;
                }
                else
                {
                    return null;
                }
            }

            var elementMatParam = element.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
            if (elementMatParam != null && elementMatParam.HasValue)
            {
                var elementMatId = elementMatParam.AsElementId();
                return doc.GetElement(elementMatId) as Material;
            }
            else
            {
                var elementTypeId = element.GetTypeId();
                var elementType = doc.GetElement(elementTypeId);
                var typeMatParam = elementType.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                if (typeMatParam != null)
                {
                    var typeMatId = typeMatParam.AsElementId();
                    return doc.GetElement(typeMatId) as Material;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get MaterialType for Revit Material
        /// </summary>
        /// <param name="material">The Revit Material</param>
        /// <returns>The found MaterialType</returns>
        private MaterialType GetMaterialType(Material material)
        {
            MaterialType materialType = MaterialType.Unknown;

            List<string> steelMaterialClasses = new List<string>()
            {
                "METAL"
            };
            List<string> timberMaterialClasses = new List<string>()
            {
                "WOOD"
            };
            List<string> concreteMaterialClasses = new List<string>()
            {
                "CONCRETE","MASONRY"
            };

            if (steelMaterialClasses.Contains(material.MaterialClass.ToUpper()))
                materialType = MaterialType.Steel;
            else if (timberMaterialClasses.Contains(material.MaterialClass.ToUpper()))
                materialType = MaterialType.Timber;
            else if (concreteMaterialClasses.Contains(material.MaterialClass.ToUpper()))
                materialType = MaterialType.Concrete;

            return materialType;
        }

        /// <summary>
        /// Get density from a Revit Material
        /// </summary>
        /// <param name="material">The Revit Material</param>
        /// <returns>The density in lb/ft^3, 0.0 if not found</returns>
        private double GetDensity(Material material)
        {
            double density = 0.0;
            var structuralAsset = a_doc.GetElement(material.StructuralAssetId);
            if (structuralAsset != null)
            {
                var densityParam = structuralAsset.GetParameters("Density");
                double FromMetricToImperialUnitWeight = 2.20462; //coefficient of converting unit weight from metric unit (kg/ft^3) to imperial unit (lb/ft^3)
                density = densityParam.Count() > 0 ? densityParam[0].AsDouble() * FromMetricToImperialUnitWeight : 0.0;
            }
            return density;
        }

        /// <summary>
        /// Create a Revit Curve from FamilyInstance
        /// </summary>
        /// <param name="inst">The Family Instance</param>
        /// <returns>The created Revit Curve</returns>
        public Curve CreateColumnCurve(FamilyInstance inst)
        {
            if (inst.Location != null && inst.Location is LocationCurve)
            {
                var instCrv = (inst.Location as LocationCurve).Curve;
                return (instCrv.GetEndPoint(0).Z < instCrv.GetEndPoint(1).Z) ?
                    instCrv : instCrv.CreateReversed();
            }
            else
            {
                return CreateVerticalColumnCurve(inst);
            }
        }

        /// <summary>
        /// Create a Revit Curve from a vertical column
        /// </summary>
        /// <param name="inst">The vertical column instance</param>
        /// <returns>The created Revit Line</returns>
        public Line CreateVerticalColumnCurve(FamilyInstance inst)
        {
            var lp = inst.Location != null ? (LocationPoint)inst.Location : null;
            var pt = lp != null ? lp.Point : null;
            
            var baseLevelParam = inst.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
            var baseLevel = baseLevelParam != null ? a_doc.GetElement(baseLevelParam.AsElementId()) as Level : null;
            
            var topLevelParam = inst.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
            var topLevel = topLevelParam != null ? a_doc.GetElement(topLevelParam.AsElementId()) as Level : null;

            Line retLine = null;
            if (pt != null && baseLevel != null && topLevel != null)
            {
                var baseOffset = inst.GetParameters("Base Offset");
                var baseOffsetVal = baseOffset != null && baseOffset.Count > 0 ? baseOffset[0].AsDouble() : 0.0;

                var topOffset = inst.GetParameters("Top Offset");
                var topOffsetVal = topOffset != null && topOffset.Count > 0 ? topOffset[0].AsDouble() : 0.0;

                var strPt = new XYZ(pt.X, pt.Y, baseLevel.Elevation + baseOffsetVal);
                var endPt = new XYZ(pt.X, pt.Y, topLevel.Elevation + topOffsetVal);

                double length = strPt.DistanceTo(endPt);
                if (length >= a_doc.Application.ShortCurveTolerance)
                {
                    retLine = Line.CreateBound(strPt, endPt);
                }
            }
            return retLine;
        }

        private void ShowElementErrorMessage(Element element, Exception e)
        {
            string elementId = element != null && element.Id != null ? element.Id.ToString() : "";
            string message = "Error encountered while reading element:\n" + "Name=" + element.Name + "\nId=" + elementId + "\n\n";
            message += "Error Message:\n" + e.Message + "\n\n";
            message += "Stack Trace:\n" + e.StackTrace;
            MessageBoxResult mResult = MessageBox.Show(message, "Recoverable Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
