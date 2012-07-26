using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PwLib;

namespace pwCatshopBot
{
    public static class ChatAdvertising
    {
        private static State _state;

        public static void Start()
        {

            _state = State.Working;
            new Thread(DoWork).Start();
        }

        public static void Suspend()
        {
            _state = State.Suspending;
        }

        public static void Resume()
        {
            _state = State.Resuming;
        }

        public static void Stop()
        {
            _state = State.Stopping;
        }

        private static void DoWork()
        {
            var csInitial = Logic.Client.PlayerInfo.GetMyCatshop();
            while (_state != State.Stopping)
            {
                while (_state == State.Suspending)
                    Thread.Sleep(100);

                var cs = Logic.Client.PlayerInfo.GetMyCatshop();
                var sellItem = cs.SellList.Any() ? cs.SellList.Random() : null;
                var buyItem = cs.BuyList.Any() ? cs.BuyList.Random() : null;
                if (sellItem == null && buyItem == null)
                    break;

                bool adBuy = buyItem != null && (sellItem == null || RandomUtils.RandomBoolean());
                if (adBuy)
                {
                    Logic.Client.Chat.SendMessage(string.Format("C> {0} - máxima preço!{1}",
                                                                PwDatabase.GetItemName(buyItem.ItemId, "br"),
                                                                StringUtils.Repeat(" ", RandomUtils.RandomInt(0, 10))));
                    Thread.Sleep(1200);
                    Logic.Client.Chat.SendMessage(string.Format("$C> {0} - máxima preço!{1}",
                                                                PwDatabase.GetItemName(buyItem.ItemId, "br"),
                                                                StringUtils.Repeat(" ", RandomUtils.RandomInt(0, 10))));
                }
                else
                {
                    Logic.Client.Chat.SendMessage(string.Format("V> {0} - mínimo preço!{1}",
                                                                PwDatabase.GetItemName(sellItem.ItemId, "br"),
                                                                StringUtils.Repeat(" ", RandomUtils.RandomInt(0, 10))));
                    Thread.Sleep(1200);
                    Logic.Client.Chat.SendMessage(string.Format("$V> {0} - mínimo preço!{1}",
                                                                PwDatabase.GetItemName(sellItem.ItemId, "br"),
                                                                StringUtils.Repeat(" ", RandomUtils.RandomInt(0, 10))));
                }

                Thread.Sleep(2000);
            }
        }
    }
}
