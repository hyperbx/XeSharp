using Amicitia.IO.Generics;
using Amicitia.IO;

// Reference: https://zenith.nsmbu.net/wiki/Custom_Code/PowerPC_Assembly_Cheatsheet/Special_Purpose_Registers#CR

namespace XeSharp.Debug.Processor.Registers
{
    public struct ConditionRegister
    {
        /// <summary>
        /// Packed register.
        /// </summary>
        public uint Packed;

        /// <summary>
        /// Less Than (LT) bit - the last integer operation returned a negative result.
        /// </summary>
        public BitField<uint, N0, N0> LT;

        /// <summary>
        /// Greater Than (GT) bit - the last integer operation returned a positive and non-zero result.
        /// </summary>
        public BitField<uint, N1, N1> GT;

        /// <summary>
        /// EQual (EQ) bit - the last integer operation returned zero.
        /// </summary>
        public BitField<uint, N2, N2> EQ;

        /// <summary>
        /// Summary Overflow (SO) bit - a copy of the XERSO register after the last integer operation.
        /// </summary>
        public BitField<uint, N3, N3> SO;

        /// <summary>
        /// Floating point eXception summary (FX) bit - a copy of the FPSCRFX register after the last floating point operation.
        /// </summary>
        public BitField<uint, N4, N4> FX;

        /// <summary>
        /// Floating point Enabled eXception summary (FEX) bit - a copy of the FPSCRFEX register after the last floating point operation.
        /// </summary>
        public BitField<uint, N5, N5> FEX;

        /// <summary>
        /// inValid operation eXception summary (VX) bit - a copy of the FPSCRVX register after the last floating point operation.
        /// </summary>
        public BitField<uint, N6, N6> VX;

        /// <summary>
        /// Overflow eXception (OX) bit - a copy of the FPSCROX register after the last floating point operation.
        /// </summary>
        public BitField<uint, N7, N7> OX;

        /// <summary>
        /// Expression integer Less Than (ExprLT) bit - integer A is less than integer B.
        /// </summary>
        public BitField<uint, N8, N8> ExprLT;

        /// <summary>
        /// Expression Floating point Less Than (ExprFLT) bit - float A is less than float B.
        /// </summary>
        public BitField<uint, N9, N9> ExprFLT;

        /// <summary>
        /// Expression integer Greater Than (ExprGT) bit - integer A is greater than integer B.
        /// </summary>
        public BitField<uint, N10, N10> ExprGT;

        /// <summary>
        /// Expression Floating point Greater Than (ExprFGT) bit - float A is greater than float B.
        /// </summary>
        public BitField<uint, N11, N11> ExprFGT;

        /// <summary>
        /// Expression integer EQual (ExprEQ) bit - integer A is equal to integer B.
        /// </summary>
        public BitField<uint, N12, N12> ExprEQ;

        /// <summary>
        /// Expression Floating point EQual (ExprFEQ) bit - float A is equal to float B.
        /// </summary>
        public BitField<uint, N13, N13> ExprFEQ;

        /// <summary>
        /// Expression Summary Overflow (ExprSO) bit - a copy of the XERSO register after the last integer expression operation.
        /// </summary>
        public BitField<uint, N14, N14> ExprSO;

        /// <summary>
        /// Float Unordered (FU) bit - either of the two floats being compared were <see cref="float.NaN"/> (not a number).
        /// </summary>
        public BitField<uint, N15, N15> FU;

        public ConditionRegister(uint in_packedRegister)
        {
            Packed = in_packedRegister;

            LT.Packed = Packed;
            GT.Packed = Packed;
            EQ.Packed = Packed;
            SO.Packed = Packed;
            FX.Packed = Packed;
            FEX.Packed = Packed;
            VX.Packed = Packed;
            OX.Packed = Packed;
            ExprLT.Packed = Packed;
            ExprFLT.Packed = Packed;
            ExprGT.Packed = Packed;
            ExprFGT.Packed = Packed;
            ExprEQ.Packed = Packed;
            ExprFEQ.Packed = Packed;
            ExprSO.Packed = Packed;
            FU.Packed = Packed;
        }

        public override string ToString()
        {
            var bits = new List<string>();

            if (LT.Get() != 0)      bits.Add(nameof(LT));
            if (GT.Get() != 0)      bits.Add(nameof(GT));
            if (EQ.Get() != 0)      bits.Add(nameof(EQ));
            if (SO.Get() != 0)      bits.Add(nameof(SO));
            if (FX.Get() != 0)      bits.Add(nameof(FX));
            if (FEX.Get() != 0)     bits.Add(nameof(FEX));
            if (VX.Get() != 0)      bits.Add(nameof(VX));
            if (OX.Get() != 0)      bits.Add(nameof(OX));
            if (ExprLT.Get() != 0)  bits.Add(nameof(ExprLT));
            if (ExprFLT.Get() != 0) bits.Add(nameof(ExprFLT));
            if (ExprGT.Get() != 0)  bits.Add(nameof(ExprGT));
            if (ExprFGT.Get() != 0) bits.Add(nameof(ExprFGT));
            if (ExprEQ.Get() != 0)  bits.Add(nameof(ExprEQ));
            if (ExprFEQ.Get() != 0) bits.Add(nameof(ExprFEQ));
            if (ExprSO.Get() != 0)  bits.Add(nameof(ExprSO));
            if (FU.Get() != 0)      bits.Add(nameof(FU));

            return bits.Count == 0
                ? "None"
                : string.Join(" | ", bits);
        }
    }
}
