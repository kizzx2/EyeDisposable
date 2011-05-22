using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.Threading;

namespace Tests
{
    static class AssemblyLauncher
    {
        public static void Launch(string filename)
        {
            var t = new Thread(() =>
            {
                AppDomain.CurrentDomain.ExecuteAssembly(filename);
            });

            foreach (var a in AssemblyDefinition.ReadAssembly(filename)
                .EntryPoint.CustomAttributes)
            {
                if (a.AttributeType.FullName == "System.STAThreadAttribute")
                    t.SetApartmentState(ApartmentState.STA);

                else if(a.AttributeType.FullName == "System.MTAThreadAttribute")
                    t.SetApartmentState(ApartmentState.MTA);
            }

            t.Start();
            t.Join();
        }
    }
}
