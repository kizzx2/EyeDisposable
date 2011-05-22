using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IDisposer.Logger;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace IDisposer.Core
{
    public class Instrumenter
    {
        BaseAssemblyResolver _resolver = new DefaultAssemblyResolver();

        public Instrumenter(params string[] searchDirectories)
        {
            foreach(string dir in searchDirectories)
                _resolver.AddSearchDirectory(dir);
        }

        public void Instrument(string input, string output)
        {
            bool hasSymbol = File.Exists(Path.ChangeExtension(input, ".pdb"));

            var asm = AssemblyDefinition.ReadAssembly(input,
                new ReaderParameters {
                    ReadSymbols = hasSymbol,
                    AssemblyResolver = _resolver
                } );
            var mod = asm.MainModule;

            var drAddRef = mod.Import(typeof(DisposerRegistry).GetMethod("Add"));
            var drCheckRef = mod.Import(typeof(DisposerRegistry).GetMethod("Check"));
            var drRemoveRef = mod.Import(typeof(DisposerRegistry).GetMethod("Remove"));

            var drAdd = drAddRef.Resolve();

            foreach (var t in asm.MainModule.Types)
            {
                Console.WriteLine("Instrumenting type `{0}`...", t.Name);
                foreach (var m in t.Methods)
                {
                    if (m.Body == null)
                        continue;

                    // If the method does not contain any instrument-able 
                    // targets, avoid touching it at all
                    if(ILExtractor.FindNewObjsAndDisposes(m.Body).IsEmpty)
                        continue;

                    if (ILExtractor.ContainsCallTo(m.Body, drAdd.FullName))
                        throw new InvalidOperationException(
                            "Assembly appears to be already instrumented.");

                    // This is necessary to make Cecil output proper codes 
                    // in more complicated cases
                    m.Body.SimplifyMacros();
                    var targets = ILExtractor.FindNewObjsAndDisposes(m.Body);

                    var il = m.Body.GetILProcessor();

                    foreach (var i in targets.NewObjs)
                    {
                        new ILInserter(il, i)
                            .Append(il.Create(OpCodes.Dup))
                            .Append(il.Create(OpCodes.Call, drAddRef));
                    }

                    foreach (var i in targets.Disposes)
                    {
                        var instrumentTarget = i;

                        // Constrained-Callvirt pair must be moved
                        // atomically
                        if (i.Previous.OpCode == OpCodes.Constrained)
                            instrumentTarget = i.Previous;
                        
                        // Put instrumenting opcodes _after_ 
                        // the instruction, and then replace with Nop.
                        // This way we don't have to deal with branches. 
                        new ILInserter(il, instrumentTarget)
                            .Append(il.Create(OpCodes.Dup))
                            .Append(il.Create(OpCodes.Call, drRemoveRef))
                            .Append(instrumentTarget);
                        il.Replace(instrumentTarget, il.Create(OpCodes.Nop));
                    }

                    Console.WriteLine("- {0}: {1} newobjs; {2} disposes",
                        m.FullName, targets.NewObjs.Count,
                        targets.Disposes.Count);
                }
            }

            if (asm.MainModule.EntryPoint != null)
            {
                Console.WriteLine("Instrumenting entry point...");

                var oldMain = mod.EntryPoint;

                var newMain = new MethodDefinition("IDisposer_NewMain",
                    oldMain.Attributes,
                    mod.Import(typeof(void)));
                newMain.Parameters.Add(new ParameterDefinition(
                    mod.Import(typeof(string[]))));
                foreach (var attr in oldMain.CustomAttributes)
                    newMain.CustomAttributes.Add(attr);

                var il = newMain.Body.GetILProcessor();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, oldMain);
                il.Emit(OpCodes.Call, drCheckRef);
                il.Emit(OpCodes.Ret);

                mod.EntryPoint.DeclaringType.Methods.Add(newMain);
                mod.EntryPoint = newMain;
            }

            RemoveStrongName(asm);

            asm.Write(output, new WriterParameters { WriteSymbols = hasSymbol });
        }

        static void RemoveStrongName(AssemblyDefinition asm)
        {
            if (!asm.Name.HasPublicKey)
                return;

            asm.Name.Attributes &= AssemblyAttributes.PublicKey;
            asm.Name.HashAlgorithm = AssemblyHashAlgorithm.None;
            asm.Name.PublicKeyToken = null;
            asm.Name.PublicKey = null;
        }

        static bool PdbExistsForFile(string filename)
        {
            return File.Exists(Path.ChangeExtension(filename, ".pdb"));
        }
    }
}
