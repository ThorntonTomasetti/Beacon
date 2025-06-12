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

namespace Beacon
{
    /// <summary>
    /// Interaction logic for DataGridUI.xaml
    /// </summary>
    public partial class DataGridUI : Window
    {
        private List<RevitElement> a_RevitElements = new List<RevitElement>();

        /// <summary>
        /// UI for displaying detailed element list
        /// </summary>
        /// <param name="revitElements">RevitElements to display</param>
        /// <param name="label">Label for list</param>
        public DataGridUI(List<RevitElement> revitElements, string label)
        {
            InitializeComponent();
            a_RevitElements = revitElements;
            DetailElementsDataGrid.ItemsSource = a_RevitElements;
            if (label != string.Empty)
            {
                DataGridUILabel.Content = label;
            }

            // Set Icon
            this.Icon = App.BeaconIcon;
        }

        /// <summary>
        /// Save RevitElements to CSV
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveDetailElementsButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MainUI.WriteCSV<RevitElement>(a_RevitElements, saveFileDialog.FileName);
            }
        }
    }
}
