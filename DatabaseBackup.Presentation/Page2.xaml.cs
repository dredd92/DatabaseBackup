using Microsoft.Win32;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace DatabaseBackup.Presentation
{
    /// <summary>
    /// Логика взаимодействия для Page2.xaml
    /// </summary>
    ///
    internal enum AuthenticationMode
    {
        WindowsAuthentication,
        SqlAuthentication,
    }

    public partial class Page2 : Page
    {
        private string address;
        private AuthenticationMode authMode;
        private string password;
        private string username;

        public Page2()
        {
            InitializeComponent();
            selectAutentification.SelectedItem = selectAutentification.Items[0];            
        }

        private void AddDescriptionToServerInput(object sender, RoutedEventArgs e)
        {
            if (InputServerData.Text == "")
            {
                InputServerData.Text = "Type the path to server...";
            }
        }

        private void BackupButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.choosingDatabase.SelectedItem == null)
            {
                MessageBox.Show("Error: select a database to backup", string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var database = this.choosingDatabase.SelectedItem.ToString();

            var dialog = new SaveFileDialog();
            dialog.FileName = "script";
            dialog.DefaultExt = ".sql";
            dialog.Filter = "SQL Files (.sql)|*.sql";
            var dialogResult = dialog.ShowDialog() ?? false;

            if (!dialogResult)
            {
                return;
            }
            try
            {
                switch (this.authMode)
                {

                    case AuthenticationMode.WindowsAuthentication:
                        LogicKeeper.Logic.BackupLocalInstance(dialog.FileName, this.address, database);
                        MessageBox.Show("Backup completed.");
                        break;

                    case AuthenticationMode.SqlAuthentication:
                        LogicKeeper.Logic.Backup(dialog.FileName, this.address, database, this.username, this.password);
                        MessageBox.Show("Backup completed.");
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

        private void ChoosingDatabase_DropDownOpened(object sender, EventArgs e)
        {
            this.address = this.InputServerData.Text;

            if (this.selectAutentification.SelectedItem == this.selectAutentification.Items[0])
            {
                try
                {
                    this.authMode = AuthenticationMode.WindowsAuthentication;
                    this.choosingDatabase.ItemsSource = LogicKeeper.Logic.ShowDatabasesLocalInstance(this.address);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"An error occured during database backup.{Environment.NewLine}{ex.Message}", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    this.authMode = AuthenticationMode.SqlAuthentication;
                    this.username = this.usernameTextBox.Text;
                    this.password = this.passwordTextBox.Text;
                    this.choosingDatabase.ItemsSource = LogicKeeper.Logic.ShowDatabases(this.address, this.username, this.password);
                } 
                catch (SqlException ex)
                {
                    MessageBox.Show($"An error occured during database backup.{Environment.NewLine}{ex.Message}", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Combobox_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (selectAutentification.SelectedItem == selectAutentification.Items[0])
            {
                this.usernameTextBox.IsEnabled = false;
                this.passwordTextBox.IsEnabled = false;
                this.usernameLabel.IsEnabled = false;
                this.passwordLabel.IsEnabled = false;

                this.usernameTextBox.Text = "(not required)";
                this.passwordTextBox.Text = "(not required)";
            }
            else
            {
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
            if (this.InputServerData.Text == "Type the path to server...")
            {
                this.InputServerData.Text = "";
            }
        }
    }
}

