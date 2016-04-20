using DatabaseBackup.BLL;
using DatabaseBackup.ContractsBLL;
using System;
using System.Windows;

namespace DatabaseBackup.Presentation
{
    public partial class MainWindow : Window
    {
        private ILogic BL = new Logic();

        public MainWindow()
        {
            InitializeComponent();
            frame.NavigationService.Navigate(new Uri("Page1.xaml", UriKind.Relative));
        }        
    }
}