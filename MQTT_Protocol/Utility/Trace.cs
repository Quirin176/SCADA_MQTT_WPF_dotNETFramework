using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Utility
{
    public static class Trace
    {
        public static TraceLevel TraceLevel;

        public static WriteTrace TraceListener;

        [Conditional("DEBUG")]
        public static void Debug(string format, params object[] args)
        {
            if (TraceListener != null)
            {
                TraceListener(format, args);
            }
        }

        public static void WriteLine(TraceLevel level, string format)
        {
            if (TraceListener != null && (level & TraceLevel) > (TraceLevel)0)
            {
                TraceListener(format);
            }
        }

        public static void WriteLine(TraceLevel level, string format, object arg1)
        {
            if (TraceListener != null && (level & TraceLevel) > (TraceLevel)0)
            {
                TraceListener(format, arg1);
            }
        }

        public static void WriteLine(TraceLevel level, string format, object arg1, object arg2)
        {
            if (TraceListener != null && (level & TraceLevel) > (TraceLevel)0)
            {
                TraceListener(format, arg1, arg2);
            }
        }

        public static void WriteLine(TraceLevel level, string format, object arg1, object arg2, object arg3)
        {
            if (TraceListener != null && (level & TraceLevel) > (TraceLevel)0)
            {
                TraceListener(format, arg1, arg2, arg3);
            }
        }
    }
}
