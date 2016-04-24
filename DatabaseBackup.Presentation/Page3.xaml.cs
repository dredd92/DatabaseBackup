using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace DatabaseBackup.Presentation
{
    /// <summary>
    /// Логика взаимодействия для Page3.xaml
    /// </summary>
    public partial class Page3 : Page
    {
        public Page3()
        {
            InitializeComponent();
        }

        private void BrowseButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.DefaultExt = ".sql";
            dialog.Filter = "SQL Files (.sql)|*.sql";
            var dialogResult = dialog.ShowDialog() ?? false;
            if (dialogResult)
            {
                this.selectDBTextBox.Text = dialog.FileName;
            }
        }

        private void RestoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(this.selectDBTextBox.Text))
            {
                MessageBox.Show("File with this path doesn't exist. Please, try again");
            }
            else if (!Regex.IsMatch(this.selectDBTextBox.Text, @".*\.sql"))
            {
                MessageBox.Show("Selected file has wrong type. Please, select another file");
            }

            LogicKeeper.Logic.Restore(this.selectDBTextBox.Text);
            MessageBox.Show("Restoration completed");
        }
    }
}