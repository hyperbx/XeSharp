using System.ComponentModel;
using System.Numerics;
using XeSharp.Collections;
using XeSharp.Debug.Processor.Registers;
using XeSharp.Device;
using XeSharp.Device.Title;
using XeSharp.Exceptions;
using XeSharp.Helpers;
using XeSharp.Net;
using XeSharp.Serialisation.INI;

// Reference: https://zenith.nsmbu.net/wiki/Custom_Code/PowerPC_Assembly_Cheatsheet

namespace XeSharp.Debug.Processor
{
    public class XeProcessor : INotifyPropertyChanged
    {
        protected XeConsole _console;
        protected XeThreadInfo _thread;

        /// <summary>
        /// Determines whether registers will be committed to the remote console upon changing them.
        /// </summary>
        public bool IsCommitOnChange { get; set; }

        /// <summary>
        /// Machine state register - stores information regarding the CPU and its current state.
        /// </summary>
        public uint MSR { get; set; }

        /// <summary>
        /// Instruction address register - stores the virtual address of the current instruction.
        /// </summary>
        public uint IAR { get; set; }

        /// <summary>
        /// Link register - stores the return address for the next branch instruction.
        /// </summary>
        public uint LR { get; set; }

        /// <summary>
        /// Count register - stores the current iteration of a loop or a pointer to a virtual function.
        /// </summary>
        public ulong CTR { get; set; }

        /// <summary>
        /// General purpose registers.
        /// <para>For more information, see the <a href="https://zenith.nsmbu.net/wiki/Custom_Code/PowerPC_Assembly_Cheatsheet">PowerPC Assembly Cheetsheet</a>.</para>
        /// </summary>
        public RegisterSet<ulong> GPR { get; set; } = new(32);

        /// <summary>
        /// Condition register - stores conditional results for previous instructions.
        /// </summary>
        public ConditionRegister CR { get; set; } = new(0);

        /// <summary>
        /// Fixed point exception register - stores information regarding erroneous operations.
        /// </summary>
        public FixedPointExceptionRegister XER { get; set; } = new(0);

        /// <summary>
        /// Floating point status and control register - stores exception and rounding control information for floating point arithmetic.
        /// </summary>
        public FloatingPointStatusControlRegister FPSCR { get; set; } = new(0);

        /// <summary>
        /// Floating point registers.
        /// <para>For more information, see the <a href="https://zenith.nsmbu.net/wiki/Custom_Code/PowerPC_Assembly_Cheatsheet">PowerPC Assembly Cheetsheet</a>.</para>
        /// </summary>
        public RegisterSet<double> FPR { get; set; } = new(32);

        /// <summary>
        /// Vector status and control register (VMX128).
        /// </summary>
        public Vector4 VSCR { get; set; }

        /// <summary>
        /// Vector registers (VMX128).
        /// </summary>
        public RegisterSet<Vector4> VR { get; set; } = new(128);

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Creates a snapshot of the current processor state on the remote console.
        /// </summary>
        /// <param name="in_console">The console to get CPU information from.</param>
        /// <param name="in_thread">The thread to get CPU information from.</param>
        /// <param name="in_isCommitOnChange">Determines whether registers will be committed to the remote console upon changing them.</param>
        public XeProcessor(XeConsole in_console, XeThreadInfo in_thread, bool in_isCommitOnChange = true)
        {
            _console = in_console;
            _thread = in_thread;

            var response = _console.Client.SendCommand($"getcontext thread=0x{in_thread.ID:X} full", false);

            if (response.Status.ToHResult() == EXeStatusCode.XBDM_NOTHREAD)
                throw new ThreadNotFoundException(in_thread.ID);

            if (response.Status.ToHResult() != EXeStatusCode.XBDM_MULTIRESPONSE)
                throw new InvalidDataException("Failed to obtain processor context.");

            Parse(response.Results as string[]);

            if (in_isCommitOnChange)
            {
                GPR.RegisterChanged += (s, e) => CommitGPR(e.Index);
                FPR.RegisterChanged += (s, e) => CommitFPR(e.Index);
                VR .RegisterChanged += (s, e) => CommitVR (e.Index);
            }

            IsCommitOnChange = in_isCommitOnChange;
        }

        /// <summary>
        /// Creates a snapshot of the current processor state on the remote console.
        /// </summary>
        /// <param name="in_console">The console to get CPU information from.</param>
        /// <param name="in_threadID">The thread ID to get CPU information from.</param>
        /// <param name="in_isCommitOnChange">Determines whether registers will be committed to the remote console upon changing them.</param>
        public XeProcessor(XeConsole in_console, int in_threadID, bool in_isCommitOnChange = true)
            : this(in_console, new XeThreadInfo(in_console, in_threadID), in_isCommitOnChange) { }

        private void Parse(string[] in_ini)
        {
            var ini = IniParser.DoLines(in_ini);

            MSR = MemoryHelper.ChangeType<uint>(ini[""]["Msr"]);
            IAR = MemoryHelper.ChangeType<uint>(ini[""]["Iar"]);
            LR  = MemoryHelper.ChangeType<uint>(ini[""]["Lr"]);
            CTR = MemoryHelper.ChangeType<ulong>(ini[""]["Ctr"]);

            for (int i = 0; i < GPR.Length; i++)
                GPR[i] = MemoryHelper.ChangeType<ulong>(ini[""][$"Gpr{i}"]);

            CR  = new ConditionRegister(MemoryHelper.ChangeType<uint>(ini[""]["Cr"]));
            XER = new FixedPointExceptionRegister(MemoryHelper.ChangeType<uint>(ini[""]["Xer"]));

            if (ini[""].TryGetValue("Fpscr", out string? out_fpscr))
            {
                FPSCR = new FloatingPointStatusControlRegister(MemoryHelper.ChangeType<ulong>(out_fpscr));

                for (int i = 0; i < FPR.Length; i++)
                    FPR[i] = MemoryHelper.ChangeType<double>(ini[""][$"Fpr{i}"]);
            }

            if (ini[""].TryGetValue("Vscr", out string? out_vscr))
            {
                VSCR = MemoryHelper.HexStringToVector4(out_vscr);

                for (int i = 0; i < VR.Length; i++)
                    VR[i] = MemoryHelper.HexStringToVector4(ini[""][$"Vr{i}"], true);
            }
        }

        /// <summary>
        /// Gets a formatted representation of all registers.
        /// </summary>
        public string GetRegisterInfo()
        {
            var info = $"MSR ─── : 0x{MSR:X8}\n" +
                       $"IAR ─── : 0x{IAR:X8}\n" +
                       $"LR ──── : 0x{LR:X8}\n" +
                       $"CTR ─── : 0x{CTR:X16}";

            static string GetLine(int in_index)
            {
                if (in_index < 10)
                    return "──";
                else if (in_index >= 10 && in_index < 100)
                    return "─";

                return "";
            }

            for (int i = 0; i < GPR.Length; i++)
                info += $"\nGPR{i} {GetLine(i)} : 0x{GPR[i]:X16}";

            info += $"\nCR ──── : 0x{CR.Packed:X8} ({CR})";
            info += $"\nXER ─── : 0x{XER.Packed:X8} ({XER})";
            info += $"\nFPSCR ─ : 0x{FPSCR.Packed:X16} ({FPSCR})";

            for (int i = 0; i < FPR.Length; i++)
                info += $"\nFPR{i} {GetLine(i)} : 0x{BitConverter.DoubleToUInt64Bits(FPR[i]):X16} ({FPR[i]})";

            info += $"\nVSCR ── : <{MemoryHelper.Vector4ToHexString(VSCR)}>";

            for (int i = 0; i < VR.Length; i++)
                info += $"\nVR{i} ─{GetLine(i)} : <{MemoryHelper.Vector4ToHexString(VR[i])}> ({VR[i]})";

            return info;
        }

        #region Register Committing

        /// <summary>
        /// Commits all registers to the remote console.
        /// </summary>
        public void CommitAll()
        {
            CommitSpecialRegisters();
            CommitGeneralRegisters();
        }

        /// <summary>
        /// Commits all special purpose registers to the remote console.
        /// <para>Registers: <see cref="MSR"/>, <see cref="IAR"/>, <see cref="LR"/>, <see cref="CTR"/>, <see cref="CR"/>, <see cref="XER"/>, <see cref="FPSCR"/> and <see cref="VSCR"/>.</para>
        /// </summary>
        public void CommitSpecialRegisters()
        {
            CommitMSR();
            CommitIAR();
            CommitLR();
            CommitCTR();
            CommitCR();
            CommitXER();
            CommitFPSCR();
            CommitVSCR();
        }

        /// <summary>
        /// Commits all general purpose registers to the remote console.
        /// <para>Registers: <see cref="GPR"/>0-31, <see cref="FPR"/>0-31 and <see cref="VR"/>0-127.</para>
        /// </summary>
        public void CommitGeneralRegisters()
        {
            CommitGPRs();
            CommitFPRs();
            CommitVRs();
        }

        /// <summary>
        /// Commits the <see cref="MSR"/> register to the remote console.
        /// </summary>
        public void CommitMSR()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Changed MSR to 0x{MSR:X8}");
#endif

            _console.Client.SendCommand($"setcontext thread=0x{_thread.ID:X8} Msr=0x{MSR:X8}");
        }

        /// <summary>
        /// Commits the <see cref="IAR"/> register to the remote console.
        /// </summary>
        public void CommitIAR()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Changed IAR to 0x{IAR:X8}");
#endif

            _console.Client.SendCommand($"setcontext thread=0x{_thread.ID:X8} Iar=0x{IAR:X8}");
        }

        /// <summary>
        /// Commits the <see cref="LR"/> register to the remote console.
        /// </summary>
        public void CommitLR()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Changed LR to 0x{LR:X8}");
#endif

            _console.Client.SendCommand($"setcontext thread=0x{_thread.ID:X8} Lr=0x{LR:X8}");
        }

        /// <summary>
        /// Commits the <see cref="CTR"/> register to the remote console.
        /// </summary>
        public void CommitCTR()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Changed CTR to 0x{CTR:X16}");
#endif

            _console.Client.SendCommand($"setcontext thread=0x{_thread.ID:X8} Ctr=0q{CTR:X16}");
        }

        /// <summary>
        /// Commits a <see cref="GPR"/> register to the remote console.
        /// </summary>
        /// <param name="in_index">The index of the register to commit.</param>
        public void CommitGPR(int in_index)
        {
            if (GPR.Length <= in_index || in_index < 0)
                throw new ArgumentException("Index must be a value between 0-31.");
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Changed GPR{in_index} to 0x{GPR[in_index]:X16}");
#endif
            _console.Client.SendCommand($"setcontext thread=0x{_thread.ID:X8} Gpr{in_index}=0q{GPR[in_index]:X16}");
        }

        /// <summary>
        /// Commits all <see cref="GPR"/> registers to the remote console.
        /// </summary>
        public void CommitGPRs()
        {
            for (int i = 0; i < GPR.Length; i++)
                CommitGPR(i);
        }

        /// <summary>
        /// Commits the <see cref="CR"/> register to the remote console.
        /// </summary>
        public void CommitCR()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Changed CR to 0x{CR:X8}");
#endif

            _console.Client.SendCommand($"setcontext thread=0x{_thread.ID:X8} Cr=0x{CR:X8}");
        }

        /// <summary>
        /// Commits the <see cref="XER"/> register to the remote console.
        /// </summary>
        public void CommitXER()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Changed XER to 0x{XER.Packed:X8}");
#endif

            _console.Client.SendCommand($"setcontext thread=0x{_thread.ID:X8} Xer=0x{XER.Packed:X8}");
        }

        /// <summary>
        /// Commits the <see cref="FPSCR"/> register to the remote console.
        /// </summary>
        public void CommitFPSCR()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Changed FPSCR to 0x{FPSCR.Packed:X16}");
#endif

            _console.Client.SendCommand($"setcontext thread=0x{_thread.ID:X8} Fpscr=0q{FPSCR.Packed:X16}");
        }

        /// <summary>
        /// Commits an <see cref="FPR"/> register to the remote console.
        /// </summary>
        /// <param name="in_index">The index of the register to commit.</param>
        public void CommitFPR(int in_index)
        {
            if (FPR.Length <= in_index || in_index < 0)
                throw new ArgumentException("Index must be a value between 0-31.");

            var doubleBits = BitConverter.DoubleToUInt64Bits(FPR[in_index]);
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Changed FPR{in_index} to 0x{doubleBits:X16}");
#endif
            _console.Client.SendCommand($"setcontext thread=0x{_thread.ID:X} Fpr{in_index}=0q{doubleBits:X16}");
        }

        /// <summary>
        /// Commits all <see cref="FPR"/> registers to the remote console.
        /// </summary>
        public void CommitFPRs()
        {
            for (int i = 0; i < FPR.Length; i++)
                CommitFPR(i);
        }

        /// <summary>
        /// Commits the <see cref="VSCR"/> register to the remote console.
        /// <para>Not supported with Freeboot XBDM.</para>
        /// </summary>
        public void CommitVSCR()
        {
            if (_console.Client.Info.IsFreebootXBDM)
                return;

            var vector = MemoryHelper.Vector4ToHexString(VSCR, true);
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Changed VSCR to <{vector}>");
#endif
            /* FIXME: this is just fucking broken with Freeboot XBDM.
                      "0x0000803F,0x00000000,0x00000000,0x00000000" is parsed as
                      "0x00000000,0x00000000,0xFFFFFF80,0x0000003F". */
            _console.Client.SendCommand($"setcontext thread=0x{_thread.ID:X8} Vscr={vector}");
        }

        /// <summary>
        /// Commits a <see cref="VR"/> register to the remote console.
        /// <para>Not supported with Freeboot XBDM.</para>
        /// </summary>
        /// <param name="in_index">The index of the register to commit.</param>
        public void CommitVR(int in_index)
        {
            if (_console.Client.Info.IsFreebootXBDM)
                return;

            if (VR.Length <= in_index || in_index < 0)
                throw new ArgumentException("Index must be a value between 0-127.");

            var vector = MemoryHelper.Vector4ToHexString(VR[in_index], true);
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Changed VR{in_index} to <{vector}>");
#endif
            // FIXME: see reason for CommitVSCR.
            _console.Client.SendCommand($"setcontext thread=0x{_thread.ID:X8} Vr{in_index}={vector}");
        }

        /// <summary>
        /// Commits all <see cref="VR"/> registers to the remote console.
        /// </summary>
        public void CommitVRs()
        {
            for (int i = 0; i < VR.Length; i++)
                CommitVR(i);
        }

        #endregion // Register Committing

        #region PropertyChanged Events

        private void DispatchEvent(Action in_action)
        {
            if (!IsCommitOnChange)
                return;

            in_action();
        }

        public void OnMSRChanged() => DispatchEvent(CommitMSR);
        public void OnIARChanged() => DispatchEvent(CommitIAR);
        public void OnLRChanged() => DispatchEvent(CommitLR);
        public void OnCTRChanged() => DispatchEvent(CommitCTR);
        public void OnGPRChanged() => DispatchEvent(CommitGPRs);
        public void OnCRChanged() => DispatchEvent(CommitCR);
        public void OnXERChanged() => DispatchEvent(CommitXER);
        public void OnFPSCRChanged() => DispatchEvent(CommitFPSCR);
        public void OnFPRChanged() => DispatchEvent(CommitFPRs);
        public void OnVSCRChanged() => DispatchEvent(CommitVSCR);
        public void OnVRChanged() => DispatchEvent(CommitVRs);

        #endregion // PropertyChanged Events

        public override string ToString()
        {
            return $"XCPU @ 0x{IAR:X8}";
        }
    }
}
