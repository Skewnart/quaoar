﻿using Quaoar.Containers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Quaoar
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private const int HOTKEY_ID = 9000;

        public static readonly int MAXITEMS = 50;
        public bool Selecting { get; set; } = false;

        public ObservableCollection<ClipboardLine> Lines { get; set; } = new ObservableCollection<ClipboardLine>();

        private IntPtr _windowHandle;
        private HwndSource _source;

        private ClipboardLine selectedLine;
        public ClipboardLine SelectedLine
        {
            get { return selectedLine; }
            set
            {
                selectedLine = value;
                OnPropertyChanged("SelectedTile");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                ClipboardMonitor.Start();
                ClipboardMonitor.OnClipboardChange += new ClipboardMonitor.OnClipboardChangeEventHandler(ClipboardMonitor_OnClipboardChange);

                this.Visibility = Visibility.Hidden; //Need to be here for the monitor
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, 0x0006, 0x58);
            RegisterHotKey(_windowHandle, HOTKEY_ID, 0x0006, 0x57);
            RegisterHotKey(_windowHandle, HOTKEY_ID, 0x0006, 0x56);
            RegisterHotKey(_windowHandle, HOTKEY_ID, 0x0006, 0x43);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    handleIt();
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        private void handleIt()
        {
            if (this.Visibility == Visibility.Hidden)
                this.Visibility = Visibility.Visible;
        }

        private void ClipboardMonitor_OnClipboardChange(ClipboardFormat format, object data)
        {
            if (data != null && !this.Selecting)
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    this.Lines.Insert(0, new ClipboardLine(1, data, format));
                    foreach (ClipboardLine line in this.Lines)
                        line.Number = this.Lines.IndexOf(line) + 1;

                    while (this.Lines.Count > MAXITEMS)
                        this.Lines.RemoveAt(MAXITEMS);
                }));
            }

            this.Selecting = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            ClipboardMonitor.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.EnableBlur();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                if ((e.Key == Key.Escape))
                    this.Visibility = Visibility.Hidden;
                else if (this.SelectedLine != null)
                {
                    if (e.Key == Key.Enter)
                    {
                        this.Selecting = true;
                        Clipboard.SetData(this.SelectedLine.Format.ToString(), this.SelectedLine.Content);
                        this.Visibility = Visibility.Hidden;
                    }
                    else if (e.Key == Key.Delete)
                    {
                        int index = this.Lines.IndexOf(this.SelectedLine);
                        this.Lines.Remove(this.SelectedLine);
                        foreach (ClipboardLine line in this.Lines)
                            line.Number = this.Lines.IndexOf(line) + 1;

                        if (this.Lines.Count > 0)
                            this.SelectedLine = index < this.Lines.Count ? this.Lines[index] : this.Lines.Last();
                    }
                }
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.SelectedLine != null)
            {
                this.Selecting = true;
                Clipboard.SetData(this.SelectedLine.Format.ToString(), this.SelectedLine.Content);
                this.Visibility = Visibility.Hidden;
            }
        }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        internal void EnableBlur()
        {
            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData()
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };
            SetWindowCompositionAttribute(new WindowInteropHelper(this).Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }
    internal enum AccentState
    {
        ACCENT_ENABLE_BLURBEHIND = 3,
    }
}
