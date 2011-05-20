using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    class SampleDisposable : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
        }

        ~SampleDisposable()
        {
            Dispose(false);
        }
    }
}
