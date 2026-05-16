using System;
using System.Collections.Generic;
using System.IO;

internal sealed class PeImage
{
    public const int DosHeaderELfanewOffset = 0x3C;
    public const int FileHeaderOffsetFromNt = 4;
    public const int FileHeaderMachineOffset = 0;
    public const int FileHeaderNumberOfSectionsOffset = 2;
    public const int FileHeaderSizeOfOptionalHeaderOffset = 16;
    public const int OptionalHeaderMagicOffset = 0;
    public const int OptionalHeaderAddressOfEntryPointOffset = 16;
    public const int OptionalHeaderImageBaseOffset32 = 28;
    public const int OptionalHeaderImageBaseOffset64 = 24;
    public const int OptionalHeaderSizeOfImageOffset = 56;
    public const int OptionalHeaderSizeOfHeadersOffset = 60;
    public const int SectionVirtualAddressOffset = 12;
    public const int SectionSizeOfRawDataOffset = 16;
    public const int SectionPointerToRawDataOffset = 20;
    public const int SectionHeaderSize = 40;

    public byte[] RawBytes { get; private set; }
    public int NtHeadersOffset { get; private set; }
    public ushort Machine { get; private set; }
    public ushort OptionalMagic { get; private set; }
    public bool Is64Bit { get; private set; }
    public IntPtr PreferredImageBase { get; private set; }
    public int SizeOfImage { get; private set; }
    public int SizeOfHeaders { get; private set; }
    public int AddressOfEntryPoint { get; private set; }
    public int FirstSectionOffset { get; private set; }
    public short NumberOfSections { get; private set; }

    private PeImage(byte[] rawBytes)
    {
        RawBytes = rawBytes;
    }

    public static PeImage FromFile(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        return FromBytes(data);
    }

    public static PeImage FromBytes(byte[] data)
    {
        if (data.Length < 0x40)
            throw new InvalidDataException("File is too small to be a PE image.");

        if (data[0] != (byte)'M' || data[1] != (byte)'Z')
            throw new InvalidDataException("Missing DOS MZ signature.");

        var pe = new PeImage(data);
        pe.NtHeadersOffset = BitConverter.ToInt32(data, DosHeaderELfanewOffset);
        if (pe.NtHeadersOffset <= 0 || pe.NtHeadersOffset + 0x108 > data.Length)
            throw new InvalidDataException("Invalid NT headers offset.");

        if (data[pe.NtHeadersOffset] != (byte)'P' || data[pe.NtHeadersOffset + 1] != (byte)'E')
            throw new InvalidDataException("Missing PE signature.");

        int fileHeader = pe.NtHeadersOffset + FileHeaderOffsetFromNt;
        pe.Machine = BitConverter.ToUInt16(data, fileHeader + FileHeaderMachineOffset);
        pe.NumberOfSections = BitConverter.ToInt16(data, fileHeader + FileHeaderNumberOfSectionsOffset);
        ushort sizeOfOptionalHeader = BitConverter.ToUInt16(data, fileHeader + FileHeaderSizeOfOptionalHeaderOffset);

        int optionalHeader = fileHeader + 20;
        pe.OptionalMagic = BitConverter.ToUInt16(data, optionalHeader + OptionalHeaderMagicOffset);
        pe.Is64Bit = pe.OptionalMagic == NativeInterop.IMAGE_NT_OPTIONAL_HDR64_MAGIC;

        if (pe.Is64Bit)
        {
            if (pe.Machine != NativeInterop.IMAGE_FILE_MACHINE_AMD64)
                throw new InvalidDataException(
                    $"PE32+ image must be AMD64 (machine 0x{NativeInterop.IMAGE_FILE_MACHINE_AMD64:X}), got 0x{pe.Machine:X}.");
            pe.PreferredImageBase = new IntPtr(BitConverter.ToInt64(data, optionalHeader + OptionalHeaderImageBaseOffset64));
        }
        else if (pe.OptionalMagic == NativeInterop.IMAGE_NT_OPTIONAL_HDR32_MAGIC)
        {
            if (pe.Machine != NativeInterop.IMAGE_FILE_MACHINE_I386)
                throw new InvalidDataException(
                    $"PE32 image must be I386 (machine 0x{NativeInterop.IMAGE_FILE_MACHINE_I386:X}), got 0x{pe.Machine:X}.");
            pe.PreferredImageBase = new IntPtr(BitConverter.ToInt32(data, optionalHeader + OptionalHeaderImageBaseOffset32));
        }
        else
        {
            throw new InvalidDataException("Unknown optional header magic.");
        }

        pe.AddressOfEntryPoint = BitConverter.ToInt32(data, optionalHeader + OptionalHeaderAddressOfEntryPointOffset);
        pe.SizeOfImage = BitConverter.ToInt32(data, optionalHeader + OptionalHeaderSizeOfImageOffset);
        pe.SizeOfHeaders = BitConverter.ToInt32(data, optionalHeader + OptionalHeaderSizeOfHeadersOffset);
        pe.FirstSectionOffset = optionalHeader + sizeOfOptionalHeader;

        return pe;
    }

    public static bool IsMachineMatch(PeImage source, PeImage target)
    {
        return source.Machine == target.Machine && source.Is64Bit == target.Is64Bit;
    }

    public IEnumerable<PeSection> EnumerateSections()
    {
        int offset = FirstSectionOffset;
        for (int i = 0; i < NumberOfSections; i++)
        {
            yield return new PeSection
            {
                VirtualAddress = BitConverter.ToInt32(RawBytes, offset + SectionVirtualAddressOffset),
                SizeOfRawData = BitConverter.ToInt32(RawBytes, offset + SectionSizeOfRawDataOffset),
                PointerToRawData = BitConverter.ToInt32(RawBytes, offset + SectionPointerToRawDataOffset)
            };
            offset += SectionHeaderSize;
        }
    }
}

internal struct PeSection
{
    public int VirtualAddress;
    public int SizeOfRawData;
    public int PointerToRawData;
}
