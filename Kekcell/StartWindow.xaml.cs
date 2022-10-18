using System;
using System.Diagnostics;
using System.Windows;


namespace Kekcell
{
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void CreateNewTable_Click(object sender, RoutedEventArgs e)
        {
            var mw = new MainWindow();
            Close();
            mw.Show();
        }

        private void OpenTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = FileDialogs.Open();
                if (filePath != "")
                {
                    var mw = new MainWindow(filePath);
                    Close();
                    mw.Show();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, ex.Message);
                MessageBox.Show(
                    "Помилка при відкритті файлу",
                    "Kekcell",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
