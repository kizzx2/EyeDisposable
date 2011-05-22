using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace IDisposer.Logger
{
    /// <summary>
    /// Write to both Trace output and a stream
    /// </summary>
    class TraceAndStreamWriter : IDisposable
    {
        StreamWriter _writer;

        public TraceAndStreamWriter(Stream stream)
        {
            _writer = new StreamWriter(stream);
        }

        public void WriteLine(string message)
        {
            _writer.WriteLine(message);
            Trace.WriteLine(message);
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
