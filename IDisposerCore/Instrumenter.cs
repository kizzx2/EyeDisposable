using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;

namespace IDisposerCore
{
    public static class Instrumenter
    {
        public static void Instrument(string inputFile, string outputFile)
        {
            using (var output = new MemoryStream())
            {
                using (var input = new FileStream(inputFile, FileMode.Open))
                {
                    Instrument(input, output);
                }

                using(var outFileStream = new FileStream(outputFile, FileMode.OpenOrCreate))
                {
                    output.WriteTo(outFileStream);
                }
            }
        }

        public static void Instrument(Stream input, Stream output)
        {
            bool hasSymbol = false;
            var inputFileStream = input as FileStream;
            if (inputFileStream != null)
                hasSymbol = PdbExistsForFile(inputFileStream.Name);

            var asm = AssemblyDefinition.ReadAssembly(input,
                new ReaderParameters { ReadSymbols = hasSymbol } );
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
                    var newobjs = new List<Instruction>();
                    var disposes = new List<Instruction>();

                    foreach (var i in m.Body.Instructions)
                    {
                        var method = i.Operand as MethodReference;
                        if (method == null)
                            continue;

                        if (i.OpCode == OpCodes.Newobj &&
                            method.DeclaringType.HasInterface(idisposable))
                        {
                            newobjs.Add(i);
                        }

                        else if (i.OpCode == OpCodes.Callvirt ||
                            i.OpCode == OpCodes.Call)
                        {
                            if (method.Resolve().Equals(drAdd))
                                throw new InvalidOperationException(
                                    "Assembly seems already instrumented.");

                            else if (method.Name == "Dispose" &&
                                method.Parameters.Count == 0 &&
                                method.ReturnType.Resolve().Equals(typeVoid))
                                disposes.Add(i);
                        }
                    }

                    var il = m.Body.GetILProcessor();

                    foreach (var i in newobjs)
                    {
                        new ILInserter(il, i).Insert(il.Create(OpCodes.Dup))
                            .Insert(il.Create(OpCodes.Call, drAddRef));
                    }

                    foreach (var i in disposes)
                    {
                        // Put instrumenting opcodes _after_ 
                        // the instruction, and then replace with Nop.
                        // This way we don't have to deal with seqeunce 
                        // points.
                        new ILInserter(il, i).Insert(il.Create(OpCodes.Dup))
                            .Insert(il.Create(OpCodes.Call, drRemoveRef))
                            .Insert(i);
                        il.Replace(i, il.Create(OpCodes.Nop));
                    }

                    Console.WriteLine("- {0}: {1} news; {2} disposes",
                        m.FullName, newobjs.Count, disposes.Count);
                }
            }

            asm.Write(output, new WriterParameters { WriteSymbols = hasSymbol });
        }

        static bool PdbExistsForFile(string filename)
        {
            return File.Exists(Path.ChangeExtension(filename, ".pdb"));
        }
    }
}
