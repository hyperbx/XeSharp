using Amicitia.IO;
using Amicitia.IO.Generics;

// Reference: https://zenith.nsmbu.net/wiki/Custom_Code/PowerPC_Assembly_Cheatsheet/Special_Purpose_Registers#FPSCR

namespace XeSharp.Debug.Processor.Registers
{
    public struct FloatingPointStatusControlRegister
    {
        /// <summary>
        /// Packed register.
        /// </summary>
        public ulong Packed;

        /// <summary>
        /// Floating point eXception summary (FX) bit.
        /// </summary>
        public BitField<ulong, N0, N0> FX;

        /// <summary>
        /// Floating point Enabled eXception summary (FEX) bit.
        /// </summary>
        public BitField<ulong, N1, N1> FEX;

        /// <summary>
        /// inValid operation eXception summary (VX) bit.
        /// </summary>
        public BitField<ulong, N2, N2> VX;

        /// <summary>
        /// Overflow eXception (OX) bit.
        /// </summary>
        public BitField<ulong, N3, N3> OX;

        /// <summary>
        /// Underflow eXception (UX) bit.
        /// </summary>
        public BitField<ulong, N4, N4> UX;

        /// <summary>
        /// Zero-divide eXception (ZX) bit.
        /// </summary>
        public BitField<ulong, N5, N5> ZX;

        /// <summary>
        /// ineXact eXception (XX) bit.
        /// </summary>
        public BitField<ulong, N6, N6> XX;

        /// <summary>
        /// inValid operation eXception for "SNaN" (VXSNAN) bit.
        /// </summary>
        public BitField<ulong, N7, N7> VXSNAN;

        /// <summary>
        /// inValid operation eXception for Infinity Subtracted by Infinity (VXISI) bit.
        /// </summary>
        public BitField<ulong, N8, N8> VXISI;

        /// <summary>
        /// inValid operation eXception for Infinity Divided by Infinity (VXIDI) bit.
        /// </summary>
        public BitField<ulong, N9, N9> VXIDI;

        /// <summary>
        /// inValid operation eXception for Zero Divided by Zero (VXZDZ) bit.
        /// </summary>
        public BitField<ulong, N10, N10> VXZDZ;

        /// <summary>
        /// inValid operation eXception for Infinity Multiplied by Zero (VXIMZ) bit.
        /// </summary>
        public BitField<ulong, N11, N11> VXIMZ;

        /// <summary>
        /// inValid operation eXception for inValid Comparison (VXVC) bit.
        /// </summary>
        public BitField<ulong, N12, N12> VXVC;

        /// <summary>
        /// Fraction Rounded (FR) bit.
        /// </summary>
        public BitField<ulong, N13, N13> FR;

        /// <summary>
        /// Fraction Inexact (FI) bit.
        /// </summary>
        public BitField<ulong, N14, N14> FI;

        /// <summary>
        /// Floating Point Result Flags (FPRF) bits.
        /// </summary>
        public BitField<ulong, N15, N19> FPRF;

        /// <summary>
        /// System reserved.
        /// </summary>
        public BitField<ulong, N20, N20> FPSCR20;

        /// <summary>
        /// inValid operation eXception for SOFTware request (VXSOFT) bit.
        /// </summary>
        public BitField<ulong, N21, N21> VXSOFT;

        /// <summary>
        /// inValid operation eXception for invalid SQuare RooT (VXSQRT) bit.
        /// </summary>
        public BitField<ulong, N22, N22> VXSQRT;

        /// <summary>
        /// inValid operation eXception for invalid ConVerted Integer (VXCVI) bit.
        /// </summary>
        public BitField<ulong, N23, N23> VXCVI;

        /// <summary>
        /// inValid operation exception Enable (VE) bit.
        /// </summary>
        public BitField<ulong, N24, N24> VE;

        /// <summary>
        /// Overflow exception Enable (OE) bit.
        /// </summary>
        public BitField<ulong, N25, N25> OE;

        /// <summary>
        /// Underflow exception Enable (UE) bit.
        /// </summary>
        public BitField<ulong, N26, N26> UE;

        /// <summary>
        /// Zero-division exception Enable (ZE) bit.
        /// </summary>
        public BitField<ulong, N27, N27> ZE;

        /// <summary>
        /// ineXact exception Enable (XE) bit.
        /// </summary>
        public BitField<ulong, N28, N28> XE;

        /// <summary>
        /// Non-IEEE mode (NI) bit.
        /// </summary>
        public BitField<ulong, N29, N29> NI;

        /// <summary>
        /// RouNding control (RN) bits.
        /// </summary>
        public BitField<ulong, N30, N31> RN;

        public FloatingPointStatusControlRegister(ulong in_packedRegister)
        {
            Packed = in_packedRegister;

            FX.Packed = Packed;
            FEX.Packed = Packed;
            VX.Packed = Packed;
            OX.Packed = Packed;
            UX.Packed = Packed;
            ZX.Packed = Packed;
            XX.Packed = Packed;
            VXSNAN.Packed = Packed;
            VXISI.Packed = Packed;
            VXIDI.Packed = Packed;
            VXZDZ.Packed = Packed;
            VXIMZ.Packed = Packed;
            VXVC.Packed = Packed;
            FR.Packed = Packed;
            FI.Packed = Packed;
            FPRF.Packed = Packed;
            FPSCR20.Packed = Packed;
            VXSOFT.Packed = Packed;
            VXSQRT.Packed = Packed;
            VXCVI.Packed = Packed;
            VE.Packed = Packed;
            OE.Packed = Packed;
            UE.Packed = Packed;
            ZE.Packed = Packed;
            XE.Packed = Packed;
            NI.Packed = Packed;
            RN.Packed = Packed;
        }

        public override string ToString()
        {
            var bits = new List<string>();

            if (FX.Get() != 0)      bits.Add(nameof(FX));
            if (FEX.Get() != 0)     bits.Add(nameof(FEX));
            if (VX.Get() != 0)      bits.Add(nameof(VX));
            if (OX.Get() != 0)      bits.Add(nameof(OX));
            if (UX.Get() != 0)      bits.Add(nameof(UX));
            if (ZX.Get() != 0)      bits.Add(nameof(ZX));
            if (XX.Get() != 0)      bits.Add(nameof(XX));
            if (VXSNAN.Get() != 0)  bits.Add(nameof(VXSNAN));
            if (VXISI.Get() != 0)   bits.Add(nameof(VXISI));
            if (VXIDI.Get() != 0)   bits.Add(nameof(VXIDI));
            if (VXZDZ.Get() != 0)   bits.Add(nameof(VXZDZ));
            if (VXIMZ.Get() != 0)   bits.Add(nameof(VXIMZ));
            if (VXVC.Get() != 0)    bits.Add(nameof(VXVC));
            if (FR.Get() != 0)      bits.Add(nameof(FR));
            if (FI.Get() != 0)      bits.Add(nameof(FI));
            if (FPRF.Get() != 0)    bits.Add(nameof(FPRF));
            if (FPSCR20.Get() != 0) bits.Add(nameof(FPSCR20));
            if (VXSOFT.Get() != 0)  bits.Add(nameof(VXSOFT));
            if (VXSQRT.Get() != 0)  bits.Add(nameof(VXSQRT));
            if (VXCVI.Get() != 0)   bits.Add(nameof(VXCVI));
            if (VE.Get() != 0)      bits.Add(nameof(VE));
            if (OE.Get() != 0)      bits.Add(nameof(OE));
            if (UE.Get() != 0)      bits.Add(nameof(UE));
            if (ZE.Get() != 0)      bits.Add(nameof(ZE));
            if (XE.Get() != 0)      bits.Add(nameof(XE));
            if (NI.Get() != 0)      bits.Add(nameof(NI));
            if (RN.Get() != 0)      bits.Add(nameof(RN));

            return bits.Count == 0
                ? "None"
                : string.Join(" | ", bits);
        }
    }
}
