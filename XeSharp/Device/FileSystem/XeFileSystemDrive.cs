﻿using XeSharp.Device.FileSystem.IO;
using XeSharp.Helpers;
using XeSharp.Serialisation.INI;

namespace XeSharp.Device.FileSystem
{
    public class XeFileSystemDrive : XeFileSystemNode
    {
        /// <summary>
        /// The user-defined name of this drive.
        /// </summary>
        public string FriendlyName { get; internal set; }

        /// <summary>
        /// The total bytes free for this drive.
        /// </summary>
        public ulong FreeSpace { get; internal set; }

        /// <summary>
        /// The total bytes used for this drive.
        /// </summary>
        public ulong UsedSpace => Capacity - FreeSpace;

        /// <summary>
        /// The capacity of this drive.
        /// </summary>
        public ulong Capacity { get; internal set; }

        public override EXeFileSystemNodeType Type => EXeFileSystemNodeType.Directory;

        public XeFileSystemDrive() { }

        /// <summary>
        /// Creates a new drive instance.
        /// </summary>
        /// <param name="in_filesystem">The filesystem this drive belongs to.</param>
        /// <param name="in_name">The name of this drive.</param>
        /// <param name="in_nodes">The nodes in this drive.</param>
        public XeFileSystemDrive(XeFileSystem in_filesystem, string in_name, List<XeFileSystemNode> in_nodes = null)
        {
            Name = in_name;
            Nodes = in_nodes;
            FileSystem = in_filesystem;
            FriendlyName = GetFriendlyName();

            Parse((string)in_filesystem.Console.Client.SendCommand($"drivefreespace name=\"{ToString()}\"").Results[0]);
        }

        /// <summary>
        /// Parses drive capacity information.
        /// </summary>
        /// <param name="in_driveCsv">The space-separated values for information about this drive's capacity.</param>
        public void Parse(string in_driveCsv)
        {
            var ini = IniParser.DoInline(in_driveCsv);

            FreeSpace = ((ulong)MemoryHelper.ChangeType<uint>(ini[""]["totalfreebyteshi"]) << 32) |
                MemoryHelper.ChangeType<uint>(ini[""]["totalfreebyteslo"]);

            Capacity = ((ulong)MemoryHelper.ChangeType<uint>(ini[""]["totalbyteshi"]) << 32) |
                MemoryHelper.ChangeType<uint>(ini[""]["totalbyteslo"]);
        }

        /// <summary>
        /// Gets the user-defined name of this drive.
        /// </summary>
        public string GetFriendlyName()
        {
            var data = FileSystem.Download($@"{Name}\name.txt");

            if (data == null || data.Length <= 0)
                return string.Empty;

            return ByteOrderMark.DecodeFromBOM(data);
        }

        /// <summary>
        /// Determines whether space is available to fit the specified size.
        /// </summary>
        /// <param name="in_size">The size to check.</param>
        public bool IsSpaceAvailable(ulong in_size)
        {
            return FreeSpace > in_size;
        }

        /// <summary>
        /// Determines whether space is available to fit the specified local file or directory.
        /// </summary>
        /// <param name="in_localPath">The local path to check the size with.</param>
        public bool IsSpaceAvailable(string in_localPath)
        {
            if (File.Exists(in_localPath))
                return IsSpaceAvailable((ulong)new FileInfo(in_localPath).Length);

            if (Directory.Exists(in_localPath))
                return IsSpaceAvailable((ulong)FileSystemHelper.GetDirectorySize(in_localPath, false));

            return false;
        }

        /// <summary>
        /// Gets friendly information about this drive.
        /// </summary>
        public override string GetInfo(bool in_isRecursiveNodes = false)
        {
            return $"Name ─────── : {(string.IsNullOrEmpty(FriendlyName) ? "None" : FriendlyName)}\n" +
                   $"Volume ───── : {Name[..^1] ?? "Unknown"}\n" +
                   $"Used Space ─ : {FormatHelper.ByteLengthToDecimalString(UsedSpace)} ({UsedSpace:N0} bytes)\n" +
                   $"Free Space ─ : {FormatHelper.ByteLengthToDecimalString(FreeSpace)} ({FreeSpace:N0} bytes)\n" +
                   $"Capacity ─── : {FormatHelper.ByteLengthToDecimalString(Capacity)} ({Capacity:N0} bytes)\n";
        }

        public override string ToString()
        {
            return base.ToString() + '\\';
        }
    }
}
