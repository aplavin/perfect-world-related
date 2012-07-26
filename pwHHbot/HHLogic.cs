using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PwLib;

namespace pwHHbot
{
    public static class HHLogic
    {
        public static PwClient Assasin;
        public static PwClient Cleric;
        private static Thread _thread;

        public static bool CheckCharacters()
        {
            var clients = PwClient.GetClients();
            return clients.Any(cl => cl.PlayerInfo.Name == "chersanya") &&
                   clients.Any(cl => cl.PlayerInfo.Name == "chersаnyа");
        }

        public static void Init()
        {
            var clients = PwClient.GetClients();
            Assasin = clients.First(cl => cl.PlayerInfo.Name == "chersanya");
            Cleric = clients.First(cl => cl.PlayerInfo.Name == "chersаnyа");
            Assasin.UnfreezePermanent();
            Cleric.UnfreezePermanent();
        }

        public static void Start()
        {
            _thread = new Thread(DoHH);
            _thread.Start();
        }

        public static void Stop()
        {
            _thread.Abort();
        }

        private static void GoToUntilDistance(uint mobWid, float distance)
        {
            Assasin.PacketSender.RegularAttack(0);
            while (Assasin.Environment.GetMob(mobWid).Distance > distance)
            {
                Thread.Sleep(100);
            }
            Assasin.PacketSender.CancelAction();
        }

        public static void DoPool(uint worldId)
        {
            var coords = Assasin.PlayerInfo.Coords;
            Assasin.Additional.DoSelect(worldId);
            GoToUntilDistance(worldId, 24);
            Assasin.Additional.PoolGenie(worldId);
            Thread.Sleep(100);
            Assasin.ActionStructs.GoTo(coords);
        }

        public static void KillSelectedMob()
        {
            Assasin.ActionStructs.RegularAttack(Assasin.PlayerInfo.TargetId);
            while (Assasin.PlayerInfo.TargetId != 0)
            {
                Assasin.PacketSender.UseSkill(1122, Assasin.PlayerInfo.TargetId);
                Thread.Sleep(300);
                Assasin.PacketSender.RegularAttack(0);
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
        }

        public static void KillSelectedBoss(bool heal = false)
        {
            if (heal)
            {
                Assasin.PacketSender.UseSkill(1085, 0);
                Cleric.Additional.DoSelect((uint)Assasin.PlayerInfo.PlayerId);
            }

            Assasin.ActionStructs.RegularAttack(Assasin.PlayerInfo.TargetId);

            DateTime lastSpark = DateTime.MinValue;
            while (Assasin.PlayerInfo.TargetId != 0)
            {
                if (Assasin.PlayerInfo.Chi >= 200 && DateTime.Now - lastSpark > TimeSpan.FromSeconds(15))
                {
                    lastSpark = DateTime.Now;
                    Assasin.PacketSender.UseSkill(1179, 0);
                    Thread.Sleep(1000);
                }

                if (heal)
                    Cleric.PacketSender.UseSkill(114, (uint)Assasin.PlayerInfo.PlayerId);

                Assasin.PacketSender.UseSkill(new[] { 1120, 1181, 1122 }.Random(), Assasin.PlayerInfo.TargetId);
                Thread.Sleep(300);
                Assasin.PacketSender.RegularAttack(0);
                Thread.Sleep(1000);
            }
        }

        private static void TEMP()
        {
            Rectangle room;
            uint wid;

            Assasin.Additional.DoSelect(Assasin.Environment.GetMobs(true).First(m => m.Level == 150).WorldId);
            KillSelectedBoss(true);
            Assasin.Additional.CollectLoot();
        }

        private static void Move01(PwClient client)
        {
            // go to the first room
            client.ActionStructs.GoToGame(363.5f, 502.5f);
            client.Additional.WaitForTeleportation(446.2f, 569.9f);
            client.ActionStructs.GoToGame(446.2f, 574.6f);
        }

        private static void Do1Room()
        {
            // first running mob
            uint wid = Assasin.Additional.WaitForDistance(14636, 24);
            Assasin.Additional.DoSelect(wid);
            Assasin.Additional.PoolGenie(wid);
            Thread.Sleep(2000);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();

            // second running mob
            wid = Assasin.Additional.WaitForDistance(14636, 24);
            Assasin.Additional.DoSelect(wid);
            Assasin.PacketSender.UseSkill(1119, Assasin.PlayerInfo.TargetId);
            Thread.Sleep(300);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();

            // other mobs (staying)
            var room = new Rectangle(448, 573.2f, 438.3f, 581, true, 0);
            var mobs = Assasin.Environment.GetMobs(true).Where(m => m.Id == 14636).Where(m => room.IsCoordIn(m.Coords));
            foreach (var mob in mobs)
            {
                Assasin.Additional.DoSelect(mob.WorldId);
                KillSelectedMob();
                Assasin.Additional.CollectLoot();
            }

            Assasin.ActionStructs.GoToGame(443, 577);
        }

        private static void Move12(PwClient client)
        {
            // go to the next room
            client.ActionStructs.GoToGame(436.9f, 577.6f);
            client.ActionStructs.GoToGame(435.4f, 579);
            client.ActionStructs.GoToGame(435.4f, 579);
            client.ActionStructs.GoToGame(434.3f, 580.6f);
            client.ActionStructs.GoToGame(433.1f, 580.6f);
            client.ActionStructs.GoToGame(433.1f, 582.1f);
            client.ActionStructs.GoToGame(430.4f, 585);
            client.ActionStructs.GoToGame(429.1f, 585);
            client.ActionStructs.GoToGame(429.1f, 586.3f);
            client.ActionStructs.GoToGame(425.9f, 587.9f);
        }

        private static void Do2Room()
        {
            var room = new Rectangle(427.3f, 590.1f, 421.4f, 595, true);
            // pool and kill first mob
            DoPool(Assasin.Environment.GetMobs(true).Where(m => room.IsCoordIn(m.Coords)).First(m => m.Id == 14636).WorldId);
            Thread.Sleep(1000);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();
            Thread.Sleep(500);

            // kill second mob
            Assasin.Additional.DoSelect(Assasin.Environment.GetMobs(true).Where(m => room.IsCoordIn(m.Coords)).First(m => m.Id == 14636).WorldId);
            Thread.Sleep(1000);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();
            Thread.Sleep(500);

            // pool and kill third mob
            Assasin.ActionStructs.GoToGame(421.5f, 590.6f);
            DoPool(Assasin.Environment.GetMobs(true).Where(m => room.IsCoordIn(m.Coords)).First(m => m.Id == 14639).WorldId);
            Thread.Sleep(1000);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();
            Thread.Sleep(500);

            // kill fourth mob
            Assasin.PacketSender.Select(Assasin.Environment.GetMobs(true).Where(m => room.IsCoordIn(m.Coords)).First(m => m.Id == 14639).WorldId);
            Thread.Sleep(1000);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();
            Thread.Sleep(500);
        }

        private static void Move23(PwClient client)
        {
            client.ActionStructs.GoToGame(424.8f, 592.9f);
            client.ActionStructs.GoToGame(422.8f, 594.8f);
            client.ActionStructs.GoToGame(422.8f, 596.8f);
        }

        private static void Do3Room()
        {
            Assasin.ActionStructs.GoToGame(425.6f, 596.8f);

            var room = new Rectangle(430.1f, 597.4f, 434.3f, 591.4f, true);
            // kill first mob
            DoPool(Assasin.Environment.GetMobs(true).Where(m => room.IsCoordIn(m.Coords)).First(m => m.Id == 14639).WorldId);
            Thread.Sleep(1000);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();

            Assasin.ActionStructs.GoToGame(425.6f, 596.8f);
            // kill second mob
            DoPool(Assasin.Environment.GetMobs(true).Where(m => room.IsCoordIn(m.Coords)).First(m => m.Id == 14639).WorldId);
            Thread.Sleep(1000);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();

            // go to the room
            Assasin.ActionStructs.GoToGame(430.7f, 596.8f);

            // pool and kill boss
            DoPool(Assasin.Environment.GetMobs(true).First(m => m.Id == 14724).WorldId);
            Assasin.ActionStructs.GoToGame(430.3f, 597.3f);
            Thread.Sleep(2000);
            KillSelectedBoss();
            Assasin.Additional.CollectLoot();

            // pool and kill third mob
            DoPool(Assasin.Environment.GetMobs(true).Where(m => room.IsCoordIn(m.Coords)).First(m => m.Id == 14636).WorldId);
            Assasin.ActionStructs.GoToGame(432.4f, 596.9f);
            Thread.Sleep(1000);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();

            // kill fourth mob
            Assasin.Additional.DoSelect(Assasin.Environment.GetMobs(true).Where(m => room.IsCoordIn(m.Coords)).First(m => m.Id == 14636).WorldId);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();
        }

        private static void Move34(PwClient client)
        {
            client.ActionStructs.GoToGame(432.4f, 592.7f);
            client.ActionStructs.GoToGame(434.3f, 591.8f);
            client.ActionStructs.GoToGame(434.8f, 591.8f);
        }

        private static void Do4Room()
        {
            var room = new Rectangle(437.8f, 590.8f, 441.8f, 595.4f, true);

            // pool and kill first standing mob
            DoPool(Assasin.Environment.GetMobs(true).Where(m => room.IsCoordIn(m.Coords)).First(m => m.Id == 14636).WorldId);
            Thread.Sleep(1000);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();

            Assasin.ActionStructs.GoToGame(436.3f, 591.8f);
            // pool and kill first running mob
            var wid = Assasin.Additional.WaitForDistance(14639, 24);
            Assasin.Additional.DoSelect(wid);
            Assasin.Additional.PoolGenie(wid);
            Assasin.ActionStructs.GoToGame(435f, 591.8f);
            Thread.Sleep(2000);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();

            Assasin.ActionStructs.GoToGame(436.3f, 591.8f);
            // pool and kill first running mob
            wid = Assasin.Additional.WaitForDistance(14639, 24);
            Assasin.Additional.DoSelect(wid);
            Assasin.Additional.PoolGenie(wid);
            Assasin.ActionStructs.GoToGame(435f, 591.8f);
            Thread.Sleep(2000);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();

            Assasin.ActionStructs.GoToGame(438f, 591.8f);
            Assasin.ActionStructs.GoToGame(437.8f, 590.8f);

            // kill last mob
            Assasin.Additional.DoSelect(Assasin.Environment.GetMobs(true).Where(m => room.IsCoordIn(m.Coords)).First(m => m.Id == 14636).WorldId);
            KillSelectedMob();
            Assasin.Additional.CollectLoot();
        }

        private static void Move45(PwClient client)
        {
            client.ActionStructs.GoToGame(439.8f, 591.8f);
            client.ActionStructs.GoToGame(439.8f, 594.7f);
        }

        private static void Do5Room()
        {
            Assasin.ActionStructs.GoToGame(439.9f, 596f);
            // kill all mobs
            while (Assasin.Environment.GetMobs(true).Where(m => m.Id == 14638).Any())
            {
                Assasin.Additional.DoSelect(Assasin.Environment.GetMobs(true).First(m => m.Id == 14638).WorldId);
                KillSelectedMob();
                Assasin.Additional.CollectLoot();
            }
        }

        private static void Move56(PwClient client)
        {
            client.ActionStructs.GoToGame(438.2f, 597.8f);
            client.ActionStructs.GoToGame(436.3f, 598.6f);
            client.ActionStructs.GoToGame(433.6f, 598.6f);
            client.Additional.WaitForBomb();
            client.ActionStructs.GoToGame(425.5f, 598.6f);
            client.Additional.WaitForBomb();
            client.ActionStructs.GoToGame(417.3f, 598.6f);
            client.Additional.WaitForBomb();
            client.ActionStructs.GoToGame(414.5f, 598.6f);
            client.ActionStructs.GoToGame(414.5f, 593.2f);
            client.ActionStructs.GoToGame(418.3f, 589.9f);
            client.ActionStructs.GoToGame(418.3f, 587.1f);
            client.ActionStructs.GoToGame(423.5f, 587.1f);
            client.ActionStructs.GoToGame(423.5f, 585.3f);
            client.ActionStructs.GoToGame(425.6f, 585.3f);
            client.ActionStructs.GoToGame(425.6f, 586.3f);
            client.ActionStructs.GoToGame(422.8f, 586.3f);
            client.ActionStructs.GoToGame(422.8f, 585.1f);
            client.ActionStructs.GoToGame(421.2f, 585.1f);
            client.ActionStructs.GoToGame(421.2f, 570f);
            client.ActionStructs.GoToGame(422.8f, 570f);
            client.ActionStructs.GoToGame(422.8f, 568.1f);
            client.ActionStructs.GoToGame(425.8f, 567.5f);
        }

        private static void Do6Room()
        {
        }

        private static void DoHH()
        {
            Move01(Assasin);
            //Move01(Cleric);

            Do1Room();

            Move12(Assasin);
            //Move12(Cleric);
            Thread.Sleep(500);
            Assasin.PacketSender.UseSkill(1093, 0);
            Thread.Sleep(5000);
            Assasin.ActionStructs.GoToGame(426, 590.7f);
            Assasin.ActionStructs.GoToGame(427.3f, 590.5f);

            Do2Room();

            //Cleric.ActionStructs.GoToGame(426, 590.7f);
            Move23(Assasin);
            //Move23(Cleric);

            Do3Room();

            Move34(Assasin);
            //Cleric.ActionStructs.GoToGame(432.2f, 596.8f);
            //Move34(Cleric);

            Do4Room();

            Move45(Assasin);
            //Move45(Cleric);

            Do5Room();

            //Cleric.ActionStructs.GoToGame(440, 598.6f);
            Move56(Assasin);
            //Move56(Cleric);

            Do6Room();
        }
    }
}
