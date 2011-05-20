using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Leaker2
{
    class Foo : IDisposable
    {
        public void Dispose()
        {
        }
    }

    class Program
    {
        static bool sTrue = new object() != null;

        static void Main(string[] args)
        {
            var stream = new MemoryStream();
            if (!sTrue)
                stream.Dispose();

            new Foo();
            GC.Collect();
        }
    }
}
