using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using PwLib;
using System.Linq;

namespace PwConsole
{
    static class Program
    {
        private static PwClient _client;

        private static void OpenAllChips()
        {
            int ind;
            while ((ind = _client.PlayerInfo.GetInventory().IndexOf(ii => ii.ItemId == 21049)) != -1)
            {
                int cnt = _client.PlayerInfo.GetInventory().Where(ii => ii.ItemId == 19277).Sum(ii => ii.Amount);
                _client.PacketSender.UseItem(0, (byte)ind, 21049);
                while (_client.PlayerInfo.GetInventory().Where(ii => ii.ItemId == 19277).Sum(ii => ii.Amount) < cnt + 10)
                    Thread.Sleep(100);
            }
        }

        private static void Main(string[] args)
        {
            _client = PwClient.GetClients().First(cl => cl.PlayerInfo.Name == "chersanya");
            var ps = _client.Environment.GetPlayers();
            _client.UnfreezePermanent();
            OpenAllChips();
        }
    }
}
