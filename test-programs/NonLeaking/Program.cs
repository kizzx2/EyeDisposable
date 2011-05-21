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

    class Bar
    {
        public void Dispose()
        {
        }
    }

    static class TestCases
    {
        // Use this in if() statements to false a branch in generated IL
        static bool sTrue = new object() != null;

        public static void CreateAnObject()
        {
            new object();
        }

        public static void SomeBasicScenarios()
        {
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

        public static void DisposeTwice()
        {
            var f = new Foo();
            f.Dispose();
            f.Dispose();
        }

        public static void NonDisposableItemWithAMethodCalledDispose()
        {
            new Bar();
        }
    }

    /// <summary>
    /// This program should not be detected as leaking.
    /// </summary>
    class Program
    {

        static void Main(string[] args)
        {
            TestCases.CreateAnObject();
            TestCases.SomeBasicScenarios();
            TestCases.DisposeTwice();
            TestCases.NonDisposableItemWithAMethodCalledDispose();
        }
    }
}
