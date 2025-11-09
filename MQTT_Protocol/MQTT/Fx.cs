using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQTT_Protocol
{
    internal class Fx
    {
        public static void StartThread(ThreadStart threadStart)
        {
            new Thread(threadStart).Start();
        }

        public static void SleepThread(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }
    }
}
