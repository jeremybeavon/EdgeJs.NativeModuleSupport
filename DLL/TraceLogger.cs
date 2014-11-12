using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Logging;

namespace EdgeJs.NativeModuleSupport
{
    public sealed class TraceLogger : ConsoleLogger
    {
        public TraceLogger()
        {
            WriteHandler = message =>
            {
                Console.Write(message);
                Trace.Write(message);
            };
        }
    }
}
