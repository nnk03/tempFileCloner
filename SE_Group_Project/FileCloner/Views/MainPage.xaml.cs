﻿using System.Windows;
using System.Windows.Controls;
using FileCloner.ViewModels;

namespace FileCloner.Views
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        /// <summary>
        /// Creates an instance of the main page.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            try
            {
                // Create the ViewModel and set as data context.
                MainPageViewModel viewModel = new();
                DataContext = viewModel;
            }
            catch (Exception exception)
            {
                _ = MessageBox.Show(exception.Message);
                Application.Current.Shutdown();
            }
        }
    }
}
