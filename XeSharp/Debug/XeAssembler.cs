using Keystone;

namespace XeSharp.Debug
{
    public class XeAssembler
    {
        public static Architecture Architecture => Architecture.PPC;
        public static Mode Mode => Mode.BIG_ENDIAN | Mode.PPC64;

        /// <summary>
        /// Assembles instructions into bytecode.
        /// </summary>
        /// <param name="in_asm">The instructions to assemble.</param>
        /// <param name="in_addr">The address for context for the assembler.</param>
        public static byte[] Assemble(string in_asm, uint in_addr = 0)
        {
            using (var engine = new Engine(Architecture, Mode) { ThrowOnError = true })
                return engine.Assemble(in_asm, in_addr).Buffer;
        }
    }
}
