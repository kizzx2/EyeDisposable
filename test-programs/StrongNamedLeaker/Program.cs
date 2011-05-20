using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StrongNamedLeaker
{
    class Program
    {
        static void Main(string[] args)
        {
            new MemoryStream();

            using (new MemoryStream())
            {
            }
        }
    }
}
