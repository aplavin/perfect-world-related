using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PWChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PW _pw;
        private readonly List<IntPtr> _handles = new List<IntPtr>();
        private int _cnt;
        private readonly Dictionary<Scopes, Color> _colors = new Dictionary<Scopes, Color> {
        { Scopes.World, Colors.Gold },
        { Scopes.Local, Colors.DarkSlateGray },
        { Scopes.Squad, Colors.MediumSeaGreen },
        { Scopes.Faction, Colors.DeepSkyBlue },
        { Scopes.Whisper, Colors.Blue },
        { Scopes.Trade, Colors.Brown },

        { Scopes.S5, Colors.LightSkyBlue },
        { Scopes.S6, Colors.LightSkyBlue },
        { Scopes.Notification, Colors.LightSkyBlue },
        { Scopes.System, Colors.LightSkyBlue },
        { Scopes.GenInfo, Colors.LightSkyBlue },
        { Scopes.LocalInfoB, Colors.LightSkyBlue },
        { Scopes.LocalInfoC, Colors.LightSkyBlue },
        };

        private readonly Dictionary<Scopes, string> _prefixes = new Dictionary<Scopes, string> {
        {Scopes.Squad, "!!"},
        {Scopes.Faction, "!~"},
        {Scopes.Trade, "$"},
        };


        public MainWindow()
        {
            InitializeComponent();
        }

        private TabItem TabByName(string name)
        {
            return tabControl.Items.Cast<TabItem>().Single(ti => ti.Name == name);
        }

        private RichTextBox TextBoxByName(string name)
        {
            var tab = tabControl.Items.Cast<TabItem>().Single(ti => ti.Name == name);
            return (RichTextBox)tab.Content;
        }

        private void AddText(ChatMessage cm)
        {
            foreach (var tb in new[] { TextBoxByName(cm.Scope.ToString()), TextBoxByName("All") })
            {
                var p = new Paragraph { Margin = new Thickness(0) };
                var r = new Run(cm.Nickname)
                {
                    Foreground = Brushes.LimeGreen,
                    Cursor = Cursors.Hand,
                };
                r.MouseEnter += (s, e) => { ((Run)s).Background = new RadialGradientBrush(Colors.LightBlue, Colors.LightCyan); };
                r.MouseLeave += (s, e) => { ((Run)s).Background = Brushes.White; };
                r.MouseDown += (s, e) => { message.Text = "/" + cm.Nickname + " "; message.Focus(); };
                p.Inlines.Add(new Bold(r));
                r = new Run
                {
                    Text = (cm.Nickname.IsEmpty() ? "" : ": ") + cm.Text + (cm.ItemId != 0 ? " [+item]" : ""),
                    Foreground = new SolidColorBrush(_colors[cm.Scope]),
                };
                p.Inlines.Add(new Bold(r));
                tb.Document.Blocks.Add(p);


                //var tr = new TextRange(tb.Document.ContentEnd, tb.Document.ContentEnd) { Text = cm.Nickname };
                //tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.LimeGreen);
                //tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.ExtraBold);
                //tr.ApplyPropertyValue(TextElement.FontSizeProperty, 16d);
                //tr.ApplyPropertyValue();

                //tr = new TextRange(tb.Document.ContentEnd, tb.Document.ContentEnd) { Text = (cm.Nickname.IsEmpty() ? "" : ": ") + cm.Text + (cm.ItemId != 0 ? " [+item]" : "") + "\r" };
                //tr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(_colors[cm.Scope]));
                //tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.ExtraBold);
                //tr.ApplyPropertyValue(TextElement.FontSizeProperty, 16d);

                tb.ScrollToEnd();
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            tabControl.Items.Add(new TabItem { Header = "All", Name = "All", IsSelected = true });
            foreach (Scopes scope in Enum.GetValues(typeof(Scopes)))
            {
                tabControl.Items.Add(new TabItem
                {
                    Header = scope.ToString(),
                    Name = scope.ToString(),
                    Visibility = Visibility.Collapsed,
                    Background = null
                });
            }

            foreach (TabItem tab in tabControl.Items)
            {
                tab.Content = new RichTextBox
                {
                    Name = tab.Name,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Background = null,
                    IsReadOnly = true,
                };
            }

            var timer = new DispatcherTimer();
            timer.Tick += (s, ev) =>
            {
                var clients = ElementClient.GetClients();
                clientSelect.Items.Clear();
                clients.ForEach(ec => clientSelect.Items.Add(new ListBoxItem { Content = ec.Nickname, Tag = ec.Pid }));
            };
            timer.Interval = TimeSpan.FromMilliseconds(5000);
            timer.Start();

            _pw = new PW(ElementClient.GetClients()[0]);
            _pw.NewMessage += cm => tabControl.Dispatcher.Invoke(new Action(() =>
            {
                var filters = filter.Text.Split(' ').WhereNot(s => s.Empty());
                if (filters.Empty() || filters.Any(s => cm.Text.ContainsCi(s)))
                {
                    TabByName(cm.Scope.ToString()).Visibility = Visibility.Visible;
                    AddText(cm);
                    _cnt++;
                    if (IsActive)
                        _cnt = 0;
                    UpdateIcon();
                }
            }));
        }

        private void UpdateIcon()
        {
            if (_cnt == 0)
            {
                TaskbarItemInfo.Overlay = null;
                return;
            }

            const int iconWidth = 20;
            const int iconHeight = 20;

            var bmp = new RenderTargetBitmap(iconWidth, iconHeight, 96, 96, PixelFormats.Default);

            var root = new ContentControl
            {
                ContentTemplate = ((DataTemplate)Resources["OverlayIcon"]),
                Content = _cnt.ToString(),
            };

            root.Arrange(new Rect(0, 0, iconWidth, iconHeight));
            bmp.Render(root);

            TaskbarItemInfo.Overlay = bmp;
        }

        private void WindowActivated(object sender, EventArgs e)
        {
            _cnt = 0;
            UpdateIcon();
        }

        private void AttachClick(object sender, RoutedEventArgs e)
        {
            var client = ElementClient.GetClients().Single(ec => ec.Pid == ((ListBoxItem)clientSelect.SelectedItem).Tag.To<int>());
            _pw = new PW(client);

            foreach (TabItem tab in tabControl.Items)
            {
                tab.Visibility = (tab.Name == "All" ? Visibility.Visible : Visibility.Collapsed);
                tab.Content = new RichTextBox
                {
                    Name = tab.Name,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Background = null
                };
            }
        }

        private void MessageKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _pw.SendMessage(message.Text);
                message.Clear();
                message.Text = GetPrefix();
                message.CaretIndex = message.Text.Length;
            }
        }

        private void TabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.Source is TabControl))
                return;

            message.Text = GetPrefix();
        }

        private string GetPrefix()
        {
            try
            {
                return _prefixes[((TabItem)tabControl.SelectedItem).Name.To<Scopes>()];
            }
            catch (KeyNotFoundException)
            {
                return string.Empty;
            }
        }
    }
}
