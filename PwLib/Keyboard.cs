using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PwLib
{
    public class Keyboard
    {
        private PwClient _client;

        public Keyboard(PwClient client)
        {
            _client = client;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass = null, string lpszWindow = null);

        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        private const uint WM_KEYDOWN = 0x100;
        private const Int32 WM_KEYUP = 0x101;

        private static readonly Dictionary<string, int> vkCodes = new Dictionary<string, int>{
        {"lmb", 0x01},
        {"rmb", 0x02},
        {"bs", 0x08},
        {"tab", 0x09},
        {"clear", 0x0C},
        {"enter", 0x0D},
        {"shift", 0x10},
        {"ctrl", 0x11},
        {"alt", 0x12},
        {"esc", 0x1B},
        {"space", 0x20},
        {"pgup", 0x21},
        {"pgdn", 0x22},
        {"end", 0x23},
        {"home", 0x24},
        {"left", 0x25},
        {"up", 0x26},
        {"right", 0x27},
        {"down", 0x28},
        {"del", 0x2E},
        };

        public void SendKey(string key)
        {
            key = key.ToLower();

            int vkCode;
            if (vkCodes.ContainsKey(key))
                vkCode = vkCodes[key];
            else if (2 <= key.Length && key.Length <= 3 && key.StartsWith("f"))
                vkCode = 0x70 + (int.Parse(key.Substring(1)) - 1);
            else if (key.Length == 4 && key.StartsWith("num"))
                vkCode = 0x60 + (key[3] - '0');
            else if (key.Length == 1 && char.IsDigit(key[0]))
                vkCode = 0x30 + (key[0] - '0');
            else if (key.Length == 1 && char.IsLetter(key[0]))
                vkCode = 0x41 + (key[0] - 'a');
            else
                throw new ArgumentException(string.Format("Not supported key: '{0}'", key));

            IntPtr hWnd = _client.Process.MainWindowHandle;
            PostMessage(hWnd, WM_KEYDOWN, vkCode, 0);
            PostMessage(hWnd, WM_KEYUP, vkCode, 0);
        }
    }
}
