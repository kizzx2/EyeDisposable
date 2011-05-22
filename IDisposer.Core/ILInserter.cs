using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace IDisposer.Core
{
    /// <summary>
    /// Syntactic sugar to make fluent InsertAfter() calls
    /// </summary>
    class ILInserter
    {
        ILProcessor _il;
        Instruction _anchor;

        public ILInserter(ILProcessor il, Instruction anchor)
        {
            _il = il;
            _anchor = anchor;
        }

        public ILInserter Append(Instruction instruction)
        {
            _il.InsertAfter(_anchor, instruction);
            return new ILInserter(_il, instruction);
        }
    }
}
