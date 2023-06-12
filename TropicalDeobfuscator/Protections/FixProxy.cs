using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TropicalDeobfuscator.Protections
{
    internal class FixProxy
    {
        public static ModuleDefMD Module = DeobfuscatorContext.Module;

        private static string GetProxyString(MethodDef method)
        {
            return method.Body.Instructions[0].Operand.ToString();
        }

        private static bool IsProxyString(MethodDef method)
        {
            return method.HasBody && method.Body.HasInstructions &&
                (method.ReturnType == Module.CorLibTypes.String
                && method.Body.Instructions.Count == 2)
                && method.Body.Instructions[0].OpCode == OpCodes.Ldstr;
        }

        public static int ProxyStrings()
        {
            int Deobfuscated = 0;
            foreach (TypeDef type in Module.GetTypes())
            {
                foreach (MethodDef method in type.Methods.ToArray<MethodDef>())
                {
                    if (method.HasBody && method.Body.HasInstructions)
                    {
                        IList<Instruction> instr = method.Body.Instructions;
                        for (int i = 0; i < instr.Count; i++)
                        {
                            if (instr[i].OpCode == OpCodes.Call)
                            {
                                Instruction instruction = instr[i];
                                if (instruction.Operand is MethodDef)
                                {
                                    MethodDef methodx = (MethodDef)instruction.Operand;
                                    if (IsProxyString(methodx))
                                    {
                                        string proxyString = GetProxyString(methodx);
                                        instruction.OpCode = OpCodes.Ldstr;
                                        instruction.Operand = proxyString;
                                        type.Remove(methodx);
                                        Deobfuscated++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Deobfuscated;
        }

        private static int GetProxyInt(MethodDef method)
        {
            return method.Body.Instructions[0].GetLdcI4Value();
        }

        private static bool IsProxyInt(MethodDef method)
        {
            return method.HasBody
                && method.Body.HasInstructions
                && (method.ReturnType == Module.CorLibTypes.Int32
                && method.Body.Instructions.Count == 2)
                && method.Body.Instructions[0].IsLdcI4();
        }

        public static int ProxyInt()
        {
            int Deobfuscated = 0;
            foreach (TypeDef type in Module.GetTypes())
            {
                foreach (MethodDef method in type.Methods.ToArray())
                {
                    if (!method.HasBody && !method.Body.HasInstructions)
                        continue;


                    var instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].OpCode == OpCodes.Call)
                        {
                            Instruction instruction = instr[i];
                            if (instruction.Operand is MethodDef)
                            {
                                MethodDef methodx = (MethodDef)instruction.Operand;
                                if (IsProxyInt(methodx))
                                {
                                    int proxyInt = GetProxyInt(methodx);
                                    instruction.OpCode = OpCodes.Ldc_I4;
                                    instruction.Operand = proxyInt;
                                    type.Remove(methodx);
                                    Deobfuscated++;
                                }
                            }
                        }
                    }

                }
            }
            return Deobfuscated;

        }


        private static float GetProxyFloat(MethodDef method)
        {
            return (float)method.Body.Instructions[0].Operand;
        }

        private static bool IsProxyFloat(MethodDef method)
        {
            return method.HasBody
                && method.Body.HasInstructions
                && (method.ReturnType == Module.CorLibTypes.Single
                && method.Body.Instructions.Count == 2)
                && method.Body.Instructions[0].OpCode == OpCodes.Ldc_R4;
        }

        public static int ProxyFloat()
        {
            int Deobfuscated = 0;
            foreach (TypeDef type in Module.GetTypes())
            {
                foreach (MethodDef method in type.Methods.ToArray())
                {
                    if (!method.HasBody && !method.Body.HasInstructions)
                        continue;


                    var instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].OpCode == OpCodes.Call)
                        {
                            Instruction instruction = instr[i];
                            if (instruction.Operand is MethodDef)
                            {
                                MethodDef methodx = (MethodDef)instruction.Operand;
                                if (IsProxyFloat(methodx))
                                {
                                    float proxyFloat = GetProxyFloat(methodx);
                                    instruction.OpCode = OpCodes.Ldc_R4;
                                    instruction.Operand = proxyFloat;
                                    type.Remove(methodx);
                                    Deobfuscated++;
                                }
                            }
                        }
                    }

                }
            }
            return Deobfuscated;

        }



        private static bool GetProxyBool(MethodDef method)
        {
            return (bool)method.Body.Instructions[0].Operand;
        }

        private static bool IsProxyBool(MethodDef method)
        {
            return method.HasBody
                && method.Body.HasInstructions
                && (method.ReturnType == Module.CorLibTypes.Boolean
                && method.Body.Instructions.Count == 2)
                && method.Body.Instructions[0].IsLdcI4();
        }

        public static int ProxyBool()
        {
            int Deobfuscated = 0;
            foreach (TypeDef type in Module.GetTypes())
            {
                foreach (MethodDef method in type.Methods.ToArray())
                {
                    if (!method.HasBody && !method.Body.HasInstructions)
                        continue;


                    var instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].OpCode == OpCodes.Call)
                        {
                            Instruction instruction = instr[i];
                            if (instruction.Operand is MethodDef)
                            {
                                MethodDef methodx = (MethodDef)instruction.Operand;
                                if (IsProxyBool(methodx))
                                {
                                    bool proxyValue = GetProxyBool(methodx);
                                    instruction.OpCode = OpCodes.Ldc_I4;
                                    instruction.Operand = proxyValue;
                                    type.Remove(methodx);
                                    Deobfuscated++;
                                }
                            }
                        }
                    }

                }
            }
            return Deobfuscated;

        }








        private static Instruction GetProxyNewObf(MethodDef method)
        {
            Instruction result;

            if (method.Body.Instructions[0].OpCode == OpCodes.Newobj)
            {
                result = new Instruction
                {
                    OpCode = OpCodes.Newobj,
                    Operand = method.Body.Instructions[0].Operand 
                };
                return result;
            }
            return null;
        }

        private static bool IsProxyNewObj(MethodDef method)
        {
            return method.HasBody
                && method.Body.HasInstructions
                && method.Body.Instructions.Count == 2
                && method.Body.Instructions[method.Body.Instructions.Count - 2].OpCode == OpCodes.Newobj;
        }

        public static int ProxyNewObj()
        {
            int Deobfuscated = 0;
            foreach (TypeDef type in Module.GetTypes())
            {
                foreach (MethodDef method in type.Methods.ToArray())
                {
                    if (!method.HasBody && !method.Body.HasInstructions)
                        continue;
                    var instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].OpCode == OpCodes.Call)
                        {
                            Instruction instruction = instr[i];
                            if (instruction.Operand is MethodDef)
                            {
                                MethodDef methodx = (MethodDef)instruction.Operand;
                                if (IsProxyNewObj(methodx))
                                {
                                    Instruction proxyNewObf = GetProxyNewObf(methodx);
                                    type.Remove(methodx);
                                    instruction.OpCode = proxyNewObf.OpCode;
                                    instruction.Operand = proxyNewObf.Operand;
                                    Deobfuscated++;
                                }
                            }
                        }
                    }

                }
            }
            return Deobfuscated;
        }
    }
}
