using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LeakerLib
{
    public static class LeakFactory
    {
        public static MemoryStream CreateMemoryStream()
        {
            return new MemoryStream();
        }
    }
}
