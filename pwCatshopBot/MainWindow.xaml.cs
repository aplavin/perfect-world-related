using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using PwLib;

namespace pwCatshopBot
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
        }

        private void charactersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (charactersGrid.SelectedIndex == -1)
                return;

            Logic.Client = (PwClient)charactersGrid.SelectedItem;
            Logic.Client.UnfreezePermanent();

            if (catsWalkingRb.IsChecked.Value)
                CatsWalking.Start();
            else if (chatAdsRb.IsChecked.Value)
                ChatAdvertising.Start();

            var timer = new DispatcherTimer();
            timer.Tick += updateInfo_Click;
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Start();
        }

        private void updateInfo_Click(object sender, object e)
        {
            if (catsWalkingRb.IsChecked.Value)
            {
                catsItemsGrid.ItemsSource = new string[0];
                catsItemsGrid.ItemsSource = CatsWalking.Items;
                Title = "Cnt: " + CatsWalking.CatshopCnt;
            }
            else if (chatAdsRb.IsChecked.Value)
            {
                catItemsGrid.ItemsSource = new string[0];
                var cs = Logic.Client.PlayerInfo.GetMyCatshop();
                catItemsGrid.ItemsSource = cs.SellList.Concat(cs.BuyList).ToList();
            }
        }

        private void itemsGrid_SelectedCellsChanged(object sender, object e)
        {
            if (catsItemsGrid.SelectedItem == null)
                return;

            sellersGrid.ItemsSource = new string[0];
            buyersGrid.ItemsSource = new string[0];
            sellersGrid.ItemsSource = CatsWalking.GetSellers(((ItemInfo)catsItemsGrid.SelectedItem).Id);
            buyersGrid.ItemsSource = CatsWalking.GetBuyers(((ItemInfo)catsItemsGrid.SelectedItem).Id);
        }

        private void itemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (catsItemsGrid.SelectedItem == null)
                return;
            Process.Start("http://pwdatabase.com/ru/items/" + ((ItemInfo)catsItemsGrid.SelectedItem).Id);
        }

        private void suspend_Click(object sender, RoutedEventArgs e)
        {
            if (catsWalkingRb.IsChecked.Value)
                CatsWalking.Suspend();
            else if (chatAdsRb.IsChecked.Value)
                ChatAdvertising.Suspend();
        }

        private void resume_Click(object sender, RoutedEventArgs e)
        {
            if (catsWalkingRb.IsChecked.Value)
                CatsWalking.Resume();
            else if (chatAdsRb.IsChecked.Value)
                ChatAdvertising.Resume();
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            if (catsWalkingRb.IsChecked.Value)
                CatsWalking.Stop();
            else if (chatAdsRb.IsChecked.Value)
                ChatAdvertising.Stop();
        }

        private void sellersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sellersGrid.SelectedItem == null)
                return;

            CatsWalking.Suspend();
            int id = ((Catshop)sellersGrid.SelectedItem).Id;
            Logic.Client.Additional.DoSelect((uint)id);
            Logic.Client.ActionStructs.TalkNpc((uint)id);
        }

        private void buyersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (buyersGrid.SelectedItem == null)
                return;

            CatsWalking.Suspend();
            int id = ((Catshop)buyersGrid.SelectedItem).Id;
            Logic.Client.Additional.DoSelect((uint)id);
            Logic.Client.ActionStructs.TalkNpc((uint)id);
        }
    }
}
