using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PwLib.Objects;

namespace PwLib
{
    public class Additional
    {
        private readonly PwClient _client;

        public Additional(PwClient client)
        {
            _client = client;
        }

        public void WaitForBomb()
        {
            Mob mob;
            while (!((mob = _client.Environment.GetMobs().FirstOrDefault(m => m.Id == 15669)) != null && mob.Distance < 20 && mob.Action == MobAction.Death))
                Thread.Sleep(100);
            Thread.Sleep(1000);
        }

        public uint WaitForDistance(int mobId, float distance)
        {
            if (!_client.Environment.GetMobs(true).Any(m => m.Id == mobId))
                throw new ArgumentException();

            while (_client.Environment.GetMobs(true).First(m => m.Id == mobId).Distance > distance)
                Thread.Sleep(100);

            return _client.Environment.GetMobs(true).First(m => m.Id == mobId).WorldId;
        }
    }
}
