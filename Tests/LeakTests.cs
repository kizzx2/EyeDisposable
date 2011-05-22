using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using EyeDisposable.Core;
using EyeDisposable.Logger;
using Mono.Cecil;
using System.Threading;
using System.Diagnostics;

namespace Tests
{
    [TestFixture]
    class LeakTests
    {
        [SetUp]
        public void SetUp()
        {
            DisposerRegistry.Clear();
        }

        [TestCase("NonLeaking.exe", 0)]
        [TestCase("Leaker1.exe", 1)]
        [TestCase("Leaker2.exe", 2)]
        [TestCase("StrongNamedLeaker.exe", 1)]
        [TestCase("DotNet2Leaker.exe", 3)]
        [TestCase("DotNet4Leaker.exe", 2)]
        [TestCase("StaThreadLeaker.exe", 1)]
        public void LeakTest(string filename, int expectedLeaks)
        {
            using (var copy = new InstrumentedCopy(filename))
            {
                AssemblyLauncher.Launch(copy.FileName);
            }

            Assert.AreEqual(expectedLeaks, DisposerRegistry.LeakedObjects.Count);
        }

        [Test]
        public void CrossBoundaryLeakTest()
        {
            // This actually works, but writing test for it is a little 
            // more involved. The test runner would always resolve the 
            // un-instrumented LeakerLib. We'll need to use a separate 
            // AppDomain
            Assert.Ignore("Test not implemented");

            using (var lib = new InstrumentedCopy("LeakerLib.dll"))
            using (var exe = new InstrumentedCopy("CrossBoundaryLeaker.exe",
                lib.DirectoryName))
            {
                AssemblyLauncher.Launch(exe.FileName);
            }

            Assert.AreEqual(1, DisposerRegistry.LeakedObjects.Count);
        }
    }
}
