using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace pwHHbot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer();
            timer.Tick += (s, ee) => UpdateInfo();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private void UpdateInfo()
        {
            charsStateLbl.Content = HHLogic.CheckCharacters() ? "Characters OK" : "Characters NOT OK";
            
            if (HHLogic.Assasin != null && HHLogic.Cleric != null)
            {
                assasinPb.Maximum = HHLogic.Assasin.PlayerInfo.MaxHp;
                assasinPb.Value = HHLogic.Assasin.PlayerInfo.Hp;
                assasinLbl.Content = string.Format("Assasin: HP {0}/{1}", assasinPb.Value, assasinPb.Maximum);

                clericPb.Maximum = HHLogic.Cleric.PlayerInfo.MaxHp;
                clericPb.Value = HHLogic.Cleric.PlayerInfo.Hp;
                clericLbl.Content = string.Format("Cleric: HP {0}/{1}", clericPb.Value, clericPb.Maximum);
            }
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            startBtn.IsEnabled = false;
            HHLogic.Init();
            HHLogic.Start();
        }
    }
}
