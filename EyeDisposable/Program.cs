using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using EyeDisposable.Core;
using System.Reflection;

namespace EyeDisposable
{
    static class Program
    {
        static void Usage()
        {
            Console.WriteLine("EyeDisposable by Chris Yuen <chris@kizzx2.com> 2011");
            Console.WriteLine();
            Console.WriteLine("Instrument assembly to catch IDispose leaks.");
            Console.WriteLine();
            Console.WriteLine("Example: {0} foo.exe");
            Console.WriteLine("Example: {0} foo.dll");
        }

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Usage();
                return;
            }

            var targetDir = Path.GetDirectoryName(args[0]);

            new Instrumenter(targetDir).Instrument(args[0], args[0]);

            // Put EyeDisposable.Logger.dll next to my target
            File.Copy("EyeDisposable.Logger.dll", Path.Combine(
                targetDir, "EyeDisposable.Logger.dll"), true);

            if (File.Exists("EyeDisposable.Logger.pdb"))
                File.Copy("EyeDisposable.Logger.pdb", Path.Combine(targetDir,
                    "EyeDisposable.Logger.pdb"), true);
        }
    }
}
