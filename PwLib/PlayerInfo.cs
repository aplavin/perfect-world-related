using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using PwLib.Objects;
using PwLib.Structs;

namespace PwLib
{
    public class Catshop
    {
        public bool Empty { get { return SellList.Empty() && BuyList.Empty(); } }
        public InventoryItem[] SellList { get; set; }
        public InventoryItem[] BuyList { get; set; }
        public InventoryItem[] FullList { get { return SellList.Concat(BuyList).ToArray(); } }

        public static implicit operator InventoryItem[](Catshop c)
        {
            return c.FullList;
        }
    }

    public class PlayerInfo
    {
        private readonly PwClient _client;

        public PlayerInfo(PwClient client)
        {
            _client = client;
        }

        private InventoryItem[] ReadInventoryArray(int listPointer, bool excludeEmpty)
        {
            var list = new List<InventoryItem>();
            int maxCnt = _client.Mem.ReadInt(listPointer, 0x14);
            int pointer = _client.Mem.ResolveNestedPointer(listPointer, 0xC);

            for (int i = 0; i < maxCnt; i++)
            {
                var ins = _client.Mem.ReadStruct<InventoryItemStruct>(pointer, i * 4, 0);

                if (excludeEmpty && ins.Id == 0)
                    continue;

                list.Add(_client.StructToObject<InventoryItem>(ins));
            }

            return list.ToArray();
        }

        public InventoryItem[] GetInventory()
        {
            int pointer = _client.Mem.ResolveNestedPointer(_client.Addresses.BaseAddress, Addresses.OffsetStruct, Addresses.OffsetPlayerStruct, 0xCAC);
            return ReadInventoryArray(pointer, false);
        }

        public InventoryItem[] GetEquipment()
        {
            int pointer = _client.Mem.ResolveNestedPointer(_client.Addresses.BaseAddress, Addresses.OffsetStruct, Addresses.OffsetPlayerStruct, 0xCB0);
            return ReadInventoryArray(pointer, false);
        }

        private Catshop ReadCatshop(int offsetSell, int offsetBuy = -1)
        {
            if (offsetBuy == -1)
                offsetBuy = offsetSell + 4;

            var catshop = new Catshop();

            int pointer = _client.Mem.ResolveNestedPointer(_client.Addresses.BaseAddress, Addresses.OffsetStruct, Addresses.OffsetPlayerStruct, offsetSell);
            catshop.SellList = ReadInventoryArray(pointer, true);
            catshop.SellList = ReadInventoryArray(pointer, true);
            catshop.SellList.ForEach(ii => ii.BuyPrice = 0);

            pointer = _client.Mem.ResolveNestedPointer(_client.Addresses.BaseAddress, Addresses.OffsetStruct, Addresses.OffsetPlayerStruct, offsetBuy);
            catshop.BuyList = ReadInventoryArray(pointer, true);
            catshop.BuyList.ForEach(ii => ii.BuyPrice = ii.SellPrice);
            catshop.BuyList.ForEach(ii => ii.SellPrice = 0);

            return catshop;
        }

        public Catshop GetOpenedCatshop()
        {
            return ReadCatshop(0xD00);
        }

        public Catshop GetMyCatshop()
        {
            return ReadCatshop(0xCF8);
        }

        public Skill[] GetSkills()
        {
            int countActive = _client.Mem.ReadInt(_client.Addresses.BaseAddress, Addresses.OffsetStruct, Addresses.OffsetPlayerStruct, 0x1080);
            int countPassive = _client.Mem.ReadInt(_client.Addresses.BaseAddress, Addresses.OffsetStruct, Addresses.OffsetPlayerStruct, 0x1098);
            int count = countActive;

            int pointer = _client.Mem.ResolveNestedPointer(_client.Addresses.BaseAddress, Addresses.OffsetStruct, Addresses.OffsetPlayerStruct, 0x107C);

            var list = new List<Skill>();

            for (int i = 0; i < count; i++)
            {
                int addr = _client.Mem.ReadInt(pointer, i * 4);

                var str = _client.Mem.ReadStruct<SkillStruct>(addr);
                var skill = _client.StructToObject<Skill>(str);
                skill.Pointer = addr;
                skill.ChiRequired = _client.Mem.ReadInt(addr + 0x4, 0x4, 0x36);
                list.Add(skill);
            }

            return list.ToArray();
        }

        public PwObject GetTarget()
        {
            uint id = _client.HostPlayer.TargetId;
            PwObject result =
                _client.Environment.GetMobs().FirstOrDefault(o => o.WorldId == id)
                ?? _client.Environment.GetNpcs().FirstOrDefault(o => o.WorldId == id)
                ?? _client.Environment.GetPets().FirstOrDefault(o => o.WorldId == id)
                ?? (PwObject)_client.Environment.GetPlayers().FirstOrDefault(o => o.WorldId == id);
            return result;
        }
    }
}
