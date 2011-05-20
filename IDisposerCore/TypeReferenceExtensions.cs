using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace IDisposerCore
{
    static class TypeReferenceExtensions
    {
        /// <summary>
        /// Traverse up object hierarchy to see if `iface` is implemented.
        /// </summary>
        public static bool HasInterface(this TypeReference type,
            TypeDefinition iface)
        {
            var resolved = type.Resolve();

            if (resolved.Interfaces.Any(i => i.Resolve().Equals(iface)))
                return true;

            var b = resolved.BaseType;
            if (b == null)
                return false;

            return b.HasInterface(iface);
        }
    }
}
