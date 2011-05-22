using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using IDisposer.Core;
using System.IO;

namespace Tests
{
    [TestFixture]
    class InstrumenterTests
    {
        [Test]
        public void ShouldDetectAlreadyInstrumentedAssemblies()
        {
            using (InstrumentedCopy copy = new InstrumentedCopy("NonLeaking.exe"))
            {
                Assert.Throws(typeof(InvalidOperationException), () =>
                {
                    new Instrumenter(Path.GetDirectoryName(copy.FileName))
                        .Instrument(copy.FileName, copy.FileName);
                });
            }
        }
    }
}
