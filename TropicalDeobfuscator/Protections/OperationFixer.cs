using de4dot.blocks;
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TropicalDeobfuscator.Protections
{
    internal class OperationFixer
    {
        public static int Fix()
        {
            int Deobfuscated = 0;

            var Module = DeobfuscatorContext.Module;
            foreach (TypeDef TypeDef in Module.GetTypes())
            {
                foreach (MethodDef Method in TypeDef.Methods)
                {
                    if (!Method.HasBody && !Method.Body.HasInstructions)
                        continue;

                    var instr = Method.Body.Instructions;
                    Method.RemoveUnusedNops();
                    if (Method.DeclaringType2.Name.Contains("siticoneButton14") && Method.Name.Contains("MoveNext"))
                        continue;

                    Cleaner(Method);
                    Deobfuscated++;
                }
            }

            return Deobfuscated;
        }
        public static int FixAirthmethic() //Add new check
        {
            int Deobfuscated = 0;

            var Module = DeobfuscatorContext.Module;
            foreach (TypeDef TypeDef in Module.GetTypes())
            {
                foreach (MethodDef Method in TypeDef.Methods)
                {
                    if (!Method.HasBody && !Method.Body.HasInstructions)
                        continue;

                    var instr = Method.Body.Instructions;
                    Method.RemoveUnusedNops();

                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].IsLdcI4() && instr[i + 1].IsLdcI4() && instr[i + 2].isArithmeticOperation())
                        {
                            int value1 = instr[i].GetLdcI4Value();
                            int value2 = instr[i + 1].GetLdcI4Value();
                            int finalval = 0;
                            var code = instr[i + 2].OpCode.Code;
                            try
                            {
                                switch (code)
                                {

                                    case Code.Div:
                                        finalval = value1 / value2;
                                        break;
                                    case Code.Add:
                                        finalval = value1 + value2;
                                        break;
                                    case Code.Sub:
                                        finalval = value1 - value2;
                                        break;
                                }
                            }
                            catch { 
                            }

                            if (finalval != 0)
                            {
                                instr[i].OpCode = OpCodes.Ldc_I4;
                                instr[i].Operand = finalval;
                                instr[i + 1].OpCode = OpCodes.Nop;
                                instr[i + 2].OpCode = OpCodes.Nop;
                            }

                        }
                    }


                    Deobfuscated++;
                }
            }

            return Deobfuscated;
        }
        public static void Cleaner(MethodDef method)
        {


            BlocksCflowDeobfuscator blocksCflowDeobfuscator = new BlocksCflowDeobfuscator();
            Blocks blocks = new Blocks(method);

            List<Block> test = blocks.MethodBlocks.GetAllBlocks();
            blocks.Method.Body.SimplifyBranches();
            blocks.Method.Body.OptimizeBranches();
            blocks.RemoveDeadBlocks();
            blocks.RepartitionBlocks();
            blocks.UpdateBlocks();

            blocksCflowDeobfuscator.Initialize(blocks);
            blocksCflowDeobfuscator.Deobfuscate();
            blocks.RepartitionBlocks();
            IList<Instruction> instructions;
            IList<ExceptionHandler> exceptionHandlers;
            blocks.GetCode(out instructions, out exceptionHandlers);
            DotNetUtils.RestoreBody(method, instructions, exceptionHandlers);

        }
    }

    public static class Extensions
    {
        static List<Code> ArithmeticOpCodes = new List<Code>
        {
            Code.Add,
            Code.Div,
            Code.Sub,
            Code.Mul,
        };

        public static bool isArithmeticOperation(this Instruction instruction)
        {
            if (ArithmeticOpCodes.Contains(instruction.OpCode.Code))
                return true;
            return false;
        }

        public static void RemoveUnusedNops(this MethodDef MethodDef)
        {
            if (MethodDef.HasBody)
            {
                for (int i = 0; i < MethodDef.Body.Instructions.Count; i++)
                {
                    Instruction instruction = MethodDef.Body.Instructions[i];
                    if (instruction.OpCode == OpCodes.Nop)
                    {
                        if (!IsNopBranchTarget(MethodDef, instruction))
                        {
                            if (!IsNopSwitchTarget(MethodDef, instruction))
                            {
                                if (!IsNopExceptionHandlerTarget(MethodDef, instruction))
                                {
                                    MethodDef.Body.Instructions.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IsNopBranchTarget(MethodDef MethodDef, Instruction NopInstr)
        {
            for (int i = 0; i < MethodDef.Body.Instructions.Count; i++)
            {
                Instruction instruction = MethodDef.Body.Instructions[i];
                if (instruction.OpCode.OperandType == OperandType.InlineBrTarget || instruction.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                {
                    if (instruction.Operand != null)
                    {
                        Instruction instruction2 = (Instruction)instruction.Operand;
                        if (instruction2 == NopInstr)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsNopSwitchTarget(MethodDef MethodDef, Instruction NopInstr)
        {
            for (int i = 0; i < MethodDef.Body.Instructions.Count; i++)
            {
                Instruction instruction = MethodDef.Body.Instructions[i];
                if (instruction.OpCode.OperandType == OperandType.InlineSwitch)
                {
                    if (instruction.Operand != null)
                    {
                        Instruction[] source = (Instruction[])instruction.Operand;
                        if (source.Contains(NopInstr))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsNopExceptionHandlerTarget(MethodDef MethodDef, Instruction NopInstr)
        {
            bool result;
            if (!MethodDef.Body.HasExceptionHandlers)
            {
                result = false;
            }
            else
            {
                IList<ExceptionHandler> exceptionHandlers = MethodDef.Body.ExceptionHandlers;
                foreach (ExceptionHandler exceptionHandler in exceptionHandlers)
                {
                    if (exceptionHandler.FilterStart == NopInstr)
                    {
                        return true;
                    }
                    if (exceptionHandler.HandlerEnd == NopInstr)
                    {
                        return true;
                    }
                    if (exceptionHandler.HandlerStart == NopInstr)
                    {
                        return true;
                    }
                    if (exceptionHandler.TryEnd == NopInstr)
                    {
                        return true;
                    }
                    if (exceptionHandler.TryStart == NopInstr)
                    {
                        return true;
                    }
                }
                result = false;
            }
            return result;
        }
    }
}
