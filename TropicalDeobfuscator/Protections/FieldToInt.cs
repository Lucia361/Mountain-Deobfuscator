using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TropicalDeobfuscator.Protections
{
    internal class FieldToInt
    {
        public static int Fix()
        {
            int Deobfuscated = 0;

            var Module = DeobfuscatorContext.Module;
            var ReflectionAssembly = DeobfuscatorContext.ReflectionAssembly;

            foreach (TypeDef Type in Module.GetTypes())
            {
                foreach(MethodDef Method in Type.Methods)
                {
                    if (!Method.HasBody && !Method.Body.HasInstructions)
                        continue;

                    var instr = Method.Body.Instructions;

                    for(int i = 0; i < instr.Count; i++)
                    {
                        if(instr[i].OpCode == OpCodes.Ldsfld && instr[i + 1].IsLdcI4() && instr[i + 2].OpCode == OpCodes.Ldelem_I4)
                        {
                            FieldDef IntField = instr[i].Operand as FieldDef;
                            int FieldToken = IntField.MDToken.ToInt32();

                            int[] value = (int[])ReflectionAssembly.ManifestModule.ResolveField(FieldToken).GetValue(null); //use reflection because i suck at emulation
                            int Index = instr[i + 1].GetLdcI4Value();

                            object realValue = value[Index]; 


                            instr[i].OpCode = OpCodes.Ldc_I4;
                            instr[i].Operand = realValue;

                        

                            instr[i + 1].OpCode = OpCodes.Nop;
                            instr[i + 2].OpCode = OpCodes.Nop;


                            
                            Type.Fields.Remove(IntField); //remove junk

                            //clear field.cctor
                            


                            Deobfuscated++;
                        }

                        if(Method.Name == ".cctor") //clear field initialization cctor
                        {
                            if(instr[i].IsLdcI4() && instr[i + 1].OpCode == OpCodes.Newarr && instr[i + 2].OpCode == OpCodes.Dup && instr[i + 3].OpCode == OpCodes.Ldtoken && instr[i + 4].OpCode == OpCodes.Call && instr[i + 5].OpCode == OpCodes.Stsfld)
                            {
                                Method.Body.Instructions.Clear();
                                Method.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                            }
                        }

                    }
                }
            }

            return Deobfuscated;
        }
    }
}
