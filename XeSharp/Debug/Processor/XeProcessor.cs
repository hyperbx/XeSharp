﻿using System.ComponentModel;
using System.Numerics;
using System.Text;
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
        /// Machine State Register (MSR) - stores information regarding the CPU and its current state.
        /// </summary>
        public uint MSR { get; set; }

        /// <summary>
        /// Instruction Address Register (IAR) - stores the virtual address of the current instruction.
        /// </summary>
        public uint IAR { get; set; }

        /// <summary>
        /// Link Register (LR) - stores the return address for the next branch instruction.
        /// </summary>
        public uint LR { get; set; }

        /// <summary>
        /// CounT Register (CTR) - stores the current iteration of a loop or a pointer to a virtual function.
        /// </summary>
        public ulong CTR { get; set; }

        /// <summary>
        /// General Purpose Registers (GPR0-31).
        /// <para>For more information, see the <a href="https://zenith.nsmbu.net/wiki/Custom_Code/PowerPC_Assembly_Cheatsheet">PowerPC Assembly Cheetsheet</a>.</para>
        /// </summary>
        public RegisterSet<ulong> GPR { get; set; } = new(32);

        /// <summary>
        /// Condition Register (CR) - stores conditional results for previous instructions.
        /// </summary>
        public ConditionRegister CR { get; set; } = new(0);

        /// <summary>
        /// fiXed point Exception Register (XER) - stores information regarding erroneous operations.
        /// </summary>
        public FixedPointExceptionRegister XER { get; set; } = new(0);

        /// <summary>
        /// Floating Point Status and Control Register (FPSCR) - stores exception and rounding control information for floating point arithmetic.
        /// </summary>
        public FloatingPointStatusControlRegister FPSCR { get; set; } = new(0);

        /// <summary>
        /// Floating Point Registers (FPR0-31).
        /// <para>For more information, see the <a href="https://zenith.nsmbu.net/wiki/Custom_Code/PowerPC_Assembly_Cheatsheet">PowerPC Assembly Cheetsheet</a>.</para>
        /// </summary>
        public RegisterSet<double> FPR { get; set; } = new(32);

        /// <summary>
        /// Vector Status and Control Register (VSCR).
        /// </summary>
        public Vector4 VSCR { get; set; }

        /// <summary>
        /// Vector Registers (VR0-127).
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
        /// Parses a register from its name (e.g. "GPR0", "FPR0", etc).
        /// <para>Only use this if absolutely necessary.</para>
        /// </summary>
        /// <param name="in_registerName">The name of the register.</param>
        /// <param name="out_type">The type of the register.</param>
        /// <param name="out_index">The index of the register.</param>
        public bool TryParseRegisterByName(string in_registerName, out ERegisterType out_type, out int out_index)
        {
            in_registerName = in_registerName.ToLower();

            var isRegisterFullName = in_registerName.StartsWith("gpr") ||
                in_registerName.StartsWith("fpr");

            var isRegisterShortName = in_registerName.StartsWith("r") ||
                in_registerName.StartsWith("f");

            var isRegister = isRegisterFullName || isRegisterShortName;

            if (isRegister && int.TryParse(in_registerName[(isRegisterFullName ? 3 : 1)..], out int out_registerIndex))
            {
                out_index = out_registerIndex;

                if (in_registerName.StartsWith("gpr") || in_registerName.StartsWith('r'))
                {
                    out_type = ERegisterType.GPR;
                }
                else if (in_registerName.StartsWith("fpr") || in_registerName.StartsWith('f'))
                {
                    out_type = ERegisterType.FPR;
                }
                else
                {
                    out_type = ERegisterType.Unknown;
                    return false;
                }

                return true;
            }

            out_type = ERegisterType.Unknown;
            out_index = 0;

            return false;
        }

        /// <summary>
        /// Gets a General Purpose Register (GPR) by name (e.g. "GPR0", "GPR1", etc).
        /// <para>Only use this if absolutely necessary.</para>
        /// </summary>
        /// <param name="in_registerName">The name of the register.</param>
        /// <param name="out_gpr">The value of the register.</param>
        public bool TryParseGPRByName(string in_registerName, out ulong out_gpr)
        {
            if (TryParseRegisterByName(in_registerName, out var out_type, out var out_index))
            {
                out_gpr = GPR[out_index];

                if (out_type != ERegisterType.GPR)
                    return false;

                return true;
            }

            out_gpr = 0;

            return false;
        }

        /// <summary>
        /// Gets a Floating Point Register (FPR) by name (e.g. "FPR0", "FPR1", etc).
        /// <para>Only use this if absolutely necessary.</para>
        /// </summary>
        /// <param name="in_registerName">The name of the register.</param>
        /// <param name="out_fpr">The value of the register.</param>
        public bool TryParseFPRByName(string in_registerName, out double out_fpr)
        {
            if (TryParseRegisterByName(in_registerName, out var out_type, out var out_index))
            {
                out_fpr = FPR[out_index];

                if (out_type != ERegisterType.FPR)
                    return false;

                return true;
            }

            out_fpr = 0;

            return false;
        }

        /// <summary>
        /// Gets a formatted representation of the registers.
        /// </summary>
        public string GetRegisterInfo(ERegisterType in_registers = ERegisterType.All)
        {
            var result = new StringBuilder();

            static string GetLine(int in_index)
            {
                if (in_index < 10)
                    return "──";
                else if (in_index >= 10 && in_index < 100)
                    return "─";

                return "";
            }

            if (in_registers.HasFlag(ERegisterType.MSR))
                result.AppendLine($"MSR ─── : 0x{MSR:X8}");

            if (in_registers.HasFlag(ERegisterType.IAR))
                result.AppendLine($"IAR ─── : 0x{IAR:X8}");

            if (in_registers.HasFlag(ERegisterType.LR))
                result.AppendLine($"LR ──── : 0x{LR:X8}");

            if (in_registers.HasFlag(ERegisterType.CTR))
                result.AppendLine($"CTR ─── : 0x{CTR:X16}");

            if (in_registers.HasFlag(ERegisterType.GPR))
            {
                for (int i = 0; i < GPR.Length; i++)
                    result.AppendLine($"GPR{i} {GetLine(i)} : 0x{GPR[i]:X16}");
            }

            if (in_registers.HasFlag(ERegisterType.CR))
                result.AppendLine($"CR ──── : 0x{CR.Packed:X8} ({CR})");

            if (in_registers.HasFlag(ERegisterType.XER))
                result.AppendLine($"XER ─── : 0x{XER.Packed:X8} ({XER})");

            if (in_registers.HasFlag(ERegisterType.FPSCR))
                result.AppendLine($"FPSCR ─ : 0x{FPSCR.Packed:X16} ({FPSCR})");

            if (in_registers.HasFlag(ERegisterType.FPR))
            {
                for (int i = 0; i < FPR.Length; i++)
                    result.AppendLine($"FPR{i} {GetLine(i)} : 0x{BitConverter.DoubleToUInt64Bits(FPR[i]):X16} ({FPR[i]})");
            }

            if (in_registers.HasFlag(ERegisterType.VSCR))
                result.AppendLine($"VSCR ── : <{MemoryHelper.Vector4ToHexString(VSCR)}>");

            if (in_registers.HasFlag(ERegisterType.VR))
            {
                for (int i = 0; i < VR.Length; i++)
                    result.AppendLine($"VR{i} ─{GetLine(i)} : <{MemoryHelper.Vector4ToHexString(VR[i])}> ({VR[i]})");
            }

            return result.ToString();
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
