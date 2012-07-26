using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using AvalonDock;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using JimBlackler.DocsByReflection;
using Microsoft.CSharp;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using PwLib;
using PwLib.Objects;
using Path = System.IO.Path;

namespace scripterPw
{
    public enum MsgType { System, Out, Error, Success };

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string LayoutFile = "DockLayout.xml";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            FillInfoPanel();

            UpdateClientsClick(sender, e);
            UpdateScriptsClick(sender, e);

            //if (File.Exists(LayoutFile))
            //    dockManager.RestoreLayout(LayoutFile);
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            //dockManager.SaveLayout(LayoutFile);
        }

        private void InsertText(string text)
        {
            scriptTb.SelectedText = text;
        }

        private TreeViewItem CreateTreeViewItem(string header, string iconName)
        {
            var child = new TreeViewItem();
            var pan = new StackPanel();
            pan.Orientation = Orientation.Horizontal;
            pan.Children.Add(new Image { Height = 16, Width = 16, Stretch = Stretch.None, Source = new BitmapImage(new Uri("images\\" + iconName, UriKind.Relative)) });
            pan.Children.Add(new TextBlock(new Run("  " + header)));
            child.Header = pan;
            return child;
        }

        private void FillInfoPanel()
        {
            var types = new[]
            {
                new {Category = "Characters", Type = typeof (Mob)},
                new {Category = "Characters", Type = typeof (Npc)},
                new {Category = "Characters", Type = typeof (Pet)},
                new {Category = "Characters", Type = typeof (Player)},
                new {Category = "On ground", Type = typeof (Loot)},
                new {Category = "On ground", Type = typeof (Mine)},
                new {Category = "Other", Type = typeof (HostPlayer)},
                new {Category = "Other", Type = typeof (InventoryItem)},
                new {Category = "Other", Type = typeof (Skill)},
            };
            types = types.Concat(
                typeof(PwObject).Assembly.GetTypes()
                .Where(t => t.Namespace != null && t.Namespace.EndsWith("Objects") && t.IsEnum)
                .Select(t => new { Category = "Enums", Type = t }))
                .ToArray();

            foreach (var category in types.GroupBy(t => t.Category))
            {
                var catItem = CreateTreeViewItem(category.Key, "folder.png");
                classesTree.Items.Add(catItem);

                foreach (var type in category.Select(t => t.Type).OrderBy(t => t.Name))
                {
                    var typeItem = CreateTreeViewItem(type.Name, "class.png");
                    catItem.Items.Add(typeItem);
                    var items = new List<TreeViewItem>();
                    typeItem.Tag = items;

                    var type1 = type;
                    typeItem.MouseDoubleClick += (s, e) => InsertText(type1.Name);

                    if (type.IsEnum)
                    {
                        foreach (string enumValue in type.GetEnumNames())
                        {
                            string fullVal = string.Format("{0}.{1}", type.Name, enumValue);

                            var item = CreateTreeViewItem(enumValue, "property.png");
                            item.MouseDoubleClick += (s, e) => InsertText(fullVal);

                            items.Add(item);
                        }
                    }
                    else
                    {
                        foreach (var property in type.GetProperties().OrderBy(p => p.Name))
                        {
                            var prop = property;

                            var item = CreateTreeViewItem(string.Format("{0}  ({1})", property.Name, property.PropertyType.Name), "property.png");
                            item.ToolTip = CommonUtils.Safe(() => DocsByReflection.XMLFromMember(prop)["summary"].InnerText.Trim());

                            item.MouseDoubleClick += (s, e) => InsertText(prop.Name);

                            items.Add(item);
                        }
                    }
                }
            }

            foreach (var property in typeof(PwInterface).GetProperties().OrderBy(p => p.Name))
            {
                string insertStr = property.Name + "()";
                var propItem = CreateTreeViewItem(property.Name, "property.png");
                propItem.MouseDoubleClick += (s, e) => InsertText(insertStr);

                var panel = new StackPanel();
                propItem.Tag = panel;

                var linkTb = new TextBlock();
                linkTb.Inlines.Add("[");
                var link = new Hyperlink(new Run("Insert"));
                link.Click += (s, e) => InsertText(insertStr);
                linkTb.Inlines.Add(link);
                linkTb.Inlines.Add("]");
                linkTb.Inlines.Add(new LineBreak());

                panel.Children.Add(linkTb);

                try
                {
                    var xmlDoc = DocsByReflection.XMLFromMember(property);
                    string summaryInfo = xmlDoc["summary"].InnerText.Trim();
                    string returnInfo = CommonUtils.Safe(() => xmlDoc["returns"].InnerText.Trim());

                    var tb = new TextBlock { TextWrapping = TextWrapping.Wrap };

                    tb.Inlines.Add(new Bold(new Run("Summary")));
                    tb.Inlines.Add(new LineBreak());
                    tb.Inlines.Add(summaryInfo);
                    tb.Inlines.Add(new LineBreak());

                    panel.Children.Add(tb);
                }
                catch
                {
                    panel.Children.Add(new TextBlock(new Bold(new Italic(new Run("Sorry, no info")))));
                }

                methodsTree.Items.Add(propItem);
            }

            Func<ParameterInfo, string> parameterToString = p =>
            {
                if (Attribute.IsDefined(p, typeof(ParamArrayAttribute)))
                    return string.Format("[*{0}]", p.Name);

                if (p.IsOptional)
                {
                    if (p.RawDefaultValue is string)
                        return string.Format("[{0} = \"{1}\"]", p.Name, p.RawDefaultValue);

                    return string.Format("[{0} = {1}]", p.Name, p.RawDefaultValue ?? "None");
                }

                return p.Name;
            };

            foreach (var method in typeof(PwInterface).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName).OrderBy(m => m.Name))
            {
                string infoStr = string.Format("{0}({1})", method.Name, method.GetParameters().Aggregate(parameterToString, ", "));
                string insertStr = string.Format("{0}()", method.Name);

                var methodItem = CreateTreeViewItem(infoStr, "method.png");
                methodItem.MouseDoubleClick += (s, e) => InsertText(insertStr);

                var panel = new StackPanel();
                methodItem.Tag = panel;

                var linkTb = new TextBlock();
                linkTb.Inlines.Add("[");
                var link = new Hyperlink(new Run("Insert"));
                link.Click += (s, e) => InsertText(insertStr);
                linkTb.Inlines.Add(link);
                linkTb.Inlines.Add("]");
                linkTb.Inlines.Add(new LineBreak());

                panel.Children.Add(linkTb);

                try
                {
                    var xmlDoc = DocsByReflection.XMLFromMember(method);
                    string summaryInfo = xmlDoc["summary"].InnerText.Trim();
                    string returnInfo = CommonUtils.Safe(() => xmlDoc["returns"].InnerText.Trim());

                    var tb = new TextBlock { TextWrapping = TextWrapping.Wrap };

                    tb.Inlines.Add(new Bold(new Run("Summary")));
                    tb.Inlines.Add(new LineBreak());
                    tb.Inlines.Add(summaryInfo);
                    tb.Inlines.Add(new LineBreak());
                    tb.Inlines.Add(new LineBreak());
                    if (xmlDoc.ChildNodes.OfType<XmlElement>().Where(xel => xel.Name == "param").Any())
                    {
                        tb.Inlines.Add(new Bold(new Run("Parameters")));
                        tb.Inlines.Add(new LineBreak());
                        foreach (var param in xmlDoc.ChildNodes.OfType<XmlElement>().Where(xel => xel.Name == "param"))
                        {
                            tb.Inlines.Add(new Italic(new Run(param.GetAttribute("name") + ": ")));
                            tb.Inlines.Add(new Run(param.InnerText.Trim()));
                            tb.Inlines.Add(new LineBreak());
                        }
                        tb.Inlines.Add(new LineBreak());
                    }
                    if (!returnInfo.IsNullOrWhiteSpace())
                    {
                        tb.Inlines.Add(new Bold(new Run("Return value")));
                        tb.Inlines.Add(new LineBreak());
                        tb.Inlines.Add(returnInfo);
                        tb.Inlines.Add(new LineBreak());
                    }

                    panel.Children.Add(tb);
                }
                catch
                {
                    panel.Children.Add(new TextBlock(new Bold(new Italic(new Run("Sorry, no info")))));
                }

                methodsTree.Items.Add(methodItem);
            }
        }

        private void UpdateClientsClick(object sender, RoutedEventArgs e)
        {
            clientsGrid.ItemsSource = PwClient.GetClients();
        }

        private void UpdateScriptsClick(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists("scripts"))
            {
                scriptsGrid.ItemsSource = Directory.EnumerateFiles("scripts", "*.py").Select(f => new Script(f)).ToArray();
            }
        }

        private void UpdateGameinfoClick(object sender, RoutedEventArgs e)
        {
            var client = (PwClient)clientsGrid.SelectedItem;
            if (client == null)
                return;

            object[] arr;
            switch ((string)((ComboBoxItem)gameinfoCombo.SelectedValue).Tag)
            {
                case "mobs":
                    arr = client.Environment.GetMobs();
                    break;
                case "pets":
                    arr = client.Environment.GetPets();
                    break;
                case "npcs":
                    arr = client.Environment.GetNpcs();
                    break;
                case "players":
                    arr = client.Environment.GetPlayers();
                    break;
                case "loot":
                    arr = client.Environment.GetLoot();
                    break;
                case "mines":
                    arr = client.Environment.GetMines();
                    break;
                case "skills":
                    arr = client.PlayerInfo.GetSkills();
                    break;
                case "inventory":
                    arr = client.PlayerInfo.GetInventory();
                    break;
                case "equip":
                    arr = client.PlayerInfo.GetEquipment();
                    break;
                case "mycat":
                    arr = client.PlayerInfo.GetMyCatshop();
                    break;
                case "opencat":
                    arr = client.PlayerInfo.GetOpenedCatshop();
                    break;
                default:
                    return;
            }

            gameinfoGrid.ItemsSource = arr;
        }

        private void SaveScriptClick(object sender, RoutedEventArgs e)
        {
            if (scriptsGrid.SelectedItem == null)
                return;

            File.WriteAllText(Path.Combine("scripts", (string)scriptsGrid.SelectedValue), scriptTb.Text);
        }

        private void Log(dynamic msg, MsgType type)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                string s;
                if (msg == null)
                    s = "null";
                else if (msg is IEnumerable && !(msg is string))
                    s = ((IEnumerable)msg).Cast<dynamic>().Aggregate(", ");
                else
                    s = msg.ToString();

                var tr = new TextRange(logTb.Document.ContentEnd, logTb.Document.ContentEnd);
                tr.Text = string.Format("[{0:HH:mm:ss}] {1}\n", DateTime.Now, s);

                Brush brush = null;
                switch (type)
                {
                    case MsgType.System:
                        brush = Brushes.Brown;
                        break;
                    case MsgType.Out:
                        brush = Brushes.Black;
                        break;
                    case MsgType.Error:
                        brush = Brushes.Red;
                        break;
                    case MsgType.Success:
                        brush = Brushes.Green;
                        break;
                }

                tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);

                logTb.ScrollToEnd();
            }));
        }

        private void StartScriptClick(object sender, RoutedEventArgs e)
        {
            var script = (Script)((Button)sender).DataContext;
            script.Start((PwClient)clientsGrid.SelectedItem, Log);
        }

        private void StopScriptClick(object sender, RoutedEventArgs e)
        {
            var script = (Script)((Button)sender).DataContext;
            script.Stop();
        }

        private void scriptsGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (scriptsGrid.SelectedItem == null)
            {
                scriptTb.Text = string.Empty;
            }
            else
            {
                var script = (Script)scriptsGrid.SelectedItem;
                scriptTb.Text = script.Source;
            }
        }

        private void scriptTb_TextChanged(object sender, EventArgs e)
        {
            if (scriptsGrid.SelectedItem == null)
                return;

            var script = (Script)scriptsGrid.SelectedItem;
            script.Source = scriptTb.Text;
        }

        private void scriptTb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (scriptTb.Text.Substring(0, scriptTb.CaretOffset).EndsWith(":\r\n"))
                {
                    int offset = scriptTb.CaretOffset;
                    scriptTb.Text = scriptTb.Text.Insert(scriptTb.CaretOffset, "\t");
                    scriptTb.CaretOffset = offset + 1;
                }
            }
        }

        private void UnfreezeClick(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var client = (PwClient)element.DataContext;
            if ((string)element.Tag == "on")
                client.UnfreezePermanent();
            else
                client.CancelUnfreeze();
        }

        private void CoordsMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var client = (PwClient)((FrameworkElement)sender).DataContext;
                var coords = client.HostPlayer.Coords;
                InsertText(string.Format(NumberFormatInfo.InvariantInfo, "({0}, {1})", coords.GameX, coords.GameY));
            }
        }
    }
}