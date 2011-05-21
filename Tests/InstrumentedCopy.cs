using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IDisposerCore;

namespace Tests
{
    sealed class InstrumentedCopy : IDisposable
    {
        public string FileName { get; private set; }

        public InstrumentedCopy(string filename)
        {
            var dir = Path.GetDirectoryName(filename);
            string outFilename = Path.Combine(dir, 
                Path.ChangeExtension(Path.GetRandomFileName(),
                Path.GetExtension(filename)));
            new Instrumenter().Instrument(filename, outFilename);

            FileName = outFilename;
        }

        public void Dispose()
        {
            if (File.Exists(FileName))
                File.Delete(FileName);
        }
    }
}
