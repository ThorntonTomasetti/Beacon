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

using Beacon.Properties;

namespace Beacon
{
    /// <summary>
    /// Interaction logic for SettingUI.xaml
    /// </summary>
    public partial class SettingUI : Window
    {
        private RevitReader a_RevitReader;
        private Dictionary<string, List<string>> a_LevelMapDict = new Dictionary<string, List<string>>();
        private string defaultMapName = "Map Name";

        /// <summary>
        /// UI used for pre-export settings.
        /// </summary>
        /// <param name="revitReader">Provide RevitReader instance</param>
        public SettingUI(RevitReader revitReader)
        {
            InitializeComponent();

            a_RevitReader = revitReader;

            // Set Icon
            this.Icon = App.BeaconIcon;

            PopulateLevels();
            SetPreviousState();
        }

        /// <summary>
        /// Add level names from Revit model.
        /// </summary>
        private void PopulateLevels()
        {
            // Add all revit levels to all level list
            foreach (var levelName in a_RevitReader.RevitLevelNames)
            {
                AllLevelListBox.Items.Add(levelName);
            }
        }

        /// <summary>
        /// Set user input to saved states from saved JSON files.
        /// </summary>
        private void SetPreviousState()
        {
            // Set phases select to any available previous state
            List<RevitPhase> savedPhasesSelected = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RevitPhase>>(Settings.Default.SavedPhaseSelected);
            if (savedPhasesSelected != null)
            {
                foreach (var phase in a_RevitReader.RevitPhaseData)
                {
                    if (savedPhasesSelected.Where(x => x.Name == phase.Name && x.Export == true).Count() > 0)
                    {
                        phase.Export = true;
                    }
                    else if (savedPhasesSelected.Where(x => x.Name == phase.Name && x.Export == false).Count() > 0)
                    {
                        phase.Export = false;
                    }
                }
            }
            PhasesListBox.ItemsSource = a_RevitReader.RevitPhaseData;

            // Set level mapping to any availble previous states.
            Dictionary<string, List<string>> savedLevelMaps = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(Settings.Default.SavedMappedLevels);
            if (savedLevelMaps != null)
            {
                foreach (var savedMap in savedLevelMaps)
                {
                    foreach (var level in savedMap.Value)
                    {
                        if (a_RevitReader.RevitLevelNames.Contains(level))
                        {
                            AllLevelListBox.Items.Remove(level);
                            List<string> mapLevels = new List<string>();
                            if (a_LevelMapDict.ContainsKey(savedMap.Key) == true)
                            {
                                a_LevelMapDict.TryGetValue(savedMap.Key, out mapLevels);
                                mapLevels.Add(level);
                            }
                            else
                            {
                                mapLevels.Add(level);
                                a_LevelMapDict.Add(savedMap.Key, mapLevels);
                            }
                        }
                    }
                }
                // Create tree view of previous level mappings.
                foreach (var map in a_LevelMapDict)
                {
                    TreeViewItem tree = new TreeViewItem();
                    tree.Header = map.Key;
                    foreach (var name in map.Value)
                    {
                        tree.Items.Add(new TreeViewItem() { Header = name });
                    }
                    MappedLevelListBox.Items.Add(tree);
                }
                a_RevitReader.a_LevelMap = a_LevelMapDict;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Move levels to Build Map Level.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllLevelForwardButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = AllLevelListBox.SelectedItems;
            List<string> selectedNames = new List<string>();
            foreach (var name in selected)
            {
                selectedNames.Add(name.ToString());
            }
            foreach (var item in selectedNames)
            {
                ToBeMappedLevelListBox.Items.Add(item);
                AllLevelListBox.Items.Remove(item);
            }
        }

        /// <summary>
        /// Clear Build Map Level list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToBeMappedLevelClearButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ToBeMappedLevelListBox.Items)
            {
                if (AllLevelListBox.Items.Contains(item) == false)
                {
                    AllLevelListBox.Items.Add(item);
                }
            }
            AllLevelListBox.Items.SortDescriptions.Clear();
            AllLevelListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("",
                System.ComponentModel.ListSortDirection.Ascending));
            ToBeMappedLevelListBox.Items.Clear();
        }

        /// <summary>
        /// Create Map Level.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToBeMappedLevelForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (ToBeMappedLevelListBox.Items.Count != 0)
            {
                TreeViewItem tree = new TreeViewItem();
                if (String.IsNullOrEmpty(LevelMapNameTextbox.Text) || LevelMapNameTextbox.Text == defaultMapName)
                {
                    tree.Header = ToBeMappedLevelListBox.Items[0];
                }
                else
                {
                    tree.Header = LevelMapNameTextbox.Text;
                }
                List<string> mapNames = new List<string>();
                foreach (var name in ToBeMappedLevelListBox.Items)
                {
                    mapNames.Add(name as string);
                    tree.Items.Add(new TreeViewItem() { Header = name });
                }

                if (a_LevelMapDict.ContainsKey(tree.Header.ToString()))
                {
                    MessageBox.Show("Use Unique Level Name.", "Duplicate Level Name", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    ToBeMappedLevelListBox.Items.Clear();
                    MappedLevelListBox.Items.Add(tree);
                    a_LevelMapDict.Add(tree.Header.ToString(), mapNames);
                    a_RevitReader.a_LevelMap = a_LevelMapDict;
                    LevelMapNameTextbox.Clear();
                    LevelMapNameTextbox.Text = defaultMapName;
                }
            }
        }

        /// <summary>
        /// Clear all mapped levels.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MappedLevelClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            MappedLevelListBox.Items.Clear();
            AllLevelListBox.Items.Clear();
            a_LevelMapDict.Clear();
            Settings.Default.SavedMappedLevels = "";
            PopulateLevels();
        }

        /// <summary>
        /// Clear selected mapped level(s).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MappedLevelClearButton_Click(object sender, RoutedEventArgs e)
        {
            List<TreeViewItem> toBeDeleted = new List<TreeViewItem>();
            var selectedItems = MappedLevelListBox.SelectedItems;
            if (selectedItems.Count > 0)
            {
                foreach (var item in selectedItems)
                {
                    if (item is TreeViewItem)
                    {
                        TreeViewItem treeViewItem = item as TreeViewItem;
                        foreach (TreeViewItem subItem in treeViewItem.Items)
                        {
                            if (a_RevitReader.RevitLevelNames.Contains(subItem.Header))
                            {
                                if (AllLevelListBox.Items.Contains(subItem.Header) == false)
                                {
                                    AllLevelListBox.Items.Add(subItem.Header);
                                }
                            }
                        }
                        if (toBeDeleted.Contains(treeViewItem) == false)
                        {
                            toBeDeleted.Add(treeViewItem);
                        }
                    }
                }
                AllLevelListBox.Items.SortDescriptions.Clear();
                AllLevelListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("",
                    System.ComponentModel.ListSortDirection.Ascending));
                foreach (var item in toBeDeleted)
                {
                    if (a_LevelMapDict.ContainsKey(item.Header.ToString())) a_LevelMapDict.Remove(item.Header.ToString());
                    if (MappedLevelListBox.Items.Contains(item))
                    {
                        MappedLevelListBox.Items.Remove(item);
                    }
                }
            }
        }

        /// <summary>
        /// Clear map name textbox if content is the defaultMapName on focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LevelMapNameTextbox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox mapTextbox = sender as TextBox;
            if (mapTextbox != null && mapTextbox.Text == defaultMapName)
            {
                mapTextbox.Text = "";
            }
        }

        /// <summary>
        /// Restore map name textbox content to defaultMapName if content is blank on lost focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LevelMapNameTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox mapTextbox = sender as TextBox;
            if (mapTextbox != null && mapTextbox.Text.Trim() == "")
            {
                mapTextbox.Text = defaultMapName;
            }
        }

        /// <summary>
        /// Serialize uses inputs as json for restoring on next run.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string mappedLevels = Newtonsoft.Json.JsonConvert.SerializeObject(a_LevelMapDict);
            Settings.Default.SavedMappedLevels = mappedLevels;
            string phasesSelected = Newtonsoft.Json.JsonConvert.SerializeObject(a_RevitReader.RevitPhaseData);
            Settings.Default.SavedPhaseSelected = phasesSelected;
        }
    }
}
