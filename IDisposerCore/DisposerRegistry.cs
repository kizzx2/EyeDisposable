using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace IDisposerCore
{
    public static class DisposerRegistry
    {
        class LeakChecker
        {
            ~LeakChecker()
            {
                DisposerRegistry.Check();
            }
        }

        static Dictionary<IDisposable, string> _dict =
            new Dictionary<IDisposable, string>();
        static LeakChecker _checker = new LeakChecker();

        public static void Add(IDisposable obj)
        {
            var sb = new StringBuilder();

            // Shove the first frame and build a stack trace
            foreach (var frame in new StackTrace(true).GetFrames().Skip(1))
                sb.AppendLine(frame.ToString());

            _dict.Add(obj, sb.ToString());
        }

        public static void Remove(IDisposable obj)
        {
            _dict.Remove(obj);
        }

        public static void Check()
        {
            Logger.Log("====");
            Logger.Log("Disposer check");

            if (_dict.Count > 0)
                Logger.Log("{0} leaks detected!", _dict.Count);
            Logger.Log("====");

            foreach (var obj in _dict)
            {
                Logger.Log("Disposable object leaked!");
                Logger.Log("Type: " + obj.Key.GetType().Name);
                Logger.Log("Created at: " + obj.Value);
                Logger.Log("");
            }

            if (_dict.Count > 0 && Debugger.IsAttached)
                Debugger.Break();
        }

        public static void Clear()
        {
            _dict.Clear();
        }

        public static Dictionary<IDisposable, string> LeakedObjects
        {
            get { return _dict; }
        }
    }
}
