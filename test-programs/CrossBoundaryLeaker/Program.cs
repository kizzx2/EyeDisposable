using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeakerLib;

namespace CrossBoundaryLeaker
{
    class Program
    {
        static void Main(string[] args)
        {
            var s1 = LeakFactory.CreateMemoryStream();

            var s2 = LeakFactory.CreateMemoryStream();
            s2.Dispose();

            using (LeakFactory.CreateMemoryStream())
            {
            }
        }
    }
}
