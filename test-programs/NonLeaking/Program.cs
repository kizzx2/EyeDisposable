using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

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
        int _n;

        public Foo(int n)
        {
            _n = n;
        }

        public void Dispose()
        {
        }

        public override int GetHashCode()
        {
            return _n;
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
            new Foo(0).DisposeIndirectly();
        }

        public static void DisposeTwice()
        {
            var f = new Foo(0);
            f.Dispose();
            f.Dispose();
        }

        public static void NonDisposableItemWithAMethodCalledDispose()
        {
            new Bar();
        }

        public static void SameHashCode()
        {
            // Font overrides GetHashCode() and Equals()
            var f1 = new Font("Arial", 10, FontStyle.Regular);
            var f2 = new Font("Arial", 10, FontStyle.Regular);

            f1.Dispose();
            f2.Dispose();
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
            TestCases.SameHashCode();
        }
    }
}
