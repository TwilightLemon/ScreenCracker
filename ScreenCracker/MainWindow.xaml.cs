using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ScreenCracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += delegate{
                keybd_event(Keys.ControlKey, 0, 2, 0);
            };
            Loaded += delegate {
                InstallHotKey();
            };
        }
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ControlButtonText.Text == "开始")
            {
                ControlButtonText.Text = "停止";
                Frequency.IsReadOnly = true;
                tx.IsReadOnly = true;
                int Count = 0;
                int.TryParse(Frequency.Text, out int i);
                var t = TimeSpan.FromMilliseconds(i);
                if (t <= TimeSpan.FromMilliseconds(100))
                {
                    t = TimeSpan.FromMilliseconds(100);
                    Frequency.Text = "100";
                    State.Text = "最低只能使用100毫秒";
                }

                Action a = new(async delegate
                {
                    if (!UseClipboardData)
                        System.Windows.Clipboard.SetText(tx.Text);
                    while (true)
                    {
                        if (ControlButtonText.Text == "开始")
                            break;
                        if (on.IsChecked == true && tw.IsChecked == false)
                        {
                            keybd_event(Keys.ControlKey, 0, 0, 0);
                            await Task.Delay(10);
                            keybd_event(Keys.V, 0, 0, 0);
                            await Task.Delay(10);
                            keybd_event(Keys.ControlKey, 0, 2, 0);
                            keybd_event(Keys.V, 0, 2, 0);
                            await Task.Delay(10);

                            keybd_event(Keys.Enter, 0, 0, 0);
                            await Task.Delay(10);
                            keybd_event(Keys.Enter, 0, 2, 0);
                        }
                        else
                        {
                            keybd_event(Keys.ControlKey, 0, 0, 0);
                            await Task.Delay(10);
                            keybd_event(Keys.V, 0, 0, 0);
                            await Task.Delay(10);
                            keybd_event(Keys.ControlKey, 0, 2, 0);
                            keybd_event(Keys.V, 0, 2, 0);
                            await Task.Delay(10);
                            keybd_event(Keys.ControlKey, 0, 0, 0);
                            await Task.Delay(10);
                            keybd_event(Keys.Enter, 0, 0, 0);
                            await Task.Delay(10);
                            keybd_event(Keys.ControlKey, 0, 2, 0);
                            keybd_event(Keys.Enter, 0, 2, 0);
                        }
                        Count++;
                        State.Text = "已刷屏次数: " + Count;
                        await Task.Delay(t);
                    }
                });
                a();
            }
            else
            {
                Frequency.IsReadOnly = false;
                tx.IsReadOnly = false;
                ControlButtonText.Text = "开始";
            }
        }

        private void Frequency_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            //只允许粘贴数字
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!isNumberic(text))
                { e.CancelCommand(); }
            }
            else { e.CancelCommand(); }
        }


        private void Frequency_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //不允许输入空格
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        private void Frequency_PreviewTextInput(object sender, TextCompositionEventArgs e)
            => e.Handled = !isNumberic(e.Text);


        public static bool isNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string)
                if (!char.IsDigit(c))
                    return false;
            return true;
        }
        private bool UseClipboardData = false;
        private string PreviousData;
        private void Intro_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(UseClipboardData)
            {
                UseClipboardData = false;
                tx.Text = PreviousData;
                IntroText.Text = "导自剪切板";
            }
            else
            {
                UseClipboardData = true;
                PreviousData = tx.Text;
                tx.Text = System.Windows.Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(tx.Text))
                    tx.Text = "(正在使用剪切板数据...)";
                IntroText.Text = "Clipboard On";
            }
        }

        private void HotKeys_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
        [DllImport("user32")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, uint virtualKey);
        [DllImport("user32")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private void InstallHotKey() 
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            RegisterHotKey(handle, 126, 2, (uint)Keys.Space);
            HwndSource.FromHwnd(handle).AddHook(HotKeyHook);

        }
        private void UninstallHotKey()
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, 126);
        }
        private IntPtr HotKeyHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) 
        {
            if (msg == 0x0312)
                if (wParam.ToInt32() == 126)
                    Border_MouseDown(null, null);

            return IntPtr.Zero;
        }
        private bool UseHotKey = true;
        private void HotKey_Cancel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (UseHotKey) {
                UseHotKey = false;
                HotKey_Cancel_Text.Text= "注册";
                UninstallHotKey();
            }
            else
            {
                UseHotKey = true;
                HotKey_Cancel_Text.Text = "取消";
                InstallHotKey();
            }
        }
    }
}
