using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
                    MessageBox.Show("Error");
                    break;
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
                catch (SqlException)
                {
                    MessageBox.Show("Error");
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
                catch (SqlException)
                {
                    MessageBox.Show("Error:");
                }
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
            if (InputServerData.Text == "Type the path to server...")
            {
                InputServerData.Text = "";
            }
        }
    }
}

