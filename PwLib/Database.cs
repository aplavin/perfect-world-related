using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PwLib
{
    public static class Database
    {
        public static string PwVer = "ru";

        public static string GetItemName(int itemId, string lang = null)
        {
            if (lang == null)
                lang = PwVer;
            string page = InternetUtils.FetchPage(string.Format("http://pwdatabase.com/{0}/items/{1}", lang, itemId));
            return page.GetBetween("<title>Perfect World Item Database - ", "</title>");
        }

        public static string GetSkillName(int skillId)
        {
            var lines = File.ReadLines("skills.dat", Encoding.GetEncoding(1251));
            string startStr = skillId.ToString() + " - ";
            string line = lines.First(l => l.StartsWith(startStr));
            return line.GetAfter(" - ");
        }
    }
}
