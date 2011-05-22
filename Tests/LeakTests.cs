using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using IDisposer.Core;
using IDisposer.Logger;
using Mono.Cecil;
using System.Threading;

namespace Tests
{
    [TestFixture]
    class LeakTests
    {
        [TestCase("NonLeaking.exe", 0)]
        [TestCase("Leaker1.exe", 1)]
        [TestCase("Leaker2.exe", 2)]
        [TestCase("StrongNamedLeaker.exe", 1)]
        [TestCase("DotNet2Leaker.exe", 3)]
        [TestCase("DotNet4Leaker.exe", 2)]
        [TestCase("StaThreadLeaker.exe", 1)]
        public void LeakTest(string filename, int expectedLeaks)
        {
            DisposerRegistry.Clear();

            using (var copy = new InstrumentedCopy(filename))
            {
                AssemblyLauncher.Launch(copy.FileName);
            }

            Assert.AreEqual(expectedLeaks, DisposerRegistry.LeakedObjects.Count);
        }
    }
}
