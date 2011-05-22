using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.IO;
using System.Reflection;

namespace IDisposer.Logger
{
    public static class DisposerRegistry
    {
        public class DisposableObject
        {
            public readonly IDisposable Target;
            public readonly string StackTrace;

            public DisposableObject(IDisposable target, string stacktrace)
            {
                Target = target;
                StackTrace = stacktrace;
            }
        }

        static Dictionary<int, DisposableObject> _dict =
            new Dictionary<int, DisposableObject>();

        public static void Add(IDisposable obj)
        {
            Debug.WriteLine(string.Format("Adding object `{0}`",
                obj.GetType().Name));

            StringBuilder sb = new StringBuilder();

            // Shove the first frame and build a stack trace
            bool first = true;
            foreach (StackFrame frame in new StackTrace(true).GetFrames())
            {
                if (first)
                {
                    first = false;
                    continue;
                }

                sb.Append(string.Format("> [{0}] {1} {2}",
                    frame.GetMethod().DeclaringType.FullName,
                    frame.GetMethod().ToString(),
                    frame.GetFileName() == null ?
                        Environment.NewLine :
                        frame.ToString()));
            }

            _dict.Add(RuntimeHelpers.GetHashCode(obj),
                new DisposableObject(obj, sb.ToString()));
        }

        public static void Remove(IDisposable obj)
        {
            Debug.WriteLine(string.Format("Removing object `{0}`",
                obj.GetType().Name));
            _dict.Remove(RuntimeHelpers.GetHashCode(obj));
        }

        public static void Check()
        {
            using(var stream = new FileStream(Assembly.GetEntryAssembly().Location +
                 ".DisposeLeaks.log", FileMode.Create))
            using(var writer =new TraceAndStreamWriter(stream))
            {
                writer.WriteLine("====");
                writer.WriteLine("Disposer check");

                if (_dict.Count > 0)
                    writer.WriteLine(string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} leaks detected!", _dict.Count));
                writer.WriteLine("====");

                foreach (var obj in _dict)
                {
                    writer.WriteLine("Disposable object leaked!");
                    writer.WriteLine("Hash code: " +
                        obj.Key.ToString(CultureInfo.InvariantCulture));
                    writer.WriteLine("Type: " + obj.Value.Target.GetType().FullName);
                    writer.WriteLine("Created at: " + Environment.NewLine
                        + obj.Value.StackTrace);
                    writer.WriteLine("");
                }
            }

            if (_dict.Count > 0 && Debugger.IsAttached)
                Debugger.Break();
        }

        public static void Clear()
        {
            _dict.Clear();
        }

        public static Dictionary<int, DisposableObject> LeakedObjects
        {
            get { return _dict; }
        }
    }
}
