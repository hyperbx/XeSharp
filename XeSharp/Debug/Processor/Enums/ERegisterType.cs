namespace XeSharp.Debug.Processor
{
    [Flags]
    public enum ERegisterType
    {
        Unknown = 0,
        MSR     = 1,
        IAR     = 2,
        LR      = 4,
        CTR     = 8,
        GPR     = 16,
        CR      = 32,
        XER     = 64,
        FPSCR   = 128,
        FPR     = 256,
        VSCR    = 512,
        VR      = 1024,
        General = GPR | FPR | VR,
        Special = MSR | IAR | LR | CTR | CR | XER | FPSCR | VSCR,
        All     = MSR | IAR | LR | CTR | GPR | CR | XER | FPSCR | FPR | VSCR | VR
    }
}
