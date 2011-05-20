using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using IDisposerCore;

namespace Tests
{
    [TestFixture]
    class LeakTests
    {
        [TestCase("NonLeaking.exe", 0)]
        [TestCase("Leaker1.exe", 1)]
        [TestCase("Leaker2.exe", 2)]
        //[TestCase("StrongNamedLeaker.exe", 1)]
        public void LeakTest(string filename, int expectedLeaks)
        {
            DisposerRegistry.Clear();
            using (var copy = new InstrumentedCopy(filename))
            {
                AppDomain.CurrentDomain.ExecuteAssembly(copy.FileName);
            }
            Assert.AreEqual(expectedLeaks, DisposerRegistry.LeakedObjects.Count);
        }

        [Test]
        public void StrongNamedLeakTest()
        {
            Assert.Ignore("Strong-named assemblies not supported yet.");
        }
    }
}
