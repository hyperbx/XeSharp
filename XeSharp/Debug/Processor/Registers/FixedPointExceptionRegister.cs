using Amicitia.IO.Generics;
using Amicitia.IO;

// Reference: https://zenith.nsmbu.net/wiki/Custom_Code/PowerPC_Assembly_Cheatsheet/Special_Purpose_Registers#XER

namespace XeSharp.Debug.Processor.Registers
{
    public struct FixedPointExceptionRegister
    {
        /// <summary>
        /// Packed register.
        /// </summary>
        public uint Packed;

        /// <summary>
        /// Summary Overflow (SO) bit.
        /// <para>
        /// Set whenever an instruction (except mtspr) sets the OV bit.
        /// </para>
        /// <para>
        /// Once set, this bit remains set until it is cleared by an mtspr instruction (specifying the XER) or an mcrxr instruction.
        /// </para>
        /// <para>
        /// It is not altered by compare instructions, nor by other instructions (except mtspr to the XER, and mcrxr) that cannot overflow.
        /// </para>
        /// </summary>
        public BitField<uint, N0, N0> SO;

        /// <summary>
        /// OVerflow (OV) bit.
        /// <para>
        /// Set to indicate that a signed overflow has occurred during execution of an instruction.
        /// </para>
        /// <para>
        /// "Add", "Subtract from", and "Negate" instructions having OE = 1 (with an o suffix) will set the OV bit
        /// if the carry out of the MSB is not equal to the carry out of the (MSB + 1), and clear it otherwise.
        /// </para>
        /// <para>
        /// Multiply low and divide instructions having OE = 1 will set the OV bit if the result cannot be represented
        /// in 64 bits (mulld, divd, divdu) or in 32 bits (mullw, divw, divwu), and clear it otherwise.
        /// </para>
        /// <para>
        /// The OV bit is not altered by compare instructions that cannot overflow (except mtspr to the XER, and mcrxr).
        /// </para>
        /// </summary>
        public BitField<uint, N1, N1> OV;

        /// <summary>
        /// CArry (CA) bit.
        /// <para>
        /// Set during execution of the following instructions;
        /// </para>
        /// <para>
        /// - "Add carrying", "Subtract from carrying", "Add extended", and "Subtract from extended"
        /// instructions set CA if there is a carry out of the MSB, and clear it otherwise.
        /// </para>
        /// <para>
        /// - Shift right algebraic instructions set CA if any 1 bits have been shifted out of a negative operand, and clear it otherwise.
        /// The bit is not altered by compare instructions, nor by other instructions that cannot carry (except shift right algebraic, mtspr to the XER, and mcrxr).
        /// </para>
        /// </summary>
        public BitField<uint, N2, N2> CA;

        /// <summary>
        /// System reserved.
        /// </summary>
        public BitField<uint, N3, N24> XER24;

        /// <summary>
        /// This field specifies the number of bytes to be transferred by a Load String Word indeXed (lswx) or STore String Word indeXed (stswx) instruction.
        /// </summary>
        public BitField<uint, N25, N31> XER25;

        public FixedPointExceptionRegister(uint in_packedRegister)
        {
            Packed = in_packedRegister;

            SO.Packed = Packed;
            OV.Packed = Packed;
            CA.Packed = Packed;
            XER24.Packed = Packed;
            XER25.Packed = Packed;
        }

        public override string ToString()
        {
            var bits = new List<string>();

            if (SO.Get() != 0) bits.Add(nameof(SO));
            if (OV.Get() != 0) bits.Add(nameof(OV));
            if (CA.Get() != 0) bits.Add(nameof(CA));

            return bits.Count == 0
                ? "None"
                : string.Join(" | ", bits);
        }
    }
}
