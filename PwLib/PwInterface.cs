using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PwLib.Objects;

namespace PwLib
{
    public class PwInterface
    {
        private readonly PwClient _client;

        public PwInterface(PwClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Target of the host player.
        /// </summary>
        public PwObject Target { get { return _client.PlayerInfo.GetTarget(); } }

        /// <summary>
        /// Coords of the host player (Z is height).
        /// </summary>
        public Coords Coords { get { return _client.HostPlayer.Coords; } }

        /// <summary>
        /// HostPlayer object.
        /// </summary>
        public HostPlayer HostPlayer { get { return _client.HostPlayer; } }

        #region Loot
        /// <summary>
        /// Get nearest loot.
        /// </summary>
        /// <param name="ids">List of ids or empty/null for any loot.</param>
        /// <returns>Nearest loot or null if nothing found.</returns>
        public Loot GetLoot(params int[] ids)
        {
            return _client.Environment.GetLoot().FirstOrDefault(m => ids.Empty() || ids.Contains(m.Id));
        }

        /// <summary>
        /// Pick up specified loot.
        /// </summary>
        /// <param name="loot">Loot to pick up</param>
        /// <param name="autoMove">Move to loot automatically or not</param>
        public void PickUp(Loot loot, bool autoMove = true)
        {
            if (loot == null)
                return;

            uint wid = loot.WorldId;
            if (autoMove)
                _client.ActionStructs.PickUp(wid);
            else
                _client.PacketSender.PickUpItem(wid, loot.Id);

            int cnt = 0;
            while ((loot = _client.Environment.GetLoot().FirstOrDefault(l => l.WorldId == wid)) != null && loot.Distance > 5 && cnt++ < 300)
                Thread.Sleep(100);

            cnt = 0;
            while ((loot = _client.Environment.GetLoot().FirstOrDefault(l => l.WorldId == wid)) != null && cnt++ < 30)
                Thread.Sleep(100);
        }

        /// <summary>
        /// Pick up nearest loot.
        /// </summary>
        /// <param name="id">Id of loot to pick up (not world id), or 0 for any loot.</param>
        /// <param name="autoMove">Move to loot automatically or not</param>
        public void PickUp(int id = 0, bool autoMove = true)
        {
            var loot = GetLoot(id == 0 ? null : new[] { id });

            if (loot == null)
                return;

            PickUp(loot, autoMove);
        }

        /// <summary>
        /// Pick up all loot in radius maxDist from current position.
        /// </summary>
        /// <param name="maxDist">Maximum distance in meters.</param>
        public void PickUpAll(float maxDist = 10)
        {
            _client.Environment.GetLoot().Where(l => l.Distance <= maxDist).ForEach(l => PickUp(l));
        }
        #endregion

        #region Mine
        /// <summary>
        /// Get nearest mine.
        /// </summary>
        /// <param name="ids">List of ids or empty/null for any mine.</param>
        /// <returns>Nearest mine or null if nothing found.</returns>
        public Mine GetMine(params int[] ids)
        {
            return _client.Environment.GetMines().FirstOrDefault(m => ids.Empty() || ids.Contains(m.Id));
        }

        /// <summary>
        /// Gather specified mine.
        /// </summary>
        /// <param name="mine">Mine to gather</param>
        /// <param name="autoMove">Move to mine automatically or not</param>
        /// <returns>True if mine gathered, false if not</returns>
        public bool Gather(Mine mine, bool autoMove = true)
        {
            if (mine == null)
                return false;

            uint wid = mine.WorldId;
            if (autoMove)
                _client.ActionStructs.Gather(wid);
            else
                _client.PacketSender.HarvestResource(wid);

            int cnt = 0;
            while ((mine = _client.Environment.GetMines().FirstOrDefault(l => l.WorldId == wid)) != null && mine.Distance > 5 && cnt++ < 300)
                Thread.Sleep(100);

            if (mine == null)
                return false;

            Thread.Sleep(2000);
            if (!_client.HostPlayer.IsMining)
                return false;

            Thread.Sleep(_client.HostPlayer.TimeMining + 500);
            return true;
        }

        /// <summary>
        /// Gather nearest mine.
        /// </summary>
        /// <param name="id">Id of mine to gather or 0 to gather any mine</param>
        /// <param name="autoMove">Move to mine automatically or not</param>
        /// <returns>True if mine gathered, false if not</returns>
        public bool Gather(int id = 0, bool autoMove = true)
        {
            var mine = GetMine(id == 0 ? null : new[] { id });

            if (mine == null)
                return false;

            return Gather(mine, autoMove);
        }
        #endregion

        /// <summary>
        /// Attack host player target.
        /// </summary>
        /// <param name="skillId">Skill id to use or 0 for regular attack</param>
        /// <param name="autoMove">Move to target automatically or not</param>
        public void Attack(int skillId = 0, bool autoMove = true)
        {
            if (skillId == 0)
            {
                if (autoMove)
                    _client.ActionStructs.RegularAttack(_client.HostPlayer.TargetId);
                else
                    _client.PacketSender.RegularAttack(0);
            }
            else
            {
                if (autoMove)
                    _client.ActionStructs.UseSkill(skillId, _client.HostPlayer.TargetId);
                else
                    _client.PacketSender.UseSkill(skillId, _client.HostPlayer.TargetId);

                Thread.Sleep(300);
                while (_client.HostPlayer.IsCasting)
                    Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Use specified skill.
        /// </summary>
        /// <param name="skillId">Skill id to use</param>
        public void UseSkill(int skillId)
        {
            _client.PacketSender.UseSkill(skillId, _client.HostPlayer.WorldId);

            Thread.Sleep(300);
            while (_client.HostPlayer.IsCasting)
                Thread.Sleep(100);

            Thread.Sleep(500);
        }

        /// <summary>
        /// Get nearest mob.
        /// </summary>
        /// <param name="ids">List of ids or empty/null for any mob.</param>
        /// <returns>Nearest mob or null if nothing found.</returns>
        public Mob GetMob(params int[] ids)
        {
            return _client.Environment.GetMobs().Where(m => m.Action != MobAction.Death).FirstOrDefault(m => ids.Empty() || ids.Contains(m.Id));
        }

        public Mob[] GetMobs(params int[] ids)
        {
            return _client.Environment.GetMobs().Where(m => m.Action != MobAction.Death).Where(m => ids.Empty() || ids.Contains(m.Id)).ToArray();
        }

        /// <summary>
        /// Get nearest NPC.
        /// </summary>
        /// <param name="ids">List of ids or empty/null for any NPC.</param>
        /// <returns>Nearest NPC or null if nothing found.</returns>
        public Npc GetNpc(params int[] ids)
        {
            return _client.Environment.GetNpcs().FirstOrDefault(m => ids.Empty() || ids.Contains(m.Id));
        }

        /// <summary>
        /// Get nearest pet.
        /// </summary>
        /// <param name="ids">List of ids or empty/null for any pet.</param>
        /// <returns>Nearest pet or null if nothing found.</returns>
        public Pet GetPet(params int[] ids)
        {
            return _client.Environment.GetPets().FirstOrDefault(m => ids.Empty() || ids.Contains(m.Id));
        }

        /// <summary>
        /// Get nearest player.
        /// </summary>
        /// <param name="ids">List of ids or empty/null for any player.</param>
        /// <returns>Nearest player or null if nothing found.</returns>
        public Player GetPlayer(params int[] ids)
        {
            return _client.Environment.GetPlayers().FirstOrDefault(m => ids.Empty() || ids.Contains(m.Id));
        }

        /// <summary>
        /// Select object with specified world id.
        /// </summary>
        /// <param name="wid">World id of object to select</param>
        /// <returns>True if selection succeded, else false.</returns>
        public bool Select(uint wid)
        {
            _client.PacketSender.Select(wid);

            int cnt = 0;
            while (_client.HostPlayer.TargetId != wid && cnt++ < 30)
                Thread.Sleep(100);

            return _client.HostPlayer.TargetId == wid;
        }

        /// <summary>
        /// Select object.
        /// </summary>
        /// <param name="obj">Object to select</param>
        /// <returns>True if selection succeded, else false.</returns>
        public bool Select(PwObject obj)
        {
            if (obj == null)
                return false;

            return Select(obj.WorldId);
        }

        /// <summary>
        /// Deselect currently selected object (if any).
        /// </summary>
        public void Deselect()
        {
            _client.PacketSender.DeselectTarget();
        }

        /// <summary>
        /// Open dialog with NPC or catshop.
        /// </summary>
        /// <param name="wid">World id or 0 for use current target.</param>
        /// <returns>True if dialog was successfully opened, false if not possible to open.</returns>
        public bool OpenDialog(uint wid = 0)
        {
            if (wid == 0)
            {
                wid = _client.HostPlayer.TargetId;
            }
            else
            {
                if (!Select(wid))
                    return false;
            }

            _client.ActionStructs.TalkNpc(wid);

            int cnt = 0;
            while (_client.HostPlayer.DialogId != wid && cnt++ < 300)
                Thread.Sleep(100);

            return _client.HostPlayer.DialogId == wid;
        }

        /// <summary>
        /// Gets catshop of host player.
        /// </summary>
        /// <returns>Catshop of host player.</returns>
        public Catshop GetMyCatshop()
        {
            return _client.PlayerInfo.GetMyCatshop();
        }

        /// <summary>
        /// Gets catshop opened now.
        /// </summary>
        /// <returns>Currently opened catshop.</returns>
        public Catshop GetOpenedCatshop()
        {
            return _client.PlayerInfo.GetOpenedCatshop();
        }

        /// <summary>
        /// Get list of all nearby players.
        /// </summary>
        /// <returns>List containing Player objects for every nearby player.</returns>
        public Player[] GetPlayers()
        {
            return _client.Environment.GetPlayers();
        }

        /// <summary>
        /// Moves to specified coordinates and waits for moving to complete (or for sticking in textures).
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="fly">Height to fly at or -1 if no fly is needed.</param>
        /// <returns>True if moving completed successfully, false if sticked in textures and isn't able to reach end point.</returns>
        public bool MoveTo(float x, float y, float fly = -1)
        {
            const float accuracy = 0.5f; // in meters

            var coords = new Coords(x, y, -1, true);

            _client.ActionStructs.MoveTo(coords, fly);
            var lastCoords = _client.HostPlayer.Coords;
            int cntSameCoords = 0;
            while (_client.HostPlayer.Coords.DistancePlanar(coords) > accuracy && cntSameCoords < 10)
            {
                Thread.Sleep(200);
                _client.ActionStructs.MoveTo(coords, fly);

                if (lastCoords.DistancePlanar(_client.HostPlayer.Coords) <= 0.5)
                {
                    cntSameCoords++;
                    Jump();
                }
                else
                {
                    cntSameCoords = 0;
                }
                lastCoords = _client.HostPlayer.Coords;
            }

            return _client.HostPlayer.Coords.DistancePlanar(coords) <= accuracy;
        }

        /// <summary>
        /// Moves to specified coordinates and waits for moving to complete (or for sticking in textures).
        /// </summary>
        /// <param name="coords">Coordinates for moving to: tuple containing X and Y.</param>
        /// <param name="fly">Height to fly at or -1 if no fly is needed.</param>
        /// <returns>True if moving completed successfully, false if sticked in textures and isn't able to reach end point.</returns>
        public bool MoveTo(float[] coords, float fly = -1)
        {
            return MoveTo(coords[0], coords[1], fly);
        }

        /// <summary>
        /// Move by specified path.
        /// </summary>
        /// <param name="coords">Tuples (X,Y) specifying sequential coordinates of path.</param>
        /// <returns>True if moving by path completed successfully, false if sticked in textures and isn't able to reach end point.</returns>
        public bool MovePath(params float[][] coords)
        {
            foreach (float[] c in coords)
            {
                bool ok = MoveTo(c);
                if (!ok) return false;
            }
            return true;
        }

        /// <summary>
        /// Waits for any teleportation.
        /// </summary>
        /// <returns>True if teleported, false if not and timeout ended.</returns>
        public bool WaitTeleport()
        {
            Coords lastCoords = _client.HostPlayer.Coords;
            Coords curCoords;
            int cntSame = 0;
            while ((curCoords = _client.HostPlayer.Coords).Distance(lastCoords) <= 10 && cntSame++ < 100)
            {
                lastCoords = curCoords;
                Thread.Sleep(100);
            }
            return cntSame < 100;
        }

        /// <summary>
        /// Swaps specififed items in equip and inventory.
        /// </summary>
        /// <param name="equipInd">Index of item in equip.</param>
        /// <param name="invInd">Index of item in inventory.</param>
        public void SwapEquipInv(int equipInd, int invInd)
        {
            _client.PacketSender.SwapEquipWithInv((byte)invInd, (byte)equipInd);
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Get host player inventory.
        /// </summary>
        /// <returns>Inventory as array of InventoryItems.</returns>
        public InventoryItem[] GetInventory()
        {
            return _client.PlayerInfo.GetInventory();
        }

        /// <summary>
        /// Get item of host player inventory.
        /// </summary>
        /// <param name="index">Index of item in inventory.</param>
        /// <returns>InventoryItem which is at specified index.</returns>
        public InventoryItem GetInventory(int index)
        {
            return GetInventory()[index];
        }

        /// <summary>
        /// Get host player equip.
        /// </summary>
        /// <returns>Equip as array of InventoryItems.</returns>
        public InventoryItem[] GetEquip()
        {
            return _client.PlayerInfo.GetEquipment();
        }

        /// <summary>
        /// Get item of host player equip.
        /// </summary>
        /// <param name="index">Index of item in equip.</param>
        /// <returns>InventoryItem which is at specified index.</returns>
        public InventoryItem GetEquip(int index)
        {
            return GetEquip()[index];
        }

        /// <summary>
        /// Kill specified mob.
        /// </summary>
        /// <param name="mob">Mob to kill or null to use current target.</param>
        /// <param name="spark">Whether to use 3 chi spark or not.</param>
        public void KillMob(Mob mob = null, bool spark = false)
        {
            uint wid;
            if (mob == null)
            {
                wid = _client.HostPlayer.TargetId;
            }
            else
            {
                wid = mob.WorldId;
                Select(wid);
            }

            int sparkId = 0;
            if (spark)
                sparkId = _client.PlayerInfo.GetSkills().First(s => s.ChiRequired == 300).Id;

            Func<bool> isUseChi = () =>
            {
                var m = _client.PlayerInfo.GetTarget() as Mob;
                if (m == null)
                    return false;
                else
                    return m.Hp > 80000 && m.Distance < 5;
            };

            _client.ActionStructs.RegularAttack(wid);
            while (_client.HostPlayer.TargetId == wid)
            {
                _client.PacketSender.RegularAttack(0);
                if (spark && _client.HostPlayer.Chi >= 300 && isUseChi())
                {
                    _client.PacketSender.UseSkill(sparkId, 0);
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Kill specified mob using 3 chi spark.
        /// </summary>
        /// <param name="mob">Mob to kill or null to use current target.</param>
        public void KillMobSpark(Mob mob = null)
        {
            KillMob(mob, true);
        }

        /// <summary>
        /// Sends key to client window.
        /// </summary>
        /// <param name="key">Key to send.</param>
        public void SendKey(string key)
        {
            _client.Keyboard.SendKey(key);
        }

        /// <summary>
        /// Jump - is performed by Space key press.
        /// </summary>
        public void Jump()
        {
            _client.Keyboard.SendKey("space");
        }
    }
}