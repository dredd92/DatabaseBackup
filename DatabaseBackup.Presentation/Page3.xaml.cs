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
        private string address;
        private AuthenticationMode authMode;
        private string password;
        private string username;

        public Page3()
        {
            InitializeComponent();
            selectAutentification.SelectedItem = selectAutentification.Items[0];
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

            switch (this.authMode)
            {
                case AuthenticationMode.WindowsAuthentication:
                    LogicKeeper.Logic.RestoreLocalInstance(this.selectDBTextBox.Text, this.serverAddressTextBox.Text);
                    MessageBox.Show("Restoration completed.");
                    break;

                case AuthenticationMode.SqlAuthentication:
                    this.address = this.serverAddressTextBox.Text;
                    this.username = this.usernameTextBox.Text;
                    this.password = this.passwordTextBox.Text;
                    LogicKeeper.Logic.Restore(this.selectDBTextBox.Text, this.address, this.username, this.password);
                    MessageBox.Show("Restoration completed.");
                    break;

                default:
                    MessageBox.Show("Error");
                    break;
            }
        }

        private void AddDescriptionToServerInput(object sender, RoutedEventArgs e)
        {
            if (serverAddressTextBox.Text == "")
            {
                serverAddressTextBox.Text = "Type the path to server...";
            }
        }

        private void Combobox_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (selectAutentification.SelectedItem == selectAutentification.Items[0])
            {
                usernameTextBox.IsEnabled = false;
                passwordTextBox.IsEnabled = false;
                usernameLabel.IsEnabled = false;
                passwordLabel.IsEnabled = false;

                usernameTextBox.Text = "(not required)";
                passwordTextBox.Text = "(not required)";
            }
            else
            {
                usernameTextBox.IsEnabled = true;
                passwordTextBox.IsEnabled = true;
                usernameLabel.IsEnabled = true;
                passwordLabel.IsEnabled = true;

                usernameTextBox.Text = "";
                passwordTextBox.Text = "";
            }
        }

        private void RemoveText(object sender, RoutedEventArgs e)
        {
            if (serverAddressTextBox.Text == "Type the path to server...")
            {
                serverAddressTextBox.Text = "";
            }
        }
    }
}