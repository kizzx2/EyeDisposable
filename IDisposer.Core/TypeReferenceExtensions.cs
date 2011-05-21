using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace IDisposer.Core
{
    static class TypeReferenceExtensions
    {
        /// <summary>
        /// Traverse up object hierarchy to see if `iface` is implemented.
        /// </summary>
        public static bool HasInterface(this TypeReference type,
            string ifaceFullname)
        {
            var resolved = type.Resolve();

            if (resolved.Interfaces.Any(i => i.Resolve().FullName == ifaceFullname))
                return true;

            var b = resolved.BaseType;
            if (b == null)
                return false;

            return b.HasInterface(ifaceFullname);
        }
    }
}
