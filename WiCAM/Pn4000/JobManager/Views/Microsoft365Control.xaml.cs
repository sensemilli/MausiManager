using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager.ViewModels;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace WiCAM.Pn4000.JobManager.Views
{
    public partial class Microsoft365Control : UserControl, IView
    {
        public static Microsoft365Control Instance;

        public Microsoft365Control()
        {
            InitializeComponent();
            Instance = this;

            // ViewModel initialisieren
            DataContext = new Microsoft365ViewModel();

            Loaded += Microsoft365Control_Loaded;
        }

        private async void Microsoft365Control_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Automatisch Dateien laden wenn Control geladen wird
                if (DataContext is Microsoft365ViewModel viewModel)
                {
                    if (viewModel.LoadFilesCommand.CanExecute(null))
                    {
                        viewModel.LoadFilesCommand.Execute(null);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        // IView Implementation
        object IView.DataContext()
        {
            return this.DataContext;
        }

        void IView.DataContext(object value)
        {
            this.DataContext = value;
        }
    }
}