using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PwLib
{
    public class ActionStructs
    {
        private readonly PwClient _client;

        public ActionStructs(PwClient client)
        {
            _client = client;
        }

        public void RegularAttack(uint worldId)
        {
            InteractWith(BitConverter.ToInt32(BitConverter.GetBytes(worldId), 0), 0, 0);
        }

        public void UseSkill(int skillId, uint worldId)
        {
            int skillPointer = _client.PlayerInfo.GetSkills().Single(s => s.Id == skillId).Pointer;
            InteractWith(BitConverter.ToInt32(BitConverter.GetBytes(worldId), 0), 3, skillPointer);
        }

        public void PickUp(uint worldId)
        {
            InteractWith(BitConverter.ToInt32(BitConverter.GetBytes(worldId), 0), 1, 0);
        }

        public void TalkNpc(uint worldId)
        {
            InteractWith(BitConverter.ToInt32(BitConverter.GetBytes(worldId), 0), 2, 0);
        }

        public void Gather(uint worldId)
        {
            InteractWith(BitConverter.ToInt32(BitConverter.GetBytes(worldId), 0), 4, 0);
        }

        private void InteractWith(int objectId, int interactionType, int skillPointer)
        {
            int actionStruct = _client.Mem.ReadInt(_client.Addresses.BaseAddress, Addresses.OffsetStruct, Addresses.OffsetPlayerStruct, Addresses.OffsetActionStruct);
            int interactWithAction = _client.Mem.ReadInt(actionStruct + 0x30, 0x8);

            _client.Mem.WriteInt(0, interactWithAction + 0x8);  //action finished
            _client.Mem.WriteInt(1, interactWithAction + 0x14); //Action start
            _client.Mem.WriteInt(0, interactWithAction + 0x24); // Action not start
            _client.Mem.WriteInt(objectId, interactWithAction + 0x20); // Set object id to interact with
            _client.Mem.WriteInt(interactionType, interactWithAction + 0x38); // Set the type of interaction, 0 = regAtk, 1 = pick item, 2 = talk to NPC,3 = useSkill, 4 = gatherResources
            _client.Mem.WriteInt(0, interactWithAction + 0x34); // Set error
            _client.Mem.WriteInt(skillPointer, interactWithAction + 0x50); // Set skillPointer
            _client.Mem.WriteInt(interactWithAction, actionStruct + 0xC); // Set new actionType ?
            _client.Mem.WriteInt(1, actionStruct + 0x18); // Set next action position to 1
            _client.Mem.WriteInt(interactWithAction, actionStruct + 0x14); // Set new actionType ?
        }

        public void Follow(uint playerId)
        {
            int actionStruct = _client.Mem.ReadInt(_client.Addresses.BaseAddress, Addresses.OffsetStruct, Addresses.OffsetPlayerStruct, Addresses.OffsetActionStruct);
            int followAction = _client.Mem.ReadInt(actionStruct + 0x30, 0x1C);

            _client.Mem.WriteInt(0, followAction + 0x8);          //Set error = 0
            _client.Mem.WriteUInt(playerId, followAction + 0x20);  //Set playerId to follow

            _client.Mem.WriteInt(followAction, actionStruct + 0xC);   //Set new action at position 1
            _client.Mem.WriteInt(1, actionStruct + 0x18);             //Set next action position to 1
            _client.Mem.WriteInt(followAction, actionStruct + 0x14);  //Set new action type follow as next action
        }

        public void MoveTo(float x, float y, float z, float height = -1)
        {
            int actionStruct = _client.Mem.ReadInt(_client.Addresses.BaseAddress, Addresses.OffsetStruct, Addresses.OffsetPlayerStruct, Addresses.OffsetActionStruct);
            int moveAction = _client.Mem.ReadInt(actionStruct + 0x30, 0x4);

            _client.Mem.WriteInt(0, moveAction + 0x8);  //action finished = 0
            _client.Mem.WriteInt(1, moveAction + 0x14); //Action start = 1
            _client.Mem.WriteFloat(x, moveAction + 0x20); // Set X coord
            _client.Mem.WriteFloat(z, moveAction + 0x24); // Set Y coord
            _client.Mem.WriteFloat(y, moveAction + 0x28); // Set Z coord
            _client.Mem.WriteFloat(height, moveAction + 0x68); // Set height
            if (height >= 0.0)
            {
                _client.Mem.WriteInt(26625, moveAction + 0x64); //Set 1st var for flying up
                _client.Mem.WriteInt(256, moveAction + 0x6C); // Set 2nd var for flying up
            }
            else
            {
                _client.Mem.WriteInt(26624, moveAction + 0x64); //Set 1st var for not flying up
                _client.Mem.WriteInt(65536, moveAction + 0x6C); // Set 2nd var for not flying up
            }

            _client.Mem.WriteInt(0, moveAction + 0x2C); // Set moveType
            _client.Mem.WriteInt(moveAction, actionStruct + 0xC); // Set new moveAction
            _client.Mem.WriteInt(1, actionStruct + 0x18); // Set next action position to 1
            _client.Mem.WriteInt(moveAction, actionStruct + 0x14); // Set new moveAction
        }

        public void MoveTo(Coords coords, float height = -1)
        {
            MoveTo(coords.X, coords.Y, coords.Z, height);
        }
    }
}
