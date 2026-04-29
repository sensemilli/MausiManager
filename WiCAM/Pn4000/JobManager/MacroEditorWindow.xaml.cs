using System.Windows;

namespace WiCAM.Pn4000.JobManager
{
    public partial class MacroEditorWindow : Window
    {
        public MacroRecorder Recorder { get; }
        public MacroEditorWindow(MacroRecorder recorder)
        {
            InitializeComponent();
            Recorder = recorder;
            DataContext = recorder;
        }

        private void Ok_Click(object sender, RoutedEventArgs e) => DialogResult = true;
        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}