using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PwLib
{
    public static class PwStarter
    {
        public static Process StartClient(string path)
        {
            foreach (var exePath in new[] { Path.Combine(path, "elementclient.exe"), Path.Combine(path, "element\\elementclient.exe") })
            {
                if (File.Exists(exePath))
                {
                    return Process.Start(exePath);
                }
            }
            throw new ArgumentException();
        }

        public static void WaitForRun(Process process)
        {

        }
    }
}
