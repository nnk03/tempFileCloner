using System.Windows;
using System.Windows.Controls;

namespace FileCloner.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    { 
        /// <summary>
        ///  Creates an instance of the main window.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Page mainPage = new MainPage();
            MainFrame.Navigate(mainPage);
        }
    }
}
