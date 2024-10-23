using System;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.Generic;
using Quaoar.Components;
using System.Windows;
using System.Windows.Data;

namespace Quaoar.Containers
{
    public partial class ClipboardLine : Grid, INotifyPropertyChanged
    {        
        public object Content { get; set; }
        public ClipboardFormat Format { get; set; }

        private int number;
        public int Number
        {
            get { return number; }
            set { number = value;
                OnPropertyChanged("Number");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ClipboardLine(int number, object content, ClipboardFormat format)
        {
            this.Number = number;
            this.Content = content;
            this.Format = format;

            InitializeComponent();

            this.Load();
        }

        private void Load()
        {
            if (this.Content is string)
            {
                TextBlock textblock = new TextBlock() { Text = (string)this.Content, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.White, Padding = new System.Windows.Thickness(5, 0, 0, 0) };
                Grid grid = new Grid() { Background = new LinearGradientBrush(new GradientStopCollection() { new GradientStop(Color.FromArgb(0, 0, 0, 0), 0), new GradientStop(Color.FromArgb(0, 0, 0, 0), 0.6), new GradientStop(Colors.Black, 1) }) { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) } };

                this.MainGrid.Children.Add(textblock);
                this.MainGrid.Children.Add(grid);

                this.MainGrid.ToolTip = this.Content;
            }
            else if (this.Content is System.Drawing.Bitmap)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    ((System.Drawing.Bitmap)this.Content).Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    this.MainGrid.Children.Add(new Image() { Source = bitmapimage });
                }
            }
            else if (this.Content is string[])
            {
                string[] arr = this.Content as string[];
                if (arr.Length > 0)
                {
                    try
                    {
                        if (!String.IsNullOrEmpty(System.IO.Path.GetFullPath(arr[0])))
                        {
                            this.MainGrid.Children.Add(new Image() { Source = new BitmapImage(new Uri("/Images/folder.png", UriKind.RelativeOrAbsolute)), Width = 20, Height = 20, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = System.Windows.VerticalAlignment.Top, Margin = new System.Windows.Thickness(5,5,0,0) });
                            this.MainGrid.Children.Add(new Line() { X1 = 15, Y1 = 30, X2 = 15, Y2 = 40, StrokeThickness = 1, Stroke = Brushes.White });
                            this.MainGrid.Children.Add(new Line() { X1 = 15, Y1 = 40, X2 = 25, Y2 = 40, StrokeThickness = 1, Stroke = Brushes.White });
                            this.MainGrid.Children.Add(new TextBlock() { Margin = new Thickness(35, 5, 0, 0), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Foreground = Brushes.White, Text = new FileInfo(arr[0]).DirectoryName, ToolTip = new FileInfo(arr[0]).DirectoryName });

                            FrameworkElementFactory spFactory = new FrameworkElementFactory(typeof(StackPanel));
                            spFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                            FrameworkElementFactory img = new FrameworkElementFactory(typeof(Image));
                            img.SetValue(Image.WidthProperty, 20.0);
                            img.SetValue(Image.HeightProperty, 20.0);
                            img.SetBinding(Image.SourceProperty, new Binding("Source"));
                            spFactory.AppendChild(img);
                            FrameworkElementFactory tb = new FrameworkElementFactory(typeof(TextBlock));
                            tb.SetBinding(TextBlock.TextProperty, new Binding("Name"));
                            tb.SetBinding(TextBlock.ToolTipProperty, new Binding("Name"));
                            tb.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 0, 0));
                            tb.SetValue(TextBlock.ForegroundProperty, Brushes.White);
                            spFactory.AppendChild(tb);

                            ListBox lb = new ListBox()
                            {
                                Margin = new Thickness(30, 30, 0, 0),
                                Background = Brushes.Transparent,
                                BorderThickness = new Thickness(0),
                                ItemsSource = new List<PathLine>(arr.Select(x => new PathLine(new FileInfo(x).Name, File.GetAttributes(x).HasFlag(FileAttributes.Directory) ? "folder" : "file")).OrderBy(x => x.Name)),
                                ItemTemplate = new DataTemplate() { VisualTree = spFactory }
                            };
                            lb.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                            lb.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                            this.MainGrid.Children.Add(lb);
                        }
                    }
                    catch (Exception) { }
                }
            }
        }
    }
}
