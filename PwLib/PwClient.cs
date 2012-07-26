using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using PwLib.Objects;
using PwLib.Structs;

namespace PwLib
{
    public class PwClient : IDisposable
    {
        public Process Process { get; private set; }
        public IntPtr Handle { get; private set; }

        public Addresses Addresses { get; private set; }

        public Mem Mem { get; private set; }

        public PlayerInfo PlayerInfo { get; private set; }
        public Environment Environment { get; private set; }
        public PacketSender PacketSender { get; private set; }
        public ActionStructs ActionStructs { get; private set; }
        public Additional Additional { get; private set; }
        public Chat Chat { get; set; }
        public Keyboard Keyboard { get; set; }

        public PwInterface Interface { get; private set; }

        public HostPlayer HostPlayer
        {
            get
            {
                var playerStruct = Mem.ReadStruct<PlayerStruct>(Addresses.BaseAddress, Addresses.OffsetStruct, Addresses.OffsetPlayerStruct, 0);

                var player = StructToObject<HostPlayer>(playerStruct);
                player.IsMining = !player.IsMining;
                return player;
            }
        }

        public bool IsConnected { get { return Mem.ReadBool(0xB05938); } }

        public PwClient(Process process)
        {
            Process = process;
            Handle = process.Handle;

            Addresses = new Addresses(this);

            Mem = new Mem(this);

            PlayerInfo = new PlayerInfo(this);
            Environment = new Environment(this);
            PacketSender = new PacketSender(this);
            ActionStructs = new ActionStructs(this);
            Additional = new Additional(this);
            Chat = new Chat(this);
            Keyboard = new Keyboard(this);

            Interface = new PwInterface(this);
        }

        public static PwClient[] GetClients()
        {
            return Process
                .GetProcessesByName("elementclient")
                .Select(proc => new PwClient(proc))
                .ToArray();
        }

        public int GetFps()
        {
            return Mem.ReadInt(0xB01A84);
        }

        public int GetPing()
        {
            return Mem.ReadInt(Addresses.BaseAddress, 0x1C, 104);
        }

        public void Unfreeze()
        {
            int addr = Mem.ResolveNestedPointer(Addresses.BaseAddress, Addresses.OffsetUnfreeze);
            Mem.WriteInt(1, addr);
        }

        private byte[] _unfreezeFuncBackup = new byte[] { 0x88, 0x85, 0x8C, 0x04, 0x00, 0x00 };

        public void UnfreezePermanent()
        {
            _unfreezeFuncBackup = Mem.ReadBytes(6, Addresses.UnfreezeFunction);
            Mem.WriteBytes(Enumerable.Repeat((byte)0x90, 6).ToArray(), Addresses.UnfreezeFunction);
            Mem.WriteInt(1, Addresses.BaseAddress, Addresses.OffsetUnfreeze);
        }

        public void CancelUnfreeze()
        {
            Mem.WriteBytes(_unfreezeFuncBackup, Addresses.UnfreezeFunction);
        }

        public T StructToObject<T>(object structure) where T : new()
        {
            T t = new T();
            foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite))
            {
                string name = property.Name;
                var type = property.PropertyType;
                if (type == typeof(string))
                {
                    var field = structure.GetType().GetField('p' + name);
                    int pointer = (int)field.GetValue(structure);
                    property.SetValue(t, Mem.ReadStringToEnd(pointer), null);
                }
                else if (type == typeof(Coords) && name == "Coords")
                {
                    float x = (float)structure.GetType().GetField("X").GetValue(structure);
                    float y = (float)structure.GetType().GetField("Y").GetValue(structure);
                    float z = (float)structure.GetType().GetField("Z").GetValue(structure);
                    property.SetValue(t, new Coords(x, y, z), null);
                }
                else
                {
                    var field = structure.GetType().GetField(name);
                    if (field == null) continue;

                    object value = field.GetValue(structure);
                    if (!property.PropertyType.IsEnum)
                        value = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(t, value, null);
                }
            }
            return t;
        }

        public void Dispose()
        {
            Process.Dispose();
        }
    }
}
