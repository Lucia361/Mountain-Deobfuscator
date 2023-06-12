using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TropicalDeobfuscator.Protections
{
    internal class RemoveJunk
    {
        public static void Fix()
        {
            var module = DeobfuscatorContext.Module;
            MethodDef intDecryptor = null;
            MethodDef antiTamperMethod = null;
            MethodDef antiDebugMethod = null;

            var instrGlobal = module.GlobalType.FindOrCreateStaticConstructor().Body.Instructions;

           


            foreach (TypeDef type in module.GetTypes().ToArray())
            {
                foreach(var method in type.Methods.ToArray())
                {
                    if (method == DeobfuscatorContext.BoolDecryptionMethod)
                        module.Types.Remove(method.DeclaringType);


                    var someJunk = instrGlobal[0].Operand as MethodDef;
                    var someJunk2 = instrGlobal[1].Operand as MethodDef;
                    var someJunk3 = instrGlobal[2].Operand as MethodDef;

                    module.Types.Remove(someJunk.DeclaringType);
                    module.Types.Remove(someJunk2.DeclaringType);
                    module.Types.Remove(someJunk3.DeclaringType);


                }
            }
            instrGlobal[0].OpCode = OpCodes.Nop;
            instrGlobal[1].OpCode = OpCodes.Nop;
            instrGlobal[2].OpCode = OpCodes.Nop;
        }
    }
}
