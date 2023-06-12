using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TropicalDeobfuscator.Protections
{
    internal class BoolDecryptor
    {
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
                        if (instr[i].IsLdcI4() && instr[i + 1].OpCode== OpCodes.Call)
                        {

                            var meth = instr[i + 1].Operand as MethodDef;

                            if (meth == null)
                                continue;
                            if (!meth.HasReturnType)
                                continue;
                            if (meth.ReturnType != Module.CorLibTypes.Boolean)
                                continue;

                            DeobfuscatorContext.BoolDecryptionMethod = meth;

                            int Value = instr[i].GetLdcI4Value();
                            instr[i].OpCode = OpCodes.Ldc_I4;
                            instr[i].Operand = Decrypt(Value); //statically decrypt
                            instr[i + 1].OpCode = OpCodes.Nop;

                            Deobfuscated++;
                        }
                    }
                }
            }

            return Deobfuscated;
        }
        public static bool Decrypt(int A_0)
        {
            bool result = true;
            checked
            {
                int num = (int)Math.Round((double)A_0 / 2.0);
                int num2 = 2;
                int num3 = num;
                for (int i = num2; i <= num3; i++)
                {
                    if (A_0 % i == 0)
                    {
                        result = false;
                    }
                }
                return result;
            }
        }
    }
}
