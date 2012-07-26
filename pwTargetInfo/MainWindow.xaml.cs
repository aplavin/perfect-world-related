using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using PwLib;

namespace pwTargetInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private PwClient _client;

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += delegate { DragMove(); };

            _client = PwClient.GetClients().FirstOrDefault();

            var timer = new DispatcherTimer();
            timer.Tick += (s, e) =>
            {
                if (_client == null || _client.Process.HasExited)
                {
                    _client = null;
                    ClearInfo();
                    Hide();
                    return;
                }

                uint target = _client.PlayerInfo.TargetId;
                var player = _client.Environment.GetPlayers().FirstOrDefault(p => p.Id == target);
                if (player != null)
                {
                    bool eye = _client.PlayerInfo.Class == Class.Assasin &&
                        player.Class == Class.Assasin &&
                        player.Level >= _client.PlayerInfo.Level;
                    SetInfo(player.Name, player.Level, player.Class, player.Hp, player.MaxHp, player.Mp, player.MaxMp, eye);
                    Show();
                }
                else
                {
                    SetInfo(_client.PlayerInfo.Name, _client.PlayerInfo.Level, _client.PlayerInfo.Class, _client.PlayerInfo.Hp, _client.PlayerInfo.MaxHp, _client.PlayerInfo.Mp, _client.PlayerInfo.MaxMp, false);
                    Show();
                }
            };
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Start();



            var timerWinUpd = new DispatcherTimer();
            timerWinUpd.Tick += (s, ee) =>
            {
                IntPtr foreground = GetForegroundWindow();
                if(IsActive)
                    return;
                _client = PwClient.GetClients().FirstOrDefault(cl => cl.Process.MainWindowHandle == foreground);
            };
            timerWinUpd.Interval = TimeSpan.FromSeconds(2);
            timerWinUpd.Start();
        }

        private void ClearInfo()
        {
            hpBar.Value = mpBar.Value = 0;
            hpTb.Text = mpTb.Text = "-/-";
        }

        private void SetInfo(string name, int level, Class @class, int hp, int maxHp, int mp, int maxMp, bool eye)
        {
            infoTb.Text = string.Format("{0} - {1} {2}", name, level, @class);

            hpBar.Maximum = maxHp;
            hpBar.Value = hp;
            hpTb.Text = hp == maxHp ? hp.ToString() : string.Format("{0}/{1}", hp, maxHp);

            mpBar.Maximum = maxMp;
            mpBar.Value = mp;
            mpTb.Text = mp == maxMp ? mp.ToString() : string.Format("{0}/{1}", mp, maxMp);

            eyeImg.Visibility = eye ? Visibility.Visible : Visibility.Hidden;
        }

        private void myNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
