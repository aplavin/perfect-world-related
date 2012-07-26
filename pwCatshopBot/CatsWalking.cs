using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PwLib;

namespace pwCatshopBot
{

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Price Price { get; set; }
        public int Amount { get; set; }
    }

    public class Catshop
    {
        public int Id { get; set; }
        public string PlayerName { get; set; }
        public string ShopName { get; set; }
        public Item[] SellItems { get; set; }
        public Item[] BuyItems { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class ItemInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Price MinSell { get; set; }
        public Price MaxBuy { get; set; }
        public int NumSell { get; set; }
        public int NumBuy { get; set; }
    }

    public class Price : IComparable<Price>
    {
        public int Value { get; set; }

        private Price(int value)
        {
            Value = value;
        }

        public Price(string price)
        {
            double value = price.GetBefore("k").To<double>();
            value *= Math.Pow(1000, price.Count('k'));
            Value = (int)value;
        }

        public override string ToString()
        {
            string suffix = string.Empty;
            double value = Value;
            while (value >= 1000)
            {
                value /= 1000;
                suffix += 'k';
            }
            return value.ToString("0.##") + suffix;
        }

        public int CompareTo(Price other)
        {
            return Value.CompareTo(other.Value);
        }

        public static implicit operator Price(int value)
        {
            return new Price(value);
        }
    }

    public static class CatsWalking
    {
        public static List<ItemInfo> Items
        {
            get
            {
                var list = new List<ItemInfo>();

                var sellItems = Catshops.SelectMany(cs => cs.SellItems);
                foreach (var item in sellItems)
                {
                    if (!list.Any(i => i.Id == item.Id))
                    {
                        list.Add(new ItemInfo
                        {
                            Id = item.Id,
                            Name = item.Name,
                            MinSell = item.Price,
                            NumSell = item.Amount,
                        });
                        continue;
                    }

                    var ii = list.Single(i => i.Id == item.Id);
                    ii.NumSell += item.Amount;
                    if (item.Price.IsLess(ii.MinSell))
                        ii.MinSell = item.Price;
                }

                var buyItems = Catshops.SelectMany(cs => cs.BuyItems);
                foreach (var item in buyItems)
                {
                    if (!list.Any(i => i.Id == item.Id))
                    {
                        list.Add(new ItemInfo
                        {
                            Id = item.Id,
                            Name = item.Name,
                            MaxBuy = item.Price,
                            NumBuy = item.Amount,
                        });
                        continue;
                    }

                    var ii = list.Single(i => i.Id == item.Id);
                    ii.NumBuy += item.Amount;
                    if (item.Price.IsGreater(ii.MaxBuy))
                        ii.MaxBuy = item.Price;
                }

                return list.OrderByDescending(ii => ii.NumSell + ii.NumBuy).ToList();
            }
        }
        public static readonly List<Catshop> Catshops = new List<Catshop>();
        public static int CatshopCnt;
        private static State _state;

        public static List<Catshop> GetSellers(int id)
        {
            return Catshops.Where(cs => cs.SellItems.Any(si => si.Id == id)).ToList();
        }

        public static List<Catshop> GetBuyers(int id)
        {
            return Catshops.Where(cs => cs.BuyItems.Any(si => si.Id == id)).ToList();
        }

        public static void Start()
        {
            new Thread(DoWork).Start();
        }

        public static void Suspend()
        {
            if (_state == State.Working)
                _state = State.Suspending;

            while (_state != State.Suspended)
                Thread.Sleep(100);
        }

        public static void Resume()
        {
            if (_state == State.Suspended)
                _state = State.Resuming;

            while (_state != State.Working)
                Thread.Sleep(100);
        }

        public static void Stop()
        {
            if (_state == State.Working)
                _state = State.Stopping;

            while (_state != State.Stopped)
                Thread.Sleep(100);
        }

        public static void DoWork()
        {
            var rect = new Rectangle(116, 861, 125, 857, true);
            _state = State.Working;

            for (; ; )
            {
                var visited = new List<Player>();
                while (true)
                {
                    if (_state == State.Suspending)
                    {
                        _state = State.Suspended;
                        while (_state != State.Resuming)
                            Thread.Sleep(100);
                        _state = State.Working;
                    }
                    else if (_state == State.Stopping)
                    {
                        _state = State.Stopped;
                        return;
                    }

                    Catshops.RemoveAll(cs => DateTime.Now - cs.LastUpdate > TimeSpan.FromMinutes(5));

                    var players = Logic.Client.Environment.GetPlayers().Where(p => rect.IsCoordIn(p.Coords)).Where(p => p.IsCatshop).ToList();

                    var player = players.FirstOrDefault(p => !visited.Any(vis => vis.Id == p.Id));
                    if (player == null)
                        break;
                    visited.Add(player);
                    CatshopCnt = visited.Count;

                    Logic.Client.Additional.DoSelect((uint)player.Id);
                    Thread.Sleep(500);
                    int cnt = 0;
                    while (Logic.Client.PlayerInfo.CurrentDialogId != player.Id && cnt < 10)
                    {
                        cnt++;
                        Logic.Client.ActionStructs.TalkNpc((uint)player.Id);
                        Thread.Sleep(500);
                    }
                    if (Logic.Client.PlayerInfo.CurrentDialogId != player.Id)
                        continue;
                    cnt = 0;
                    while (Logic.Client.PlayerInfo.GetOpenedCatshop().Empty && cnt++ < 30)
                        Thread.Sleep(100);
                    if (Logic.Client.PlayerInfo.GetOpenedCatshop().Empty)
                        continue;

                    Catshops.RemoveAll(cs => cs.Id == player.Id);
                    Catshops.Add(new Catshop
                    {
                        Id = player.Id,
                        PlayerName = player.Name,
                        ShopName = player.ShopName,
                        BuyItems = Logic.Client.PlayerInfo.GetOpenedCatshop().BuyList.
                        Select(ii => new Item { Id = ii.ItemId, Name = PwDatabase.GetItemName(ii.ItemId), Price = ii.SellPrice, Amount = ii.Amount }).
                        ToArray(),
                        SellItems = Logic.Client.PlayerInfo.GetOpenedCatshop().SellList.
                        Select(ii => new Item { Id = ii.ItemId, Name = PwDatabase.GetItemName(ii.ItemId), Price = ii.SellPrice, Amount = ii.Amount }).
                        ToArray(),
                        LastUpdate = DateTime.Now,
                    });

                    Thread.Sleep(500);
                }
            }
        }
    }
}
