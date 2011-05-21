using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;

namespace IDisposer.Logger
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
            StringBuilder sb = new StringBuilder();

            // Shove the first frame and build a stack trace
            bool first = true;
            foreach (StackFrame frame in new StackTrace(true).GetFrames())
            {
                if (first) continue;
                first = false;

                sb.AppendLine(frame.ToString());
            }

            _dict.Add(obj, sb.ToString());
        }

        public static void Remove(IDisposable obj)
        {
            _dict.Remove(obj);
        }

        public static void Check()
        {
            Trace.WriteLine("====");
            Trace.WriteLine("Disposer check");

            if (_dict.Count > 0)
                Trace.WriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} leaks detected!", _dict.Count));
            Trace.WriteLine("====");

            foreach (KeyValuePair<IDisposable, string> obj in _dict)
            {
                Trace.WriteLine("Disposable object leaked!");
                Trace.WriteLine("Type: " + obj.Key.GetType().Name);
                Trace.WriteLine("Created at: " + obj.Value);
                Trace.WriteLine("");
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
