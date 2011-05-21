using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DotNet2Leaker
{
    class Program
    {
        static void Main(string[] args)
        {
            new MemoryStream();
            new MemoryStream();
            new MemoryStream();

            using (new MemoryStream())
            {
            }
        }
    }
}
