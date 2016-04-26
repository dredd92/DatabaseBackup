using System;
using System.Windows;
using System.Windows.Controls;

namespace DatabaseBackup.Presentation
{
    /// <summary>
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
        }

        private void BackupButtonClick(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("Page2.xaml", UriKind.Relative));
        }

        private void RestoreButtonClick(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("Page3.xaml", UriKind.Relative));
        }
    }
}