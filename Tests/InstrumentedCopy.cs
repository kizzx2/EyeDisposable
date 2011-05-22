using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using EyeDisposable.Core;

namespace Tests
{
    sealed class InstrumentedCopy : IDisposable
    {
        public string FileName { get; private set; }
        public string DirectoryName { get; private set; }

        public InstrumentedCopy(string filename) : this(filename,
            Path.Combine(Path.GetDirectoryName(filename),
            Path.GetRandomFileName()))
        {
        }

        public InstrumentedCopy(string filename, string dir)
        {
            DirectoryName = dir;

            if(!Directory.Exists(DirectoryName))
                Directory.CreateDirectory(DirectoryName);

            string outFilename = Path.Combine(DirectoryName, 
                Path.GetFileName(filename));
            new Instrumenter().Instrument(filename, outFilename);

            FileName = outFilename;
        }

        public void Dispose()
        {
            if (DirectoryName != null && Directory.Exists(DirectoryName))
                Directory.Delete(DirectoryName, true);
        }
    }
}
