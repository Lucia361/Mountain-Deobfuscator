using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TropicalDeobfuscator.Protections
{
    internal class StringDecryptor
    {
        public static int Fix()
        {
            int Deobfuscated = 0;

            var Module = DeobfuscatorContext.Module;
            var ReflectionAssembly = DeobfuscatorContext.ReflectionAssembly;

            foreach (TypeDef Type in Module.GetTypes())
            {
                foreach (MethodDef Method in Type.Methods.ToArray())
                {
                    if (!Method.HasBody && !Method.Body.HasInstructions)
                        continue;
                    Method.RemoveUnusedNops();

                    var instr = Method.Body.Instructions;


                    for (int i = 0; i < instr.Count; i++)
                    {
                        if(instr[i].IsLdcI4() && instr[i + 1].OpCode == OpCodes.Call)
                        {
                            var decMethod = instr[i + 1].Operand as MethodDef;

                            
                            if (decMethod == null)
                                continue;
                            if (decMethod.Parameters.Count != 1)
                                continue;
                            if (!decMethod.HasReturnType)
                                continue;
                            if (decMethod.ReturnType != Module.CorLibTypes.String)
                                continue;

                            int decMethodToken = decMethod.MDToken.ToInt32();

                            object val = ReflectionAssembly.ManifestModule.ResolveMethod(decMethodToken).Invoke(null, new object[] { false });
                            /* ghetto invoke for string since it has tons of junk */
                            Console.WriteLine("Decrypted " + val.ToString());

                            instr[i].OpCode = OpCodes.Ldstr;
                            instr[i].Operand = val;
                            instr[i + 1].OpCode = OpCodes.Nop;
                            Deobfuscated++;

                            Type.Methods.Remove(decMethod);

                        }

                        try
                        {


                            if (instr[i].OpCode == OpCodes.Call)
                            {
                                var decMethod = instr[i].Operand as MethodDef;


                                if (decMethod == null)
                                    continue;
                                if (decMethod.HasParams())
                                    continue;
                                if (!decMethod.HasReturnType)
                                    continue;
                                if (decMethod.ReturnType != Module.CorLibTypes.Int32)
                                    continue;

                                int decMethodToken = decMethod.MDToken.ToInt32();

                                object val = ReflectionAssembly.ManifestModule.ResolveMethod(decMethodToken).Invoke(null,null);

                                Console.WriteLine("Decrypted " + val.ToString());

                                instr[i].OpCode = OpCodes.Ldc_I4;
                                instr[i].Operand = val;
                                Deobfuscated++;

                                Type.Methods.Remove(decMethod);

                            }
                        }
                        catch { /* ghetto invoke for int since it has tons of junk */ }
                    }
                }
            }

            return Deobfuscated;
        }
    }
}
