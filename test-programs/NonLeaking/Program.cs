using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NonLeaking
{
    static class Disposer
    {
        public static void Dispose(IDisposable obj)
        {
            obj.Dispose();
        }
    }

    class Foo : IDisposable
    {
        public void Dispose()
        {
        }

        public void DisposeIndirectly()
        {
            Dispose();
        }
    }


    /// <summary>
    /// This program should not be detected as leaking.
    /// </summary>
    class Program
    {
        // Use this in if() statements to false a branch in generated IL
        static bool sTrue = new object() != null;

        static void Main(string[] args)
        {
            new object();

            try
            {
                using (new MemoryStream())
                {
                    throw new Exception();
                }
            }
            catch { }

            object s1 = new MemoryStream();
            (s1 as IDisposable).Dispose();

            var s2 = new MemoryStream();
            if (sTrue)
                s2.Dispose();

            Disposer.Dispose(new MemoryStream());

            new Foo().DisposeIndirectly();
        }
    }
}
