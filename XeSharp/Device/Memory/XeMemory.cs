using System.Runtime.InteropServices;
using System.Text;
using XeSharp.Debug;
using XeSharp.Device.Title;
using XeSharp.Helpers;
using XeSharp.Net;

namespace XeSharp.Device.Memory
{
    public class XeMemory(XeConsole in_console)
    {
        protected XeConsole _console = in_console;

        private XeModuleInfo _moduleBufferOwner;
        private byte[] _moduleBuffer;

        /// <summary>
        /// Determines whether the memory address is accessible.
        /// </summary>
        /// <param name="in_addr">The virtual address to check.</param>
        public bool IsAccessible(uint in_addr)
        {
            var response = _console.Client.SendCommand($"getmem addr={in_addr} length=1", false);

            if (response.Results.Length <= 0)
                return false;

            return (string)response.Results[0] != "??";
        }

        /// <summary>
        /// Reads a memory location into a buffer.
        /// </summary>
        /// <param name="in_addr">The virtual address to read from.</param>
        /// <param name="in_length">The length of memory to read.</param>
        public byte[] ReadBytes(uint in_addr, uint in_length)
        {
            var response = _console.Client.SendCommand($"getmem addr={in_addr} length={in_length}");

            return MemoryHelper.HexStringToByteArray(string.Join(string.Empty, response.Results));
        }

        /// <summary>
        /// Reads a memory location into an unmanaged type.
        /// </summary>
        /// <typeparam name="T">The type to implicitly cast to.</typeparam>
        /// <param name="in_addr">The virtual address to read from.</param>
        public T Read<T>(uint in_addr) where T : unmanaged
        {
            var data = ReadBytes(in_addr, (uint)Marshal.SizeOf(typeof(T)));

            if (data.Length <= 0)
                return default;

            return MemoryHelper.ByteArrayToUnmanagedType<T>(data.Reverse().ToArray());
        }

        /// <summary>
        /// Reads a null-terminated string from the memory location.
        /// </summary>
        /// <param name="in_addr">The virtual address to read from.</param>
        /// <param name="in_encoding">The encoding format used by the string.</param>
        public string ReadStringNullTerminated(uint in_addr, Encoding in_encoding = null)
        {
            var data = new List<byte>();
            var encoding = in_encoding ?? Encoding.UTF8;

            uint addr = in_addr;

            if (encoding == Encoding.Unicode ||
                encoding == Encoding.BigEndianUnicode)
            {
                ushort us;

                while ((us = Read<ushort>(addr)) != 0)
                {
                    data.Add((byte)(us & 0xFF));
                    data.Add((byte)((us >> 8) & 0xFF));
                    addr += 2;
                }
            }
            else
            {
                byte b;

                while ((b = Read<byte>(addr)) != 0)
                {
                    data.Add(b);
                    addr++;
                }
            }

            return encoding.GetString(data.ToArray());
        }

        /// <summary>
        /// Writes a buffer to a memory location.
        /// </summary>
        /// <param name="in_addr">The virtual address to write to.</param>
        /// <param name="in_data">The buffer to write.</param>
        public XeResponse WriteBytes(uint in_addr, byte[] in_data)
        {
            var cmd = $"setmem addr={in_addr} data=";

            // TODO: account for commands having a 512 character limit (see XBDM_LINE_TOO_LONG).
            // var len = cmd.Length + (in_data.Length * 2);

            foreach (var b in in_data)
                cmd += b.ToString("X2");

            return _console.Client.SendCommand(cmd);
        }

        /// <summary>
        /// Writes an unmanaged type to a memory location.
        /// </summary>
        /// <typeparam name="T">The implicit type to write.</typeparam>
        /// <param name="in_addr">The virtual address to write to.</param>
        /// <param name="in_data">The data to write.</param>
        public XeResponse Write<T>(uint in_addr, T in_data) where T : unmanaged
        {
            return WriteBytes(in_addr, MemoryHelper.UnmanagedTypeToByteArray(in_data));
        }

        /// <summary>
        /// Parses a memory location from a string.
        /// </summary>
        /// <param name="in_token">The token to parse into a memory location (e.g. "IAR", "LR", "CTR", "GPR#", "R#", "0x82000000").</param>
        public uint ParseAddressFromToken(string in_token)
        {
            var tokenDerefs = StringHelper.GetDereferenceCount(in_token);

            if (tokenDerefs > 0)
                in_token = StringHelper.TrimDereferences(in_token);

            var addr = 0U;
            var register = in_token.ToLower();
            var processor = new XeDebugger(in_console).GetProcessor();

            switch (register)
            {
                case "iar": addr = processor.IAR;       break;
                case "lr":  addr = processor.LR;        break;
                case "ctr": addr = (uint)processor.CTR; break;

                default:
                {
                    if (processor.TryParseGPRByName(register, out var out_gpr))
                    {
                        addr = (uint)out_gpr;
                        break;
                    }

                    // Assume address.
                    addr = MemoryHelper.ChangeType<uint>(in_token);

                    break;
                }
            }

            addr = _console.Memory.DereferencePointer(addr, tokenDerefs);

            return addr;
        }

        /// <summary>
        /// Parses a memory location from a string.
        /// </summary>
        /// <param name="in_token">The token to parse into a memory location (e.g. "IAR", "LR", "CTR", "GPR#", "R#", "0x82000000").</param>
        /// <param name="out_addr">The parsed virtual address.</param>
        public bool TryParseAddressFromToken(string in_token, out uint out_addr)
        {
            try
            {
                out_addr = ParseAddressFromToken(in_token);
                return true;
            }
            catch
            {
                out_addr = 0U;
                return false;
            }
        }

        /// <summary>
        /// Dereferences a pointer.
        /// </summary>
        /// <param name="in_addr">The pointer to dereference from.</param>
        /// <param name="in_count">The amount of times to dereference this pointer.</param>
        public uint DereferencePointer(uint in_addr, int in_count)
        {
            while (in_count > 0)
            {
                in_addr = Read<uint>(in_addr);
                in_count--;
            }

            return in_addr;
        }

        /// <summary>
        /// Scans the current foreground module for a byte pattern.
        /// </summary>
        /// <param name="in_memory">The buffer to scan (downloads the current foreground module if null).</param>
        /// <param name="in_pattern">The pattern to scan for.</param>
        /// <param name="in_mask">The mask of the pattern ('x' for scannable bytes, '?' for any byte).</param>
        /// <param name="in_moduleName">The name of the module to scan (used to get the address and size to scan).</param>
        /// <param name="in_isFirstResult">Determines whether this function returns the first match it finds, rather than all matches.</param>
        /// <param name="in_isInvalidateModuleBuffer">Determines whether the current module buffer should be cleared and reloaded.</param>
        public List<uint> ScanSignature(byte[] in_memory, byte[] in_pattern, string in_mask, string in_moduleName = "", bool in_isFirstResult = true, bool in_isInvalidateModuleBuffer = false)
        {
            var results = new List<uint>();

            if (in_pattern.Length <= 0)
                return results;

            var modules = _console.GetModules();

            if (!string.IsNullOrEmpty(in_moduleName) && !modules.ContainsKey(in_moduleName))
                return results;

            var module = string.IsNullOrEmpty(in_moduleName)
                ? modules.Last().Value
                : modules[in_moduleName];

            // Clear module buffer if the owner changed.
            if (_moduleBufferOwner != module || in_isInvalidateModuleBuffer)
                _moduleBuffer = null;

            /* TODO: this could (and probably should) be optimised to scan
                     in chunks, rather than downloading the entire binary. */
            _moduleBuffer = in_memory ?? _moduleBuffer ?? ReadBytes(module.BaseAddress, module.ImageSize);

            // TODO: implement progress event with ClientReadEventHandler.
            for (uint i = 0; i < _moduleBuffer.Length; i++)
            {
                int sigIndex;

                for (sigIndex = 0; sigIndex < in_mask.Length; sigIndex++)
                {
                    if (_moduleBuffer.Length <= i + sigIndex)
                        break;

                    if (in_mask[sigIndex] != '?' && in_pattern[sigIndex] != _moduleBuffer[i + sigIndex])
                        break;
                }

                if (sigIndex == in_mask.Length)
                {
                    results.Add(module.BaseAddress + i);

                    if (in_isFirstResult)
                        return results;
                }
            }

            return results;
        }

        /// <summary>
        /// Scans the current foreground module for a byte pattern.
        /// </summary>
        /// <param name="in_pattern">The pattern to scan for.</param>
        /// <param name="in_mask">The mask of the pattern ('x' for scannable bytes, '?' for any byte).</param>
        /// <param name="in_moduleName">The name of the module to scan (used to get the address and size to scan).</param>
        /// <param name="in_isFirstResult">Determines whether this function returns the first match it finds, rather than all matches.</param>
        /// <param name="in_isInvalidateModuleBuffer">Determines whether the current module buffer should be cleared and reloaded.</param>
        public List<uint> ScanSignature(byte[] in_pattern, string in_mask, string in_moduleName = "", bool in_isFirstResult = true, bool in_isInvalidateModuleBuffer = false)
        {
            return ScanSignature(null, in_pattern, in_mask, in_moduleName, in_isFirstResult, in_isInvalidateModuleBuffer);
        }

        /// <summary>
        /// Scans the current foreground module for an unmanaged value.
        /// </summary>
        /// <typeparam name="T">The type of data to scan for.</typeparam>
        /// <param name="in_value">The data to scan for.</param>
        /// <param name="in_moduleName">The name of the module to scan (used to get the address and size to scan).</param>
        /// <param name="in_isFirstResult">Determines whether this function returns the first match it finds, rather than all matches.</param>
        /// <param name="in_isInvalidateModuleBuffer">Determines whether the current module buffer should be cleared and reloaded.</param>
        public List<uint> ScanSignature<T>(T in_value, string in_moduleName = "", bool in_isFirstResult = true, bool in_isInvalidateModuleBuffer = false) where T : unmanaged
        {
            var pattern = MemoryHelper.UnmanagedTypeToByteArray(in_value);

            return ScanSignature(pattern, new string('x', pattern.Length), in_moduleName, in_isFirstResult, in_isInvalidateModuleBuffer);
        }
    }
}
