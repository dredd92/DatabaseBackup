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

namespace DatabaseBackup.Presentation
{
    /// <summary>
    /// Логика взаимодействия для Page2.xaml
    /// </summary>
    public partial class Page2 : Page
    {
        public Page2()
        {
            InitializeComponent();
            selectAutentification.SelectedItem = selectAutentification.Items[0];

            //combobox.Items.Add();
        }

        private string connectionString = "";
        private string username = "";
        private string password = "";

        private void RemoveText(object sender, RoutedEventArgs e)
        {
            if(InputServerData.Text == "Type the path to server...")
            {
                InputServerData.Text = "";
            }            
        }

        private void AddDescriptionToServerInput(object sender, RoutedEventArgs e)
        {
            if(InputServerData.Text == "")
            {
                InputServerData.Text = "Type the path to server...";
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

        private void BackupButtonClick(object sender, RoutedEventArgs e)
        {

        }        
    }
}

/*string errorText = "abvacas";
Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "DatabaseBackup"));
            File.WriteAllText($"log_{DateTime.Now.ToString("DD-MM-yyyy_HH-mm-ss")}", errorText);*/