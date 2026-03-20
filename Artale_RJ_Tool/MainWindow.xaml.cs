using System.Windows;

namespace WpfFirebaseSync
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow { Owner = this };
            settings.ShowDialog();
        }

        private void OpenGrid_Click(object sender, RoutedEventArgs e)
        {
            string guid = GuidTextBox.Text.Trim();
            if (string.IsNullOrEmpty(guid))
            {
                MessageBox.Show("請先輸入 GUID！");
                return;
            }

            // 開啟網格視窗並傳遞 GUID
            GridWindow gridWindow = new GridWindow(guid);
            gridWindow.Owner = this;
            gridWindow.Show();
        }
    }
}