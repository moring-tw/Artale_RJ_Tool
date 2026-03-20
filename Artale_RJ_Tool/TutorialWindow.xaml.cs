using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace WpfFirebaseSync
{
    public partial class TutorialWindow : Window
    {
        public TutorialWindow()
        {
            InitializeComponent();
        }

        // 處理超連結點擊，開啟系統預設瀏覽器
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                // .NET Core / .NET 8 中啟動外部 URL 需要加上 UseShellExecute = true
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });

                e.Handled = true; // 標記事件已處理
            }
            catch
            {
                MessageBox.Show("無法開啟瀏覽器，請手動前往 https://console.firebase.google.com/");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}