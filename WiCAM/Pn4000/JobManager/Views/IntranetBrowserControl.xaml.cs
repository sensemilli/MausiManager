using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace WiCAM.Pn4000.JobManager.Views
{
    public partial class IntranetBrowserControl : UserControl, IView
    {
        public static IntranetBrowserControl Instance;
        private WebView2 webView;
        private string _defaultUrl = "http://intranet.local"; // Ihre Intranet-URL

        public IntranetBrowserControl()
        {
            InitializeComponent();
            Instance = this;
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                webView = new WebView2
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                // WebView2 zum Grid hinzufügen
                gridWebView.Children.Add(webView);

                // WebView2 initialisieren
                await webView.EnsureCoreWebView2Async(null);

                // Event-Handler registrieren
                webView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
                webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                webView.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;

                // Standard-URL laden
                LoadUrl(_defaultUrl);

                Logger.Info("WebView2 erfolgreich initialisiert");
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                MessageBox.Show($"Fehler beim Initialisieren des Browsers: {ex.Message}", 
                              "Browser-Fehler", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        public void LoadUrl(string url)
        {
            try
            {
                if (webView?.CoreWebView2 != null)
                {
                    txtUrl.Text = url;
                    webView.CoreWebView2.Navigate(url);
                    Logger.Info("Navigiere zu URL: {0}", url);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                MessageBox.Show($"Fehler beim Laden der URL: {ex.Message}", 
                              "Navigation-Fehler", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            btnRefresh.IsEnabled = false;
            Logger.Verbose("Navigation gestartet: {0}", e.Uri);
        }

        private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            progressBar.Visibility = Visibility.Collapsed;
            btnRefresh.IsEnabled = true;

            if (e.IsSuccess)
            {
                Logger.Verbose("Navigation erfolgreich abgeschlossen");
            }
            else
            {
                Logger.Error("Navigation fehlgeschlagen. Fehlercode: {0}", e.WebErrorStatus);
                MessageBox.Show($"Fehler beim Laden der Seite: {e.WebErrorStatus}", 
                              "Navigation-Fehler", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
            }
        }

        private void CoreWebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            if (webView?.CoreWebView2 != null)
            {
                txtUrl.Text = webView.CoreWebView2.Source;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (webView?.CoreWebView2 != null && webView.CoreWebView2.CanGoBack)
            {
                webView.CoreWebView2.GoBack();
            }
        }

        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            if (webView?.CoreWebView2 != null && webView.CoreWebView2.CanGoForward)
            {
                webView.CoreWebView2.GoForward();
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            webView?.CoreWebView2?.Reload();
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            LoadUrl(_defaultUrl);
        }

        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            string url = txtUrl.Text.Trim();
            if (!string.IsNullOrEmpty(url))
            {
                // HTTP-Protokoll hinzufügen, falls nicht vorhanden
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "http://" + url;
                }
                LoadUrl(url);
            }
        }

        private void TxtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnGo_Click(sender, e);
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

        // Öffentliche Methoden für externe Nutzung
        public void NavigateToIntranetPage(string relativePath)
        {
            string fullUrl = _defaultUrl.TrimEnd('/') + "/" + relativePath.TrimStart('/');
            LoadUrl(fullUrl);
        }

        public void SetDefaultUrl(string url)
        {
            _defaultUrl = url;
            Logger.Info("Standard-URL gesetzt: {0}", url);
        }

        public async System.Threading.Tasks.Task<string> ExecuteScriptAsync(string script)
        {
            try
            {
                if (webView?.CoreWebView2 != null)
                {
                    return await webView.CoreWebView2.ExecuteScriptAsync(script);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
            return null;
        }
    }
}