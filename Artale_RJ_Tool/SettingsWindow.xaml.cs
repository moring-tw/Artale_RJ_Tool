using System.Windows;

namespace WpfFirebaseSync
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void OpenTutorial_Click(object sender, RoutedEventArgs e)
        {
            // 打開教學視窗
            TutorialWindow tutorial = new TutorialWindow { Owner = this };
            tutorial.ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}