using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PwLib
{
    public class AddressSearcher
    {
        public string FileName { get; private set; }
        private readonly string _fileString;

        public AddressSearcher(string fileName)
        {
            FileName = fileName;

            byte[] fileBytes;
            using (var file = File.OpenRead(fileName))
            {
                fileBytes = new byte[file.Length];
                file.Read(fileBytes, 0, (int)file.Length);
            }
            _fileString = new string(fileBytes.Select(b => (char)b).ToArray());
        }

        private Match GetMatch(string regex)
        {
            var matches = new Regex(regex).Matches(_fileString);
            return matches.Cast<Match>().Single();
        }

        public int Search(string regex)
        {
            return Search(regex, val => val);
        }

        public int Search(string regex, Func<int, int> postProcess)
        {
            return Search(regex, (val, ind) => postProcess(val));
        }

        public int Search(string regex, Func<int, int, int> postProcess)
        {
            var match = GetMatch(regex);
            var group = match.Groups["val"];
            if (group.Success)
            {
                int value = BitConverter.ToInt32(group.Value.Select(ch => (byte)ch).ToArray(), 0);
                return postProcess(value, match.Groups["val"].Index);
            }
            else
            {
                return postProcess(0, match.Index);
            }
        }
    }
}