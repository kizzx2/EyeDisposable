using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.Threading;
using System.Diagnostics;

namespace Tests
{
    static class AssemblyLauncher
    {
        public static void Launch(string filename)
        {
            Exception exc = null;

            var t = new Thread(() =>
            {
                var domain = AppDomain.CurrentDomain;

                try
                {
                    domain.ExecuteAssembly(filename);
                }
                catch(Exception e)
                {
                    exc = e;
                }
                finally
                {
                    //AppDomain.Unload(domain);
                }
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

            if(exc != null)
            {
                throw new Exception("Exception thrown via executing assembly",
                    exc);
            }
        }
    }
}
