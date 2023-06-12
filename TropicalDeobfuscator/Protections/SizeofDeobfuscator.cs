using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace TropicalDeobfuscator.Protections
{
    internal class SizeofDeobfuscator
    {
        private static Assembly ReflectionAssembly = DeobfuscatorContext.ReflectionAssembly;
        private static Module ReflectionModule = DeobfuscatorContext.ReflectionAssembly.ManifestModule;

        public static int Fix()
        {
            int Deobfuscated = 0;

            var Module = DeobfuscatorContext.Module;

            foreach (TypeDef Type in Module.GetTypes())
            {
                foreach (MethodDef Method in Type.Methods)
                {
                    if (!Method.HasBody && !Method.Body.HasInstructions)
                        continue;
                    var instr = Method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].OpCode == dnlib.DotNet.Emit.OpCodes.Sizeof)
                        {
                            var TypeDD = System.Type.GetType(instr[i].Operand.ToString());
                            int realValue = GetSize(TypeDD); //Use Reflection to Invoke Size reliable than Marshal.SizeOf
                            instr[i].OpCode = dnlib.DotNet.Emit.OpCodes.Ldc_I4;
                            instr[i].Operand = realValue;                       
                            Deobfuscated++;
                        }
                    }
                }
            }

            return Deobfuscated;
        }
        private static int GetSize(Type type) 
        {
            var dm = new DynamicMethod("", typeof(int), Type.EmptyTypes, ReflectionModule, true);
            var gen = dm.GetILGenerator();

            gen.Emit(System.Reflection.Emit.OpCodes.Sizeof, type);
            gen.Emit(System.Reflection.Emit.OpCodes.Ret);

            return (int)dm.Invoke(null, null);
        }
    }
}
