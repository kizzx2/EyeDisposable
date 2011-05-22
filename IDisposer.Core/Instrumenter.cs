using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;
using IDisposer.Logger;

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

            var idisposable = mod.Import(typeof(IDisposable)).Resolve();
            var typeVoid = mod.Import(typeof(void)).Resolve();

            foreach (var t in asm.MainModule.Types)
            {
                Console.WriteLine("Instrumenting type `{0}`...", t.Name);
                foreach (var m in t.Methods)
                {
                    if(m.Body == null)
                        continue;

                    var newobjs = new List<Instruction>();
                    var disposes = new List<Instruction>();

                    foreach (var i in m.Body.Instructions)
                    {
                        var method = i.Operand as MethodReference;
                        if (method == null)
                            continue;

                        if (i.OpCode == OpCodes.Newobj &&
                            method.DeclaringType.HasInterface(
                            idisposable.FullName))
                        {
                            newobjs.Add(i);
                        }

                        else if (i.OpCode == OpCodes.Callvirt ||
                            i.OpCode == OpCodes.Call)
                        {
                            if (method.FullName == drAdd.FullName)
                                throw new InvalidOperationException(
                                    "Assembly seems already instrumented.");

                            else if (
                                method.Name == "Dispose" &&
                                method.Parameters.Count == 0 &&
                                method.ReturnType.Resolve().FullName ==
                                    typeVoid.FullName)
                            {
                                // We don't do value types yet
                                if(method.DeclaringType.IsValueType)
                                    continue;

                                // This may happen if the user code contains 
                                // a `using` block with a value type 
                                // implementing IDisposable
                                else if (i.Previous.OpCode == OpCodes.Constrained)
                                {
                                    var constrainedType =
                                        i.Previous.Operand as TypeDefinition;

                                    if(constrainedType != null &&
                                        constrainedType.IsValueType)
                                        continue;
                                }

                                disposes.Add(i);
                            }
                        }
                    }

                    var il = m.Body.GetILProcessor();

                    foreach (var i in newobjs)
                    {
                            new ILInserter(il, i)
                                .Append(il.Create(OpCodes.Call, drAddRef));
                    }

                    foreach (var i in disposes)
                    {
                        // Put instrumenting opcodes _after_ 
                        // the instruction, and then replace with Nop.
                        // This way we don't have to deal with branches. 
                        new ILInserter(il, i)
                            .Append(il.Create(OpCodes.Call, drRemoveRef))
                            .Append(i);
                        il.Replace(i, il.Create(OpCodes.Nop));
                    }

                    if(newobjs.Count > 0 || disposes.Count > 0)
                        Console.WriteLine("- {0}: {1} newobjs; {2} disposes",
                            m.FullName, newobjs.Count, disposes.Count);
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
