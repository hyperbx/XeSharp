using Gee.External.Capstone;
using Gee.External.Capstone.PowerPc;
using XeSharp.Debug.Processor;
using XeSharp.Device;
using XeSharp.Helpers;

namespace XeSharp.Debug
{
    public static class XeDisassembler
    {
        private static readonly List<string> _conditionalBranchMnemonics =
        [
            "bc", "bca", "bcl", "bcla", "bclr", "bclrl", "bcctr", "bcctrl",
            "beq", "bne", "bgt", "blt", "bge", "ble", "bng", "bnl",
            "bso", "bns", "bun", "bnu", "bdnz", "bdnzt", "bdnzf", "bdz"
        ];

        /// <summary>
        /// The architecture for the disassembler.
        /// </summary>
        public static PowerPcDisassembleMode Architecture => PowerPcDisassembleMode.BigEndian | PowerPcDisassembleMode.Bit64;

        /// <summary>
        /// Disassembles a single instruction.
        /// </summary>
        /// <param name="in_byteCode">The instruction's byte code.</param>
        public static PowerPcInstruction Disassemble(uint in_byteCode)
        {
            return CapstoneDisassembler.CreatePowerPcDisassembler(Architecture)
                .Disassemble(MemoryHelper.UnmanagedTypeToByteArray(in_byteCode)).First();
        }

        /// <summary>
        /// Disassembles a single instruction from an address on the remote console.
        /// </summary>
        /// <param name="in_console">The console to read the instruction from.</param>
        /// <param name="in_addr">The address of the instruction.</param>
        public static PowerPcInstruction Disassemble(XeConsole in_console, uint in_addr)
        {
            return Disassemble(in_console.Memory.Read<uint>(in_addr));
        }

        /// <summary>
        /// Disassembles instructions from an address on the remote console.
        /// </summary>
        /// <param name="in_console">The console to read the instruction from.</param>
        /// <param name="in_addr">The address of the instruction.</param>
        /// <param name="in_count">The number of instructions to disassemble.</param>
        public static PowerPcInstruction[] Disassemble(XeConsole in_console, uint in_addr, uint in_count)
        {
            return CapstoneDisassembler.CreatePowerPcDisassembler(Architecture)
                .Disassemble(in_console.Memory.ReadBytes(in_addr, in_count * 4), in_addr);
        }

        /// <summary>
        /// Determines whether this instruction is a branch instruction.
        /// </summary>
        /// <param name="in_instr">The instruction to check.</param>
        public static bool IsBranch(this PowerPcInstruction in_instr)
        {
            return in_instr.Mnemonic.StartsWith('b');
        }

        /// <summary>
        /// Determines whether this branch instruction is conditional.
        /// </summary>
        /// <param name="in_instr">The branch instruction.</param>
        public static bool IsConditionalBranch(this PowerPcInstruction in_instr)
        {
            return _conditionalBranchMnemonics.Contains(in_instr.Mnemonic);
        }

        /// <summary>
        /// Gets the destination address of a branch instruction.
        /// </summary>
        /// <param name="in_instr">The branch instruction.</param>
        /// <param name="in_processor">The processor at the branch instruction.</param>
        public static uint GetBranchAddress(this PowerPcInstruction in_instr, XeProcessor in_processor)
        {
            if (!in_instr.IsBranch())
                return 0;

            // Return link register address.
            if (in_instr.Mnemonic == "blr")
                return in_processor.LR;

            var addr = MemoryHelper.ChangeType<uint>(in_instr.Operand);
            var isAbsolute = in_instr.Mnemonic.EndsWith('a');

            if (isAbsolute)
                return addr;

            // TODO: is 0x1000 arbitrary?
            return in_processor.IAR + (addr - 0x1000);
        }

        /// <summary>
        /// Determines whether the branch instruction should succeed.
        /// </summary>
        /// <param name="in_instr">The branch instruction.</param>
        /// <param name="in_processor">The processor with the result of the expression to test.</param>
        public static bool IsBranchLegal(this PowerPcInstruction in_instr, XeProcessor in_processor)
        {
            if (!in_instr.IsBranch())
                return false;

            if (!in_instr.IsConditionalBranch())
                return true;

            return in_instr.Mnemonic switch
            {
                "beq" => in_processor.CR.EQ.Get() == 1,
                "bne" => in_processor.CR.EQ.Get() == 0,
                "bgt" => in_processor.CR.GT.Get() == 1,
                "blt" => in_processor.CR.LT.Get() == 1,
                "bge" => in_processor.CR.GT.Get() == 1 || in_processor.CR.EQ.Get() == 1,
                "ble" => in_processor.CR.LT.Get() == 1 || in_processor.CR.EQ.Get() == 1,
                "bng" => in_processor.CR.GT.Get() == 0,
                "bnl" => in_processor.CR.LT.Get() == 0,
                "bso" => in_processor.CR.SO.Get() == 1,
                "bns" => in_processor.CR.SO.Get() == 0,
                "bun" => in_processor.CR.FU.Get() == 1,
                "bnu" => in_processor.CR.FU.Get() == 0,
                _     => false,
            };
        }
    }
}
