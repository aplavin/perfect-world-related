using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nini.Config;

namespace PwLib
{
    public class Addresses
    {
        private readonly PwClient _client;

        public int SendPacketFunction = 0x659450;
        public int BaseAddress = 0xB28AC4;
        public int LevelsExp = 0xAF7FD0;
        public int Fps = 0xB01A84;
        public int UnfreezeFunction = 0xB28534;

        public static int OffsetUnfreeze = 0x48C;
        public static int OffsetStruct = 0x1C;
        public static int OffsetPlayerStruct = 0x34;
        public static int OffsetActionStruct = 0x1050;

        public Addresses(PwClient client)
        {
            _client = client;

            var searcher = new AddressSearcher(_client.Process.MainModule.FileName);
            BaseAddress = searcher.Search(@"\x66\x8B\x4C\x24\x1C\x66\x89\x46\x1C\x66\x89\x4E\x1F\x8B\x15(?<val>.{4})\x6A\x21\x56\x8B\x4A\x20\xE8");
            SendPacketFunction = searcher.Search(@"\x6A\x21\x56\x8B\x4A\x20\xE8(?<val>.{4})\x56\xE8.{4}\x83\xC4\x04\x5E\xC3", (val, ind) => ind + val + 0x400004);
            OffsetUnfreeze = searcher.Search(@"\x0F\x95\xC0\x84\xC0\x88\x85(?<val>.{2}\x00\x00)\x75\x51\x8B\x94");
            UnfreezeFunction = (int)(_client.Process.MainModule.BaseAddress + searcher.Search(@"\x0F\x95\xC0\x84\xC0(?<val>\x88\x85.{2})\x00\x00\x75\x51\x8B\x94", (val, ind) => ind));
            //SearchAddresses();
        }

        private void SearchAddresses()
        {
            byte[] fileBytes;
            using (var file = File.OpenRead(_client.Process.MainModule.FileName))
            {
                fileBytes = new byte[file.Length];
                file.Read(fileBytes, 0, (int)file.Length);
            }
            string fileString = new string(fileBytes.Select(b => (char)b).ToArray());
            string regex = @"\x6A\x21\xE8.{4}\x8B\xF0\x83\xC4\x04\x85\xF6\x74.{1}\x8A\x44\x24\x18\x66\x8B\x4C\x24\x10\x66\xC7\x06\x00\x00\x88\x46\x1E\x8B\x44\x24\x08\x66\x89\x4E\x1A\xD9\x44\x24\x14\x8B\x10\x89\x56\x02\x8B\x48\x04\xD8\x0D.{4}\x89\x4E\x06\x8B\x50\x08\x8B\x44\x24\x0C\x89\x56\x0A\xD8\x05.{4}\x8B\x08\x89\x4E\x0E\x8B\x50\x04\x89\x56\x12\x8B\x40\x08\x89\x46\x16\xE8.{4}\x66\x8B\x4C\x24\x1C\x66\x89\x46\x1C\x66\x89\x4E\x1F\x8B\x15(.{4})\x6A\x21\x56\x8B\x4A\x20\xE8(.{4})\x56\xE8.{4}\x83\xC4\x04\x5E\xC3";
            var match = new Regex(regex).Match(fileString);

            string value = match.Groups[1].Value;
            BaseAddress = BitConverter.ToInt32(value.Select(ch => (byte)ch).ToArray(), 0);

            value = match.Groups[2].Value;
            SendPacketFunction = match.Index + 0x40007B + 7 + BitConverter.ToInt32(value.Select(ch => (byte)ch).ToArray(), 0);
        }
    }
}