using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PwLib
{
    public class PacketSender
    {
        public struct Packet
        {
            public byte[] Data { get; private set; }
            private int ParameterBytes { get; set; }
            private int ParameterStart { get { return Data.Length - ParameterBytes; } }

            public Packet(int parameterBytes, params byte[] data)
                : this()
            {
                var dlist = data.ToList();
                dlist.AddRange(Enumerable.Repeat((byte)0x00, parameterBytes));
                Data = dlist.ToArray();
                ParameterBytes = parameterBytes;
            }

            public Packet SetParameter(params byte[] parameter)
            {
                if (parameter.Length != ParameterBytes)
                    throw new ArgumentOutOfRangeException();

                var newPacket = new Packet(ParameterBytes, Data.Take(ParameterStart).ToArray());
                parameter.CopyTo(newPacket.Data, ParameterStart);

                return newPacket;
            }

            public Packet SetParameter(params IEnumerable<byte>[] parameter)
            {
                if (parameter.Sum(p => p.Count()) != ParameterBytes)
                    throw new ArgumentOutOfRangeException();

                var newPacket = new Packet(ParameterBytes, Data.Take(ParameterStart).ToArray());
                int pStart = ParameterStart;
                parameter.ForEach(p =>
                {
                    p.ToArray().CopyTo(newPacket.Data, pStart);
                    pStart += p.Count();
                });

                return newPacket;
            }
        }

        private static class Packets
        {
            public static readonly Packet LogOut = new Packet(4, 0x01, 0x00);
            public static readonly Packet Select = new Packet(4, 0x02, 0x00);
            public static readonly Packet RegularAttack = new Packet(1, 0x03, 0x00);
            public static readonly Packet ResurrectToTown = new Packet(0, 0x04, 0x00);
            public static readonly Packet ResurrectWithScroll = new Packet(0, 0x05, 0x00);
            public static readonly Packet PickUpItem = new Packet(8, 0x06, 0x00);
            public static readonly Packet UpdateInvPosition = new Packet(1, 0x09, 0x00);
            public static readonly Packet SwapItemInInv = new Packet(2, 0x0C, 0x00);
            public static readonly Packet SplitStackItemInInv = new Packet(4, 0x0D, 0x00);
            public static readonly Packet DropItem = new Packet(3, 0x0E, 0x00);
            public static readonly Packet SwapItemInEquip = new Packet(2, 0x10, 0x00);
            public static readonly Packet SwapEquipWithInv = new Packet(2, 0x11, 0x00);
            public static readonly Packet DropGold = new Packet(4, 0x14, 0x00);
            public static readonly Packet UpdateStats = new Packet(0, 0x15, 0x00);
            public static readonly Packet IncreaseStatsBy = new Packet(16, 0x16, 0x00);
            public static readonly Packet InviteParty = new Packet(4, 0x1B, 0x00);
            public static readonly Packet AcceptPartyInvite = new Packet(8, 0x1C, 0x00);
            public static readonly Packet RefusePartyInvite = new Packet(4, 0x1D, 0x00);
            public static readonly Packet LeaveParty = new Packet(0, 0x1E, 0x00);
            public static readonly Packet EvictFromParty = new Packet(4, 0x1F, 0x00);
            public static readonly Packet StartNpcDialogue = new Packet(4, 0x23, 0x00);
            public static readonly Packet UseItem = new Packet(8, 0x28, 0x00);
            public static readonly Packet UseSkill = new Packet(10, 0x29, 0x00);
            public static readonly Packet CancelAction = new Packet(0, 0x2A, 0x00);
            public static readonly Packet StartMeditating = new Packet(0, 0x2E, 0x00);
            public static readonly Packet StopMeditating = new Packet(0, 0x2F, 0x00);
            public static readonly Packet UseEmotion = new Packet(2, 0x30, 0x00);
            public static readonly Packet BeIntimate = new Packet(2, 0x30, 0x00, 0x1D, 0x00);
            public static readonly Packet HarvestResource = new Packet(16, 0x36, 0x00);
            public static readonly Packet SwapItemInBank = new Packet(2, 0x38, 0x00, 0x03);
            public static readonly Packet SplitStackItemInBank = new Packet(4, 0x39, 0x00, 0x03);
            public static readonly Packet SwapItemBankAndInv = new Packet(2, 0x3A, 0x00, 0x03);
            public static readonly Packet SplitStackItemInBankToInv = new Packet(4, 0x3B, 0x00, 0x00);
            public static readonly Packet SplitStackItemInInvToBank = new Packet(4, 0x3C, 0x00, 0x00);
            public static readonly Packet SetPartySearchSettings = new Packet(8, 0x3F, 0x00);
            public static readonly Packet ShiftPartyCaptain = new Packet(4, 0x48, 0x00);
            public static readonly Packet UseSkillWithoutCastTime = new Packet(10, 0x50, 0x00);
            public static readonly Packet InitiateSettingUpCatShop = new Packet(0, 0x54, 0x00);
            public static readonly Packet ToggleFashionDisplay = new Packet(0, 0x55, 0x00);
            public static readonly Packet AcceptRessurectByCleric = new Packet(0, 0x57, 0x00);
            public static readonly Packet IncreaseFlySpeed = new Packet(0, 0x5A, 0x00);
            public static readonly Packet InviteToDuel = new Packet(4, 0x5C, 0x00);
            public static readonly Packet AcceptDuel = new Packet(4, 0x5D, 0x00);
            public static readonly Packet AskMaleToCarry = new Packet(4, 0x5E, 0x00);
            public static readonly Packet AskFemaleToBeCarried = new Packet(4, 0x5F, 0x00);
            public static readonly Packet AcceptRequestByFemaleToBeCarried = new Packet(8, 0x60, 0x00);
            public static readonly Packet AcceptRequestByMaleToCarryYou = new Packet(8, 0x61, 0x00);
            public static readonly Packet ReleaseCarryMode = new Packet(0, 0x62, 0x00);
            public static readonly Packet ViewPlayerEquip = new Packet(4, 0x63, 0x00);
            public static readonly Packet SummonPet = new Packet(4, 0x64, 0x00);
            public static readonly Packet RecallPet = new Packet(0, 0x65, 0x00);
            public static readonly Packet SetPetMode = new Packet(4, 0x67, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00);
            public static readonly Packet SetPetFollow = new Packet(0, 0x67, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);// LOOK AT SOURCE
            public static readonly Packet SetPetStop = new Packet(0, 0x67, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00);
            public static readonly Packet SetPetAttack = new Packet(4, 0x67, 0x00);
            public static readonly Packet SetPetUseSkill = new Packet(13, 0x67, 0x00);
            public static readonly Packet SetPetStandardSkill = new Packet(4, 0x67, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00);
            public static readonly Packet UseGenieSkill = new Packet(8, 0x74, 0x00);
            public static readonly Packet FeedEquippedGenie = new Packet(5, 0x75, 0x00);
            public static readonly Packet AcceptQuest = new Packet(4, 0x25, 0x00, 0x07, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00);
            public static readonly Packet HandInQuest = new Packet(8, 0x25, 0x00, 0x06, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00);
            public static readonly Packet SellSingleItem = new Packet(12, 0x25, 0x00, 0x02, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00);
            public static readonly Packet BuySingleItem = new Packet(12, 0x25, 0x00, 0x01, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00);
            public static readonly Packet RepairAll = new Packet(0, 0x25, 0x00, 0x03, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00);
            public static readonly Packet RepairItem = new Packet(6, 0x25, 0x00, 0x03, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00);
            public static readonly Packet UpgradeSkill = new Packet(4, 0x25, 0x00, 0x09, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00);
            public static readonly Packet DeselectTarget = new Packet(0, 0x08, 0x00);
        }

        private readonly PwClient _client;
        // memory addresses
        private int _packetAddressLocation;
        private int _packetSizeAddress;
        private int _sendPacketOpcodeAddress;

        //opcode for sending a packet
        private readonly byte[] _sendPacketOpcode = new byte[] 
        { 
            0x60,                                   // PUSHAD
            0xB8, 0x00, 0x00, 0x00, 0x00,           // MOV EAX, SendPacketAddress
            0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00,     // MOV ECX, DWORD PTR [realBaseAddress]
            0x8B, 0x49, 0x20,                       // MOV ECX, DWORD PTR [ECX+20]
            0xBF, 0x00, 0x00, 0x00, 0x00,           // MOV EDI, packetAddress
            0x6A, 0x00,                             // PUSH packetSize
            0x57,                                   // PUSH EDI
            0xFF, 0xD0,                             // CALL EAX
            0x61,                                   // POPAD
            0xC3                                    // RET
        };

        public PacketSender(PwClient client)
        {
            _client = client;
        }

        private void SendPacket(Packet packet, params byte[] parameters)
        {
            SendPacket(packet.SetParameter(parameters));
        }

        private void SendPacket(Packet packet, params IEnumerable<byte>[] parameters)
        {
            SendPacket(packet.SetParameter(parameters));
        }

        public void SendPacket(Packet packet)
        {
            // Выделяем место под пакет, который мы будем посылать
            int packetAddress = _client.Mem.AllocateMemory(packet.Data.Length);
            // Записываем пакет
            _client.Mem.WriteBytes(packet.Data, packetAddress);

            // Переводим адрес, куда мы записали пакет в массив байт
            var packetLocation = BitConverter.GetBytes(packetAddress);

            // Если код не загружен ранее, загружаем его в память
            if (_sendPacketOpcodeAddress == 0)
                LoadSendPacketOpcode();

            // Записываем адрес, где лежит пакет в тело нашего инжекта
            _client.Mem.WriteBytes(packetLocation, _packetAddressLocation);
            // Записываем длину пакета
            _client.Mem.WriteBytes(new[] { (byte)packet.Data.Length }, _packetSizeAddress);

            // Запускаем инжект
            IntPtr threadHandle = _client.Mem.CreateRemoteThread(_sendPacketOpcodeAddress);

            // Ждем окончания выполнения потока
            _client.Mem.WaitForSingleObject(threadHandle);
            // Уничтожаем поток
            WinApi.CloseHandle(threadHandle);

            // Освобождаем память, выделенную под пакет
            _client.Mem.FreeMemory(packetAddress, packet.Data.Length);
            // Освобождаем память, выделенную под код отправки пакета
            _client.Mem.FreeMemory(_sendPacketOpcodeAddress, _sendPacketOpcode.Length);
        }

        private void LoadSendPacketOpcode()
        {
            // Выделяем память под код отправки пакета
            _sendPacketOpcodeAddress = _client.Mem.AllocateMemory(_sendPacketOpcode.Length);

            // Записываем код отправки пакета
            _client.Mem.WriteBytes(_sendPacketOpcode, _sendPacketOpcodeAddress);

            // Переводим адрес функции отправки пакетов в массив байт
            var functionAddress = BitConverter.GetBytes(_client.Addresses.SendPacketFunction);
            // Переводим базовый адрес в массив байт
            var realBaseAddress = BitConverter.GetBytes(_client.Addresses.BaseAddress);

            // Записываем адрес функции отправки пакетов в тело нашего инжекта
            _client.Mem.WriteBytes(functionAddress, _sendPacketOpcodeAddress + 2);
            // Записываем базовый адрес в тело нашего инжекта
            _client.Mem.WriteBytes(realBaseAddress, _sendPacketOpcodeAddress + 8);

            // Указываем адрес, куда будет записан адрес загруженного пакета 
            _packetAddressLocation = _sendPacketOpcodeAddress + 16;
            // Указываем адрес, куда будет записан размер загруженного пакета
            _packetSizeAddress = _sendPacketOpcodeAddress + 21;
        }

        #region Functions to send packets
        public void LogOut(int toAccount)
        {
            SendPacket(Packets.LogOut, BitConverter.GetBytes(toAccount));
        }

        public void Select(uint objectId)
        {
            SendPacket(Packets.Select, BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(objectId), 0)));
        }

        public void RegularAttack(byte afterSkill)
        {
            SendPacket(Packets.RegularAttack, afterSkill);
        }

        public void ResurrectToTown()
        {
            SendPacket(Packets.ResurrectToTown);
        }

        public void ResurrectWithScroll()
        {
            SendPacket(Packets.ResurrectWithScroll);
        }

        public void PickUpItem(uint uniqueId, int typeId)
        {
            SendPacket(Packets.PickUpItem, BitConverter.GetBytes(uniqueId), BitConverter.GetBytes(typeId));
        }

        public void UpdateInvPosition(byte invIndex)
        {
            SendPacket(Packets.UpdateInvPosition, invIndex);
        }

        public void SwapItemInInv(byte invIndex1, byte invIndex2)
        {
            SendPacket(Packets.SwapItemInInv, invIndex1, invIndex2);
        }

        public void SplitStackInInventory(byte invIndexSource, byte invIndexDestination, short amount)
        {
            SendPacket(Packets.SplitStackItemInInv, BitConverter.GetBytes(amount).Prepend(invIndexDestination).Prepend(invIndexSource));
        }

        public void DropItem(byte invIndex, short amount)
        {
            SendPacket(Packets.DropItem, BitConverter.GetBytes(amount).Prepend(invIndex));
        }

        public void SwapItemInEquip(byte equipIndex1, byte equipIndex2)
        {
            SendPacket(Packets.SwapItemInEquip, equipIndex1, equipIndex2);
        }

        public void SwapEquipWithInv(byte invIndex, byte equipIndex)
        {
            SendPacket(Packets.SwapEquipWithInv, invIndex, equipIndex);
        }

        public void DropGold(int amount)
        {
            SendPacket(Packets.DropGold, BitConverter.GetBytes(amount));
        }

        public void UpdateStats()
        {
            SendPacket(Packets.UpdateStats);
        }

        public void IncreaseStatsBy(int con, int @int, int str, int agi)
        {
            SendPacket(Packets.IncreaseStatsBy, BitConverter.GetBytes(con), BitConverter.GetBytes(@int), BitConverter.GetBytes(str), BitConverter.GetBytes(agi));
        }

        public void InviteParty(int playerId)
        {
            SendPacket(Packets.InviteParty, BitConverter.GetBytes(playerId));
        }

        public void AcceptPartyInvite(int playerId, int partyInviteCounter)
        {
            SendPacket(Packets.AcceptPartyInvite, BitConverter.GetBytes(playerId), BitConverter.GetBytes(partyInviteCounter));
        }

        public void RefusePartyInvite(int playerId)
        {
            SendPacket(Packets.RefusePartyInvite, BitConverter.GetBytes(playerId));
        }

        public void LeaveParty()
        {
            SendPacket(Packets.LeaveParty);
        }

        public void EvictFromParty(int playerId)
        {
            SendPacket(Packets.EvictFromParty, BitConverter.GetBytes(playerId));
        }

        public void StartNpcDialogue(int npcId)
        {
            SendPacket(Packets.StartNpcDialogue, BitConverter.GetBytes(npcId));
        }

        public void UseItem(byte isEquip, byte itemIndex, int typeId)
        {
            SendPacket(Packets.UseItem, BitConverter.GetBytes(typeId).Prepend((byte)0x00).Prepend(itemIndex).Prepend((byte)0x01).Prepend(isEquip));
        }

        public void UseSkill(int skillId, uint targetId)
        {
            SendPacket(Packets.UseSkill, BitConverter.GetBytes(skillId).Append((byte)0x00, (byte)0x01).Append(BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(targetId), 0))));
        }

        public void CancelAction()
        {
            SendPacket(Packets.CancelAction);
        }

        public void StartMeditating()
        {
            SendPacket(Packets.StartMeditating);
        }

        public void StopMeditating()
        {
            SendPacket(Packets.StopMeditating);
        }

        public void UseEmotion(short emoteId)
        {
            SendPacket(Packets.UseEmotion, BitConverter.GetBytes(emoteId));
        }

        public void BeIntimate()
        {
            SendPacket(Packets.BeIntimate);
        }

        public void HarvestResource(uint uniqueId)
        {
            SendPacket(Packets.HarvestResource, BitConverter.GetBytes(uniqueId).Append((byte)0x00, (byte)0x00, (byte)0x1E, (byte)0x00).Append((byte)0x01, (byte)0x0C, (byte)0x00, (byte)0x00).Append((byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00));
        }

        public void SwapItemInBank(byte bankIndex1, byte bankIndex2)
        {
            SendPacket(Packets.SwapItemInBank, bankIndex1, bankIndex2);
        }

        public void SplitStackItemInBank(byte bankIndexSource, byte bankIndexDestination, short amount)
        {
            SendPacket(Packets.SplitStackItemInBank, BitConverter.GetBytes(amount).Prepend(bankIndexDestination, bankIndexSource));
        }

        public void SwapItemBankAndInv(byte bankIndex, byte invIndex)
        {
            SendPacket(Packets.SwapItemBankAndInv, bankIndex, invIndex);
        }

        public void SplitStackItemInBankToInv(byte bankIndex, byte invIndex, short amount)
        {
            SendPacket(Packets.SplitStackItemInBankToInv, BitConverter.GetBytes(amount).Prepend(invIndex, bankIndex));
        }

        public void SplitStackItemInInvToBank(byte invIndex, byte bankIndex, short amount)
        {
            SendPacket(Packets.SplitStackItemInInvToBank, BitConverter.GetBytes(amount).Prepend(bankIndex, invIndex));
        }

        public void SetPartySearchSettings(byte jobId, byte lvl, byte recruit, byte slogan)
        {
            SendPacket(Packets.SetPartySearchSettings, jobId, lvl, recruit, slogan, 0x00, 0x00, 0x00, 0x00);
        }

        public void ShiftPartyCaptain(int playerId)
        {
            SendPacket(Packets.ShiftPartyCaptain, BitConverter.GetBytes(playerId));
        }

        public void UseSkillWithoutCastTime(int skillId, uint targetId)
        {
            SendPacket(Packets.UseSkillWithoutCastTime, BitConverter.GetBytes(skillId).Append((byte)0x00, (byte)0x01).Append(BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(targetId), 0))));
        }

        public void InitiateSettingUpCatShop()
        {
            SendPacket(Packets.InitiateSettingUpCatShop);
        }

        public void ToggleFashionDisplay()
        {
            SendPacket(Packets.ToggleFashionDisplay);
        }

        public void AcceptRessurectByCleric()
        {
            SendPacket(Packets.AcceptRessurectByCleric);
        }

        public void IncreaseFlySpeed()
        {
            SendPacket(Packets.IncreaseFlySpeed);
        }

        public void InviteToDuel(int playerId)
        {
            SendPacket(Packets.InviteToDuel, BitConverter.GetBytes(playerId));
        }

        public void AcceptDuel(int playerId)
        {
            SendPacket(Packets.AcceptDuel, BitConverter.GetBytes(playerId));
        }

        public void AskMaleToCarry(int playerId)
        {
            SendPacket(Packets.AskMaleToCarry, BitConverter.GetBytes(playerId));
        }

        public void AskFemaleToBeCarried(int playerId)
        {
            SendPacket(Packets.AskFemaleToBeCarried, BitConverter.GetBytes(playerId));
        }

        public void AcceptRequestByFemaleToBeCarried(int playerId)
        {
            SendPacket(Packets.AcceptRequestByFemaleToBeCarried, BitConverter.GetBytes(playerId).Append(Enumerable.Repeat((byte)0x00, 4)));
        }

        public void AcceptRequestByMaleToCarryYou(int playerId)
        {
            SendPacket(Packets.AcceptRequestByMaleToCarryYou, BitConverter.GetBytes(playerId).Append(Enumerable.Repeat((byte)0x00, 4)));
        }

        public void ReleaseCarryMode()
        {
            SendPacket(Packets.ReleaseCarryMode);
        }

        public void ViewPlayerEquip(int playerId)
        {
            SendPacket(Packets.ViewPlayerEquip, BitConverter.GetBytes(playerId));
        }

        public void SummonPet(int petIndex)
        {
            SendPacket(Packets.SummonPet, BitConverter.GetBytes(petIndex));
        }

        public void RecallPet()
        {
            SendPacket(Packets.RecallPet);
        }

        public void SetPetMode(int petMode)
        {
            SendPacket(Packets.SetPetMode, BitConverter.GetBytes(petMode));
        }

        public void SetPetFollow()
        {
            SendPacket(Packets.SetPetFollow);
        }

        public void SetPetStop()
        {
            SendPacket(Packets.SetPetStop);
        }

        public void SetPetAttack(int targetId)
        {
            SendPacket(Packets.SetPetAttack, BitConverter.GetBytes(targetId).Append((byte)0x01).Append(Enumerable.Repeat((byte)0x00, 4)));
        }

        public void SetPetUseSkill(int targetId, int skillId)
        {
            SendPacket(Packets.SetPetUseSkill, BitConverter.GetBytes(targetId).Append((byte)0x04, (byte)0x00, (byte)0x00, (byte)0x00).Append(BitConverter.GetBytes(skillId)).Append((byte)0x00));
        }

        public void SetPetStandardSkill(int skillId)
        {
            SendPacket(Packets.SetPetStandardSkill, BitConverter.GetBytes(skillId));
        }

        public void UseGenieSkill(short skillId, uint targetId)
        {
            SendPacket(Packets.UseGenieSkill, BitConverter.GetBytes(skillId).Append((byte)0x00, (byte)0x01).Append(BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(targetId), 0))));
        }

        public void FeedEquippedGenie(byte invIndex, int amount)
        {
            SendPacket(Packets.FeedEquippedGenie, BitConverter.GetBytes(invIndex), BitConverter.GetBytes(amount));
        }

        public void AcceptQuest(int questId)
        {
            SendPacket(Packets.AcceptQuest, BitConverter.GetBytes(questId));
        }

        public void HandInQuest(int questId, int optionIndex)
        {
            SendPacket(Packets.HandInQuest, BitConverter.GetBytes(questId), BitConverter.GetBytes(optionIndex));
        }

        public void SellSingleItem(int typeId, int invIndex, int amount)
        {
            SendPacket(Packets.SellSingleItem, BitConverter.GetBytes(typeId), BitConverter.GetBytes(invIndex), BitConverter.GetBytes(amount));
        }

        public void BuySingleItem(int typeId, int shopIndex, int amount)
        {
            SendPacket(Packets.BuySingleItem, BitConverter.GetBytes(typeId), BitConverter.GetBytes(shopIndex), BitConverter.GetBytes(amount));
        }

        public void RepairAll()
        {
            SendPacket(Packets.RepairAll);
        }

        public void RepairItem(int typeId, byte isEquipped, byte locationIndex)
        {
            SendPacket(Packets.RepairItem, BitConverter.GetBytes(typeId).Append(isEquipped).Append(locationIndex));
        }

        public void UpgradeSkill(int skillId)
        {
            SendPacket(Packets.UpgradeSkill, BitConverter.GetBytes(skillId));
        }

        public void DeselectTarget()
        {
            SendPacket(Packets.DeselectTarget);
        }
    }
        #endregion
}