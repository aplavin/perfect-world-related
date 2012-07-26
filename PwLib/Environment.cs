using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using PwLib.Objects;
using PwLib.Structs;

namespace PwLib
{
    public class Environment
    {
        private readonly PwClient _client;

        public Environment(PwClient client)
        {
            _client = client;
        }

        private IEnumerable<MobStruct> GetMobStructs()
        {
            int nearbyCount = _client.Mem.ReadInt(_client.Addresses.BaseAddress, 0x1C, 0x1c, 0x24, 0x14);
            int pointer = _client.Mem.ResolveNestedPointer(_client.Addresses.BaseAddress, 0x1C, 0x1c, 0x24, 0x50);

            var list = new List<MobStruct>();

            for (int i = 0; i < nearbyCount; i++)
            {
                var mobStruct = _client.Mem.ReadStruct<MobStruct>(pointer, i*4, 0);
                list.Add(mobStruct);
            }

            return list;
        }

        public Mob[] GetMobs(bool undeadOnly = false)
        {
            return GetMobStructs()
                .Where(m => m.Type == 6)
                .Select(m => _client.StructToObject<Mob>(m))
                .Where(m => !undeadOnly || m.Action != MobAction.Death)
                .OrderBy(m => m.Distance)
                .ToArray();
        }

        public Npc[] GetNpcs()
        {
            return GetMobStructs()
                .Where(m => m.Type == 7)
                .Select(m => _client.StructToObject<Npc>(m))
                .OrderBy(m => m.Distance)
                .ToArray();
        }

        public Pet[] GetPets()
        {
            return GetMobStructs()
                .Where(m => m.Type == 9)
                .Select(m => _client.StructToObject<Pet>(m))
                .OrderBy(m => m.Distance)
                .ToArray();
        }

        private unsafe IEnumerable<LootStruct> GetLootStructs()
        {
            int pointer = _client.Mem.ResolveNestedPointer(_client.Addresses.BaseAddress, 0x1C, 0x1c, 0x28, 0x18);

            var list = new List<LootStruct>();

            for (int i = 0; i < 0x300; i++)
            {
                int mobBase = _client.Mem.ReadInt(pointer, i * 0x4, 4);
                if (mobBase == 0)
                    continue;

                var lootStruct = _client.Mem.ReadStruct<LootStruct>(mobBase);

                list.Add(lootStruct);
            }

            return list;
        }

        public Loot[] GetLoot()
        {
            return GetLootStructs()
                .Where(l => l.Type != 2)
                .Select(l => _client.StructToObject<Loot>(l))
                .OrderBy(l => l.Distance)
                .ToArray();
        }

        public Mine[] GetMines()
        {
            return GetLootStructs()
                .Where(l => l.Type == 2)
                .Select(l => _client.StructToObject<Mine>(l))
                .OrderBy(l => l.Distance)
                .ToArray();
        }

        public unsafe Player[] GetPlayers()
        {
            int count = _client.Mem.ReadInt(_client.Addresses.BaseAddress, 0x1C, 0x1C, 0x20, 0x14);
            int pointer = _client.Mem.ResolveNestedPointer(_client.Addresses.BaseAddress, 0x1C, 0x1C, 0x20, 0x88);

            var coords = _client.HostPlayer.Coords;
            var list = new List<Player>();

            for (int i = 0; i < count; i++)
            {
                var pf = _client.Mem.ReadStruct<PlayerStruct>(pointer, i*4, 0);

                var player = _client.StructToObject<Player>(pf);
                player.IsMining = !player.IsMining;
                list.Add(player);
            }

            return list.OrderBy(p => p.Distance).ToArray();
        }
    }
}
