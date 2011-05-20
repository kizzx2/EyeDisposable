using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace IDisposerCore
{
    static class Logger
    {
        public static void Log(string message)
        {
            Debugger.Log(1, "INFO", message);
        }

        public static void Log(string format, params object[] args)
        {
            Debugger.Log(1, "INFO", string.Format(format, args));
        }
    }
}
