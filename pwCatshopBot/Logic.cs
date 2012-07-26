using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PwLib;

namespace pwCatshopBot
{
    public enum State { Working, Suspending, Suspended, Resuming, Stopping, Stopped };

    public static class Logic
    {
        public static PwClient Client;
    }
}