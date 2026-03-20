using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;

namespace WpfFirebaseSync
{
    // ==========================================
    // 新增：用來包裝顏色的資料結構，避免 JSON 解析錯誤
    // ==========================================
    public class CellData
    {
        public string Color { get; set; } = string.Empty;
    }

    public partial class GridWindow : Window
    {
        private string _guid;
        private FirebaseClient _firebaseClient;
        private IDisposable? _subscription;

        // 儲存 40 個格子的顏色
        private string[] _colors = new string[40];
        // 儲存按鈕的參考
        private Button[] _buttons = new Button[40];

        public GridWindow(string guid)
        {
            InitializeComponent();
            _guid = guid;
            this.Title = $"房間: {_guid}";

            _firebaseClient = new FirebaseClient(AppConfig.Instance.FirebaseUrl);

            GenerateGridUI();
            StartListening();
        }

        private void GenerateGridUI()
        {
            for (int i = 0; i < 40; i++) _colors[i] = "";

            for (int i = 10; i >= 1; i--)
            {
                YAxisGrid.Children.Add(new TextBlock
                {
                    Text = i.ToString(),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.Bold
                });
            }

            for (int r = 0; r < 10; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    int index = r * 4 + c;
                    Button btn = new Button
                    {
                        Tag = index,
                        Margin = new Thickness(2),
                        Background = Brushes.WhiteSmoke
                    };

                    btn.PreviewMouseLeftButtonDown += Btn_LeftClick;
                    btn.PreviewMouseRightButtonDown += Btn_RightClick;

                    _buttons[index] = btn;
                    ButtonGrid.Children.Add(btn);
                }
            }
        }

        // ==========================================
        // 左鍵：填入自己的顏色
        // ==========================================
        private async void Btn_LeftClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender is Button btn && btn.Tag is int clickedIndex)
            {
                string myColor = AppConfig.Instance.SelectedColor;
                int rowStart = (clickedIndex / 4) * 4;

                try
                {
                    // 1. 檢查同一列是否已經有「自己的顏色」，有的話先清除
                    for (int i = 0; i < 4; i++)
                    {
                        int checkIndex = rowStart + i;

                        if (checkIndex != clickedIndex && _colors[checkIndex] == myColor)
                        {
                            // 樂觀更新：先在本地清除 UI
                            _colors[checkIndex] = "";
                            UpdateSingleUI(checkIndex);

                            // 通知 Firebase 刪除該格子的資料
                            _ = _firebaseClient
                                .Child("SyncRooms")
                                .Child(_guid)
                                .Child("Cells")
                                .Child(checkIndex.ToString())
                                .DeleteAsync();
                        }
                    }

                    // 2. 將新點擊的格子設為自己的顏色
                    _colors[clickedIndex] = myColor;
                    UpdateSingleUI(clickedIndex);

                    // 通知 Firebase 寫入新顏色 (注意這裡改成了傳遞 CellData 物件)
                    await _firebaseClient
                        .Child("SyncRooms")
                        .Child(_guid)
                        .Child("Cells")
                        .Child(clickedIndex.ToString())
                        .PutAsync(new CellData { Color = myColor });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"更新時出錯: {ex.Message}");
                }
            }
        }

        // ==========================================
        // 右鍵：取消顏色
        // ==========================================
        private async void Btn_RightClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender is Button btn && btn.Tag is int clickedIndex)
            {
                string myColor = AppConfig.Instance.SelectedColor;

                // 限制：只能取消「自己寫入的顏色」
                if (_colors[clickedIndex] == myColor)
                {
                    // 樂觀更新
                    _colors[clickedIndex] = "";
                    UpdateSingleUI(clickedIndex);

                    try
                    {
                        // 從 Firebase 移除該格子資料
                        await _firebaseClient
                            .Child("SyncRooms")
                            .Child(_guid)
                            .Child("Cells")
                            .Child(clickedIndex.ToString())
                            .DeleteAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"刪除時出錯: {ex.Message}");
                    }
                }
            }
        }

        // ==========================================
        // 監聽 Firebase 變化
        // ==========================================
        private void StartListening()
        {
            // 注意這裡改成了監聽 CellData 物件
            _subscription = _firebaseClient
                .Child("SyncRooms")
                .Child(_guid)
                .Child("Cells")
                .AsObservable<CellData>()
                .Subscribe(data =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (int.TryParse(data.Key, out int index) && index >= 0 && index < 40)
                        {
                            // 如果該節點被刪除 (例如有人按右鍵取消，或同列互斥刪除)
                            if (data.EventType == FirebaseEventType.Delete || data.Object == null)
                            {
                                _colors[index] = "";
                            }
                            // 如果是新增或修改，取出 Object 裡面的 Color 屬性
                            else
                            {
                                _colors[index] = data.Object.Color ?? "";
                            }

                            // 更新該按鈕 UI
                            UpdateSingleUI(index);
                        }
                    });
                });
        }

        // ==========================================
        // 單一按鈕 UI 更新
        // ==========================================
        private void UpdateSingleUI(int index)
        {
            string hex = _colors[index];
            if (string.IsNullOrEmpty(hex))
            {
                _buttons[index].Background = Brushes.WhiteSmoke;
            }
            else
            {
                try
                {
                    var converter = new BrushConverter();
                    _buttons[index].Background = (Brush?)converter.ConvertFromString(hex) ?? Brushes.WhiteSmoke;
                }
                catch
                {
                    _buttons[index].Background = Brushes.WhiteSmoke;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _subscription?.Dispose();
        }
    }
}