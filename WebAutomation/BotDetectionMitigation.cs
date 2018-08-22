using System;
using System.Diagnostics;
using System.Threading;

namespace WebAutomation
{
    public static class BotDetectionMitigation
    {
        const int minWaitMs = 1000;
        const int maxWaitMs = 3000;
        static Random random = new Random();

        public static void RandomizedWait()
        {
            var waitTime = minWaitMs + (int)((maxWaitMs - minWaitMs) * random.NextDouble());
            Trace.TraceInformation("waiting {0}ms", waitTime);
            Thread.Sleep(waitTime);
        }
    }
}
