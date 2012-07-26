using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Nini.Config;
using Nini.Ini;
using PwLib;

namespace pwBot
{
    public static class MainLogic
    {
        public class CharacterInfo
        {
            public int Pid;
            public string Name { get; set; }
            public int Lvl { get; set; }
            public int Hp { get; set; }
            public int Mp { get; set; }
            public string Coords { get; set; }
        }

        public enum CharacterModes { Grind, Single, Cleric, Frost };

        [Serializable]
        public class CharacterSettings
        {
            public CharacterModes Mode;
            // settings for grind mode
            public int[] MobIds;
            public Coords StartCoords;
            public float Radius;
            public bool AntiKs;
            public bool ExitOnDeath;
            // settings for single mode
            public int UseSpark;
            public bool UseAttack;
            public int[] SkillIds;
            // settings for cleric mode
            public int PlayerId;
            public int[] BuffIds;
            // settings for frost mode
            public int FrostBoss;

            public static CharacterSettings Load(string name)
            {
                string fileName = name + ".cset";

                if (!File.Exists(fileName))
                    throw new FileNotFoundException("Char settings not found", name);

                var settings = new CharacterSettings();
                var configSource = new IniConfigSource(fileName);

                settings.Mode = configSource.Configs["Main"].Get("Mode").To<CharacterModes>();
                if (settings.Mode == CharacterModes.Grind)
                {
                    settings.MobIds = configSource.Configs["Grind"].Get("MobIDs").Split('|').Select(int.Parse).ToArray();
                    settings.StartCoords = new Coords(
                        configSource.Configs["Grind"].GetFloat("CoordX", 0),
                        configSource.Configs["Grind"].GetFloat("CoordY", 0),
                        configSource.Configs["Grind"].GetFloat("CoordZ", 0),
                        true);
                    settings.Radius = configSource.Configs["Grind"].GetFloat("Radius", 10000);
                    settings.AntiKs = configSource.Configs["Grind"].GetBoolean("AntiKS");
                    settings.ExitOnDeath = configSource.Configs["Grind"].GetBoolean("ExitOnDeath");
                }
                else if (settings.Mode == CharacterModes.Single)
                {
                    settings.UseSpark = configSource.Configs["Single"].GetInt("UseSpark", 0);
                    settings.UseAttack = configSource.Configs["Single"].GetBoolean("UseAttack");
                    settings.SkillIds = configSource.Configs["Single"].GetString("SkillIDs").Split('|').WhereNot(string.IsNullOrEmpty).Select(int.Parse).ToArray();
                }
                else if (settings.Mode == CharacterModes.Cleric)
                {
                    settings.PlayerId = configSource.Configs["Cleric"].GetInt("PlayerID");
                    settings.BuffIds = configSource.Configs["Cleric"].GetString("BuffIDs").Split('|').Select(int.Parse).ToArray();
                }
                else if (settings.Mode == CharacterModes.Frost)
                {
                    settings.FrostBoss = configSource.Configs["Frost"].GetInt("BossNumber");
                }

                return settings;
            }

            public void Save(string name)
            {
                string fileName = name + ".cset";

                var configSource = new IniConfigSource(fileName);

                var conf = configSource.AddConfig("Mobs");
                for (int i = 0; i < MobIds.Length; i++)
                {
                    int mId = MobIds[i];
                    conf.Set("Mob" + i, mId);
                }

                conf = configSource.AddConfig("Place");
                conf.Set("CoordX", StartCoords.GameX);
                conf.Set("CoordY", StartCoords.GameX);
                conf.Set("CoordZ", StartCoords.GameX);

                configSource.Save();
            }
        }

        public static PwClient ActiveClient;
        public static CharacterSettings ActiveSettings;
        public static Thread WorkingThread;

        public static void SelectCharacter(int pid)
        {
            ActiveClient = PwClient.GetClients().First(cl => cl.Pid == pid);
        }

        public static Mob[] GetMobs()
        {
            return ActiveClient.Environment.GetMobs().Where(m => m.CurrentAction != CurrentAction.Death).Distinct((m1, m2) => m1.Id == m2.Id).OrderBy(m => m.Distance).ToArray();
        }

        public static Npc[] GetNpcs()
        {
            return ActiveClient.Environment.GetNpcs();
        }

        public static Loot[] GetLoot()
        {
            return ActiveClient.Environment.GetLoot();
        }

        public static Resource[] GetResources()
        {
            return ActiveClient.Environment.GetResources();
        }

        public static void StartBot()
        {
            switch (ActiveSettings.Mode)
            {
                case CharacterModes.Grind:
                    WorkingThread = new Thread(DoGrind);
                    break;
                case CharacterModes.Single:
                    WorkingThread = new Thread(DoSingle);
                    break;
                case CharacterModes.Cleric:
                    WorkingThread = new Thread(DoCleric);
                    break;
                case CharacterModes.Frost:
                    WorkingThread = new Thread(DoFrost);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            WorkingThread.Start();
        }

        public static void StopBot()
        {
            WorkingThread.Abort();
        }

        public static void DoGrind()
        {
            for (; ; )
            {
                if (ActiveClient.PlayerInfo.Hp == 0)
                {
                    if (ActiveSettings.ExitOnDeath)
                    {
                        ActiveClient.PacketSender.LogOut(0);
                    }
                    else
                    {
                        ActiveClient.PacketSender.ResurrectToTown();
                        return;
                    }
                }

                Thread.Sleep(100);
                var mobs = ActiveClient.Environment.GetMobs()
                    .Where(m => m.Id.IsOneOf(ActiveSettings.MobIds))
                    .Where(m => m.Coords.Distance(ActiveSettings.StartCoords) <= ActiveSettings.Radius)
                    .WhereNot(m => m.CurrentAction == CurrentAction.Death)
                    .ToList();

                if (mobs.Empty())
                    continue;

                var mob = mobs.First();
                Thread.Sleep(100);

                ActiveClient.PacketSender.Select(mob.WorldId);
                Thread.Sleep(100);

                while ((mob = ActiveClient.Environment.GetMob(mob.WorldId)).MaxHp == 0)
                    Thread.Sleep(10);

                if (ActiveSettings.AntiKs && mob.Hp < mob.MaxHp)
                    continue;

                while ((mob = ActiveClient.Environment.GetMob(mob.WorldId)) != null && mob.CurrentAction != CurrentAction.Death)
                {
                    ActiveClient.PacketSender.RegularAttack(0);
                    Thread.Sleep(200);
                }
                Thread.Sleep(400);

                var loot = ActiveClient.Environment.GetLoot().Where(l => l.Distance <= 10).ToList();
                foreach (var l in loot)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        ActiveClient.ActionStructs.PickUp(l.WorldId);
                        Thread.Sleep(500);
                    }
                }
            }
        }

        public static void DoSingle()
        {
            while (ActiveClient.PlayerInfo.TargetId != 0)
            {
                Thread.Sleep(200);
                if (ActiveSettings.UseSpark != 0 && ActiveClient.PlayerInfo.Chi >= ActiveSettings.UseSpark * 100)
                {
                    int sparkId;
                    switch (ActiveSettings.UseSpark)
                    {
                        case 1:
                            sparkId = 1178;
                            break;
                        case 2:
                            sparkId = 1179;
                            break;
                        case 3:
                            sparkId = 1196;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                            break;
                    }
                    ActiveClient.PacketSender.UseSkill(sparkId, ActiveClient.PlayerInfo.TargetId);
                    continue;
                }

                if (ActiveSettings.SkillIds.Any())
                    ActiveClient.PacketSender.UseSkill(ActiveSettings.SkillIds.Random(), ActiveClient.PlayerInfo.TargetId);
                Thread.Sleep(200);
                ActiveClient.PacketSender.RegularAttack(1);
            }
        }

        private enum ClericMode { None, Heal, Buff, Follow, Res };

        private static ClericMode _clericMode;

        public static void DoCleric()
        {
            _clericMode = ClericMode.None;
            ActiveClient.Chat.NewMessage += cm =>
            {
                if (cm.Scope != Scopes.Squad)
                    return;

                if (cm.Text.Contains("buff"))
                    _clericMode = ClericMode.Buff;
                else if (cm.Text.Contains("heal"))
                    _clericMode = ClericMode.Heal;
                else if (cm.Text.Contains("follow"))
                {
                    _clericMode = ClericMode.Follow;
                }
                else if (cm.Text.Contains("res"))
                {
                    _clericMode = ClericMode.Res;
                    ActiveClient.PacketSender.UseSkill(18, ActiveClient.PlayerInfo.TargetId);
                    _clericMode = ClericMode.None;
                }
                else
                {
                    _clericMode = ClericMode.None;
                    ActiveClient.ActionStructs.Follow(0);
                }
            };

            for (; ; )
            {
                Thread.Sleep(500);
                switch (_clericMode)
                {
                    case ClericMode.Heal:
                        ActiveClient.PacketSender.Select((uint)ActiveSettings.PlayerId);
                        ActiveClient.PacketSender.UseSkill(114, ActiveClient.PlayerInfo.TargetId);
                        break;
                    case ClericMode.Buff:
                        ActiveClient.PacketSender.Select((uint)ActiveSettings.PlayerId);
                        foreach (var buffId in ActiveSettings.BuffIds)
                        {
                            ActiveClient.PacketSender.UseSkill(buffId, ActiveClient.PlayerInfo.TargetId);
                        }
                        _clericMode = ClericMode.None;
                        break;
                    case ClericMode.Follow:
                        ActiveClient.ActionStructs.Follow(ActiveSettings.PlayerId);
                        Thread.Sleep(1000);
                        break;
                }
            }
        }

        public static void DoFrost()
        {
            switch (ActiveSettings.FrostBoss)
            {
                case 7:
                    int bossId = 24822;
                    int bombId = 24867;
                    while (ActiveClient.Environment.GetMobs(true).Any(m => m.Id == bossId))
                    {
                        if (ActiveClient.Environment.GetMobs(true).Any(m => m.Id == bombId))
                        {
                            var bomb = ActiveClient.Environment.GetMobs(true).First(m => m.Id == bombId);
                            ActiveClient.Additional.DoSelect(bomb.WorldId);
                            ActiveClient.PacketSender.RegularAttack(0);
                            Thread.Sleep(500);
                            continue;
                        }

                        if (ActiveClient.PlayerInfo.Chi > 350)
                        {
                            ActiveClient.PacketSender.UseSkill(1196, 0);
                            Thread.Sleep(500);
                            continue;
                        }

                        var boss = ActiveClient.Environment.GetMobs(true).Single(m => m.Id == bossId);
                        ActiveClient.Additional.DoSelect(boss.WorldId);
                        ActiveClient.PacketSender.RegularAttack(0);
                        Thread.Sleep(1000);
                    }
                    break;
                case 75:
                    int mobId = 24802;
                    while (true)
                    {
                        var mob = ActiveClient.Environment.GetMobs(true).FirstOrDefault(m => m.Id == mobId);
                        if (mob == null)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        ActiveClient.Additional.DoSelect(mob.WorldId);
                        ActiveClient.PacketSender.RegularAttack(0);
                        Thread.Sleep(200);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static string[] GetConfigs()
        {
            return Directory.GetFiles(".", "*.cset").Select(Path.GetFileNameWithoutExtension).ToArray();
        }

        public static string[] GetLoginConfigs()
        {
            return Directory.GetFiles(".", "*.lset").Select(Path.GetFileNameWithoutExtension).ToArray();
        }
    }
}