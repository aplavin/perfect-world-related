using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.CSharp;
using PwLib;
using PwLib.Objects;
using PwLib.Structs;

namespace pwLibTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PwClient _client;

        public MainWindow()
        {
            InitializeComponent();

            var timer = new DispatcherTimer();
            timer.Tick += (s, e) => UpdateInfo();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private void UpdateClientsButtonClick(object sender, RoutedEventArgs e)
        {
            clientsGrid.ItemsSource = PwClient.GetClients();
        }

        private void SelectedClientChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            _client = (PwClient)clientsGrid.SelectedItem;
        }

        private void FillGrid(object obj, DataGrid grid)
        {
            grid.Items.Clear();

            if (obj == null)
                return;

            foreach (var property in obj.GetType().GetProperties())
            {
                grid.Items.Add(new { Name = property.Name, Value = property.GetValue(obj, null) });
            }

            foreach (var field in obj.GetType().GetFields())
            {
                grid.Items.Add(new { Name = field.Name, Value = field.GetValue(obj) });
            }
        }

        private void UpdateInfo()
        {
            if (_client == null) return;

            FillGrid(_client.HostPlayer, playerGrid);
            skillsGrid.ItemsSource = _client.PlayerInfo.GetSkills();

            inventoryGrid.ItemsSource = _client.PlayerInfo.GetInventory();
            equipmentGrid.ItemsSource = _client.PlayerInfo.GetEquipment();
            myCatshopGrid.ItemsSource = _client.PlayerInfo.GetMyCatshop().FullList;
            openedCatshopGrid.ItemsSource = _client.PlayerInfo.GetOpenedCatshop().FullList;

            mobsGrid.ItemsSource = _client.Environment.GetMobs();
            npcsGrid.ItemsSource = _client.Environment.GetNpcs();
            petsGrid.ItemsSource = _client.Environment.GetPets();
            lootGrid.ItemsSource = _client.Environment.GetLoot();
            resourcesGrid.ItemsSource = _client.Environment.GetMines();
            playersGrid.ItemsSource = _client.Environment.GetPlayers();

            uint target = _client.HostPlayer.TargetId;
            object targetObj = null;
            if (targetObj == null) targetObj = _client.Environment.GetMobs().FirstOrDefault(m => m.WorldId == target);
            if (targetObj == null) targetObj = _client.Environment.GetNpcs().FirstOrDefault(m => m.WorldId == target);
            if (targetObj == null) targetObj = _client.Environment.GetPets().FirstOrDefault(m => m.WorldId == target);
            if (targetObj == null) targetObj = _client.Environment.GetLoot().FirstOrDefault(m => m.WorldId == target);
            if (targetObj == null) targetObj = _client.Environment.GetMines().FirstOrDefault(m => m.WorldId == target);
            if (targetObj == null) targetObj = _client.Environment.GetPlayers().FirstOrDefault(m => m.WorldId == target);

            FillGrid(targetObj, targetGrid);
            targetGrid.Items.Insert(0, new { Name = "Type", Value = targetObj != null ? targetObj.GetType().Name : "None" });

        }

        private void FollowClick(object sender, RoutedEventArgs e)
        {
            if (_client == null) return;

            _client.ActionStructs.Follow(_client.HostPlayer.TargetId);
        }

        private void MoveClick(object sender, RoutedEventArgs e)
        {
            if (_client == null) return;

            string title = ((Button)sender).Content.ToString();
            var coords = _client.HostPlayer.Coords;

            float height = flyCb.IsChecked == true ? 50 : -1;

            switch (title.ToUpper())
            {
                case "X++":
                    coords.GameX++;
                    break;
                case "X--":
                    coords.GameX--;
                    break;
                case "Y++":
                    coords.GameY++;
                    break;
                case "Y--":
                    coords.GameY--;
                    break;
            }

            _client.ActionStructs.MoveTo(coords, height);
        }

        private void SelectNearestClick(object sender, RoutedEventArgs e)
        {
            var nearest = _client.Environment.GetMobs().Cast<IPwObject>()
                .Concat(_client.Environment.GetPets())
                .Concat(_client.Environment.GetNpcs())
                .Concat(_client.Environment.GetPlayers())
                .MinItem(o => o.Distance);

            _client.PacketSender.Select(nearest.WorldId);
        }

        private void RegularAttackClick(object sender, RoutedEventArgs e)
        {
            if (_client == null) return;

            _client.PacketSender.RegularAttack(0);
        }

        private void UseSkillClick(object sender, RoutedEventArgs e)
        {
            if (_client == null) return;

            int skillId = int.Parse(skillIdTb.Text);
            uint target = useOnYourself.IsChecked == true ? _client.HostPlayer.WorldId : _client.HostPlayer.TargetId;

            _client.ActionStructs.UseSkill(skillId, target);
        }

        private void UnfreezeClick(object sender, RoutedEventArgs e)
        {
            _client.Unfreeze();
        }

        private void UnfreezePermClick(object sender, RoutedEventArgs e)
        {
            _client.UnfreezePermanent();
        }

        private void FreezePermClick(object sender, RoutedEventArgs e)
        {
            _client.CancelUnfreeze();
        }
    }
}
