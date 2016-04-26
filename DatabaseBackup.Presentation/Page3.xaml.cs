using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

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

            try
            {
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
                        MessageBox.Show("Error. Something unexpected happened.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"An error occured during database backup.{Environment.NewLine}{ex.Message}", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
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
                this.authMode = AuthenticationMode.WindowsAuthentication;
                this.usernameTextBox.IsEnabled = false;
                this.passwordTextBox.IsEnabled = false;
                this.usernameLabel.IsEnabled = false;
                this.passwordLabel.IsEnabled = false;

                this.usernameTextBox.Text = "(not required)";
                this.passwordTextBox.Text = "(not required)";
            }
            else
            {
                this.authMode = AuthenticationMode.SqlAuthentication;
                this.usernameTextBox.IsEnabled = true;
                this.passwordTextBox.IsEnabled = true;
                this.usernameLabel.IsEnabled = true;
                this.passwordLabel.IsEnabled = true;

                this.usernameTextBox.Text = "";
                this.passwordTextBox.Text = "";
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