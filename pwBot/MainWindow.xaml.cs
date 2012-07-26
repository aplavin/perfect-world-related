using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;
using PwLib;

namespace pwBot
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
            UpdateCharacters();
        }

        private void updateCharBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateCharacters();
        }

        private void UpdateCharacters()
        {
            charactersGrid.ItemsSource = PwClient.GetClients();
            configsCb.ItemsSource = MainLogic.GetConfigs();
        }

        private void attachBtn_Click(object sender, RoutedEventArgs e)
        {
            if (charactersGrid.SelectedIndex == -1)
                return;

            var selected = (PwClient)charactersGrid.SelectedItem;
            MainLogic.SelectCharacter(selected.Pid);
            Title = "pwBot - " + MainLogic.ActiveClient.PlayerInfo.Name;

            configsCb.IsEnabled = false;
            MainLogic.ActiveSettings = MainLogic.CharacterSettings.Load(configsCb.Text);

            var timer = new DispatcherTimer();
            timer.Tick += (s, ev) => UpdateInfo();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private void UpdateInfo()
        {
            mobsGrid.ItemsSource = MainLogic.GetMobs();
            lootGrid.ItemsSource = MainLogic.GetLoot();
            npcsGrid.ItemsSource = MainLogic.GetNpcs();
            resourcesGrid.ItemsSource = MainLogic.GetResources();

            levelLbl.Content = "Level: " + MainLogic.ActiveClient.PlayerInfo.Level + ", player ID: " + MainLogic.ActiveClient.PlayerInfo.PlayerId + ", Coords: " + MainLogic.ActiveClient.PlayerInfo.Coords.ToGameString();

            hpBar.Maximum = MainLogic.ActiveClient.PlayerInfo.MaxHp;
            hpBar.Value = MainLogic.ActiveClient.PlayerInfo.Hp;
            hpLbl.Content = "HP: " + hpBar.Value + "/" + hpBar.Maximum;

            mpBar.Maximum = MainLogic.ActiveClient.PlayerInfo.MaxMp;
            mpBar.Value = MainLogic.ActiveClient.PlayerInfo.Mp;
            mpLbl.Content = "MP: " + mpBar.Value + "/" + mpBar.Maximum;

            Mob tMob;
            if (MainLogic.ActiveClient.PlayerInfo.TargetId != 0 && (tMob = MainLogic.ActiveClient.Environment.GetMob(MainLogic.ActiveClient.PlayerInfo.TargetId)) != null)
            {
                tLevelLbl.Content = "Level: " + tMob.Level + ", ID: " + tMob.Id;
                tHpBar.Maximum = tMob.MaxHp;
                tHpBar.Value = tMob.Hp;
                tHpLbl.Content = "HP: " + tHpBar.Value + "/" + tHpBar.Maximum;

                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                TaskbarItemInfo.ProgressValue = tHpBar.Value / tHpBar.Maximum;
            }
            else
            {
                tLevelLbl.Content = "-";
                tHpBar.Maximum = 0;
                tHpBar.Value = 0;
                tHpLbl.Content = "HP: -";

                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            }
        }

        private void mobsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mobsGrid.SelectedItem == null)
                return;

            var mob = (Mob)mobsGrid.SelectedItem;
            MainLogic.ActiveClient.PacketSender.Select(mob.WorldId);
        }

        private void lootGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lootGrid.SelectedItem == null)
                return;

            var loot = (Loot)lootGrid.SelectedItem;
            MainLogic.ActiveClient.ActionStructs.PickUp(loot.WorldId);
        }

        private void npcsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (npcsGrid.SelectedItem == null)
                return;

            var npc = (Npc)npcsGrid.SelectedItem;
            MainLogic.ActiveClient.ActionStructs.TalkNpc(npc.WorldId);
        }

        private void resourcesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (resourcesGrid.SelectedItem == null)
                return;

            var res = (Resource)resourcesGrid.SelectedItem;
            MainLogic.ActiveClient.ActionStructs.Gather(res.WorldId);
        }

        private void startBtn_Checked(object sender, RoutedEventArgs e)
        {
            ((Image)startBtn.Content).Opacity = 1;
            MainLogic.StartBot();
        }

        private void startBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            ((Image)startBtn.Content).Opacity = 0.5;
            MainLogic.StopBot();
        }

        private void unfreezeCb_Checked(object sender, RoutedEventArgs e)
        {
            if (MainLogic.ActiveClient != null)
                MainLogic.ActiveClient.UnfreezePermanent();
        }

        private void unfreezeCb_Unchecked(object sender, RoutedEventArgs e)
        {
            if (MainLogic.ActiveClient != null)
                MainLogic.ActiveClient.CancelUnfreeze();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            unfreezeCb_Unchecked(null, null);
        }
    }
}
