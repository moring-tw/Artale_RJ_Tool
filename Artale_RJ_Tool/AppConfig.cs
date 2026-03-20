using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfFirebaseSync
{
    public class AppConfig : INotifyPropertyChanged
    {
        public static AppConfig Instance { get; } = new AppConfig();

        private string _firebaseUrl = "";
        public string FirebaseUrl
        {
            get => _firebaseUrl;
            set { _firebaseUrl = value; OnPropertyChanged(); }
        }

        private bool _isTopmost = false;
        public bool IsTopmost
        {
            get => _isTopmost;
            set { _isTopmost = value; OnPropertyChanged(); }
        }

        private double _windowOpacity = 1.0;
        public double WindowOpacity
        {
            get => _windowOpacity;
            set { _windowOpacity = value; OnPropertyChanged(); }
        }

        // 新增：儲存使用者目前選擇的顏色 (預設為紅色)
        private string _selectedColor = "#FF0000";
        public string SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // 更新：資料結構改為儲存陣列序列化後的 JSON 字串
    public class RoomData
    {
        public string GridDataJson { get; set; } = string.Empty;
    }
}