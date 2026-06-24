using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PEEXEAPIChecker
{
    internal static class PeImportReader
    {
        private const ushort Pe32Magic = 0x10B;
        private const ushort Pe32PlusMagic = 0x20B;
        private const int ImportDirectoryIndex = 1;

        public static IList<PeImportEntry> GetImportedApis(string filePath)
        {
            byte[] data = File.ReadAllBytes(filePath);
            var imports = new List<PeImportEntry>();

            int peOffset = BitConverter.ToInt32(data, 0x3C);
            if (peOffset <= 0 || peOffset + 4 > data.Length)
                throw new InvalidDataException("Invalid PE header offset.");

            if (Encoding.ASCII.GetString(data, peOffset, 4) != "PE\0\0")
                throw new InvalidDataException("Missing PE signature.");

            int optionalHeaderOffset = peOffset + 24;
            if (optionalHeaderOffset + 2 > data.Length)
                throw new InvalidDataException("Invalid optional header.");

            ushort magic = BitConverter.ToUInt16(data, optionalHeaderOffset);
            bool isPe32Plus = magic == Pe32PlusMagic;
            if (magic != Pe32Magic && !isPe32Plus)
                throw new InvalidDataException("Unsupported PE format.");

            ushort sizeOfOptionalHeader = BitConverter.ToUInt16(data, peOffset + 20);
            int sectionCount = BitConverter.ToUInt16(data, peOffset + 6);
            int sectionTableOffset = optionalHeaderOffset + sizeOfOptionalHeader;

            int importDirectoryOffset = GetImportDirectoryOffset(data, optionalHeaderOffset, isPe32Plus);
            uint importRva = BitConverter.ToUInt32(data, importDirectoryOffset);
            uint importSize = BitConverter.ToUInt32(data, importDirectoryOffset + 4);

            if (importRva == 0 || importSize == 0)
                return imports;

            int importTableOffset = RvaToOffset(data, sectionTableOffset, sectionCount, importRva);
            ParseImportDescriptors(data, sectionTableOffset, sectionCount, importTableOffset, isPe32Plus, imports);

            imports.Sort((left, right) => string.Compare(left.ImportName, right.ImportName, StringComparison.OrdinalIgnoreCase));
            return imports;
        }

        private static int GetImportDirectoryOffset(byte[] data, int optionalHeaderOffset, bool isPe32Plus)
        {
            int dataDirectoryStart = optionalHeaderOffset + (isPe32Plus ? 112 : 96);
            return dataDirectoryStart + (ImportDirectoryIndex * 8);
        }

        private static void ParseImportDescriptors(
            byte[] data,
            int sectionTableOffset,
            int sectionCount,
            int importTableOffset,
            bool isPe32Plus,
            List<PeImportEntry> imports)
        {
            const int descriptorSize = 20;

            for (int descriptorOffset = importTableOffset; ; descriptorOffset += descriptorSize)
            {
                if (descriptorOffset + descriptorSize > data.Length)
                    break;

                uint originalFirstThunk = BitConverter.ToUInt32(data, descriptorOffset);
                uint nameRva = BitConverter.ToUInt32(data, descriptorOffset + 12);
                uint firstThunk = BitConverter.ToUInt32(data, descriptorOffset + 16);

                if (originalFirstThunk == 0 && nameRva == 0 && firstThunk == 0)
                    break;

                uint lookupRva = originalFirstThunk != 0 ? originalFirstThunk : firstThunk;
                if (lookupRva == 0 || nameRva == 0)
                    continue;

                int dllNameOffset = RvaToOffset(data, sectionTableOffset, sectionCount, nameRva);
                string dllName = ReadNullTerminatedAscii(data, dllNameOffset);
                int thunkOffset = RvaToOffset(data, sectionTableOffset, sectionCount, lookupRva);

                ReadThunkEntries(data, sectionTableOffset, sectionCount, thunkOffset, isPe32Plus, dllName, imports);
            }
        }

        private static void ReadThunkEntries(
            byte[] data,
            int sectionTableOffset,
            int sectionCount,
            int thunkOffset,
            bool isPe32Plus,
            string dllName,
            List<PeImportEntry> imports)
        {
            int entrySize = isPe32Plus ? 8 : 4;
            ulong ordinalMask = isPe32Plus ? 0x8000000000000000UL : 0x80000000UL;

            for (int offset = thunkOffset; ; offset += entrySize)
            {
                if (offset + entrySize > data.Length)
                    break;

                ulong value = isPe32Plus
                    ? BitConverter.ToUInt64(data, offset)
                    : BitConverter.ToUInt32(data, offset);

                if (value == 0)
                    break;

                string importName;
                if ((value & ordinalMask) != 0)
                {
                    importName = string.Format("Ordinal_{0}", value & 0xFFFF);
                }
                else
                {
                    int nameEntryOffset = RvaToOffset(data, sectionTableOffset, sectionCount, (uint)value);
                    int functionNameOffset = nameEntryOffset + 2; // skip Hint
                    importName = ReadNullTerminatedAscii(data, functionNameOffset);
                }

                imports.Add(new PeImportEntry(dllName, importName));
            }
        }

        private static int RvaToOffset(byte[] data, int sectionTableOffset, int sectionCount, uint rva)
        {
            const int sectionHeaderSize = 40;

            for (int i = 0; i < sectionCount; i++)
            {
                int sectionOffset = sectionTableOffset + (i * sectionHeaderSize);
                if (sectionOffset + sectionHeaderSize > data.Length)
                    break;

                uint virtualSize = BitConverter.ToUInt32(data, sectionOffset + 8);
                uint virtualAddress = BitConverter.ToUInt32(data, sectionOffset + 12);
                uint sizeOfRawData = BitConverter.ToUInt32(data, sectionOffset + 16);
                uint pointerToRawData = BitConverter.ToUInt32(data, sectionOffset + 20);

                uint sectionSize = Math.Max(virtualSize, sizeOfRawData);
                if (rva >= virtualAddress && rva < virtualAddress + sectionSize)
                    return (int)(pointerToRawData + (rva - virtualAddress));
            }

            throw new InvalidDataException(string.Format("Could not map RVA 0x{0:X} to file offset.", rva));
        }

        private static string ReadNullTerminatedAscii(byte[] data, int offset)
        {
            if (offset < 0 || offset >= data.Length)
                return string.Empty;

            int end = offset;
            while (end < data.Length && data[end] != 0)
                end++;

            return Encoding.ASCII.GetString(data, offset, end - offset);
        }
    }
}
