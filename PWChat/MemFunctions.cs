using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;


public class MemFunctions
{
    public delegate int ThreadProc(IntPtr param);


    public enum EncodingType
    {
        ASCII,
        Unicode,
        UTF7,
        UTF8,
        GBK
    }

    [Flags]
    public enum FreeType
    {
        Decommit = 0x4000,
        Release = 0x8000,
    }

    const UInt32 INFINITE = 0xFFFFFFFF;
    const UInt32 WAIT_ABANDONED = 0x00000080;
    const UInt32 WAIT_OBJECT_0 = 0x00000000;
    const UInt32 WAIT_TIMEOUT = 0x00000102;

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(
        UInt32 dwDesiredAccess,
        Int32 bInheritHandle,
        UInt32 dwProcessId
        );

    [DllImport("Kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
        [In, Out] byte[] lpBuffer, UInt32 nSize, ref UInt32 lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    private static extern Int32 CloseHandle(
        IntPtr hObject
        );

    [DllImport("kernel32.dll")]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] lpBuffer, UInt32 nSize, ref UInt32 lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
       int dwSize, FreeType dwFreeType);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);


    [DllImport("kernel32.dll")]
    private static extern IntPtr CreateRemoteThread(IntPtr hProcess,
       IntPtr lpThreadAttributes, uint dwStackSize, IntPtr
       lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);


    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
       uint dwSize, UInt32 flAllocationType, UInt32 flProtect);

    [DllImport("kernel32.dll")]
    private static extern UInt32 GetLastError();


    public static int AllocateMemory(IntPtr processHandle, int memorySize)
    {
        return (int)VirtualAllocEx(processHandle, (IntPtr)0, (uint)memorySize, 0x1000, 0x40);
    }

    public static IntPtr CreateRemoteThread(IntPtr processHandle, int address)
    {

        return CreateRemoteThread(processHandle, (IntPtr)0, 0, (IntPtr)address, (IntPtr)0, 0, (IntPtr)0);
    }

    public static void WaitForSingleObject(IntPtr threadHandle)
    {
        if (WaitForSingleObject(threadHandle, INFINITE) != WAIT_OBJECT_0)
        {
            Debug.WriteLine("Failed waiting for single object");
        }
    }

    public static void FreeMemory(IntPtr processHandle, int address)
    {
        bool result;
        result = VirtualFreeEx(processHandle, (IntPtr)address, 0, FreeType.Release);
    }

    public static UInt32 GetError()
    {
        return GetLastError();
    }

    // constants information can be found in <winnt.h>
    [Flags]
    public enum ProcessAccessType
    {
        PROCESS_TERMINATE = (0x0001),
        PROCESS_CREATE_THREAD = (0x0002),
        PROCESS_SET_SESSIONID = (0x0004),
        PROCESS_VM_OPERATION = (0x0008),
        PROCESS_VM_READ = (0x0010),
        PROCESS_VM_WRITE = (0x0020),
        PROCESS_DUP_HANDLE = (0x0040),
        PROCESS_CREATE_PROCESS = (0x0080),
        PROCESS_SET_QUOTA = (0x0100),
        PROCESS_SET_INFORMATION = (0x0200),
        PROCESS_QUERY_INFORMATION = (0x0400)
    }

    public static IntPtr OpenProcess(int pId)
    {
        ProcessAccessType access = ProcessAccessType.PROCESS_VM_READ
                                   | ProcessAccessType.PROCESS_VM_WRITE
                                   | ProcessAccessType.PROCESS_VM_OPERATION;
        return OpenProcess(2035711, 0, (UInt32)pId);
        //return OpenProcess((uint)access, 0, (UInt32)pId);
    }

    public static void CloseProcess(IntPtr handle)
    {
        Int32 result = CloseHandle(handle);
    }

    public static void MemWriteBytes(IntPtr processHandle, int address, byte[] value)
    {
        bool success;
        UInt32 nBytesRead = 0;
        success = WriteProcessMemory(processHandle, (IntPtr)address, value, (uint)value.Length, ref nBytesRead);
    }

    public static void MemWriteStruct(IntPtr processHandle, int address, object value)
    {
        bool success;
        byte[] buffer = RawSerialize(value);
        UInt32 nBytesRead = 0;
        success = WriteProcessMemory(processHandle, (IntPtr)address, buffer, (uint)buffer.Length, ref nBytesRead);
    }

    public static void MemWriteInt(IntPtr processHandle, int address, int value)
    {
        bool success;
        byte[] buffer = BitConverter.GetBytes(value);
        UInt32 nBytesRead = 0;
        success = WriteProcessMemory(processHandle, (IntPtr)address, buffer, 4, ref nBytesRead);
    }

    public static void MemWriteFloat(IntPtr processHandle, int address, float value)
    {
        bool success;
        byte[] buffer = BitConverter.GetBytes(value);
        UInt32 nBytesRead = 0;
        success = WriteProcessMemory(processHandle, (IntPtr)address, buffer, 4, ref nBytesRead);
    }

    public static void MemWriteShort(IntPtr processHandle, int address, short value)
    {
        bool success;
        byte[] buffer = BitConverter.GetBytes(value);
        UInt32 nBytesRead = 0;
        success = WriteProcessMemory(processHandle, (IntPtr)address, buffer, 2, ref nBytesRead);
    }

    public static void MemWriteByte(IntPtr processHandle, int address, byte value)
    {
        bool success;
        byte[] buffer = BitConverter.GetBytes(value);
        UInt32 nBytesRead = 0;
        success = WriteProcessMemory(processHandle, (IntPtr)address, buffer, 1, ref nBytesRead);
    }

    public static byte[] MemReadBytes(IntPtr processHandle, int address, int size)
    {
        bool success;
        byte[] buffer = new byte[size];
        UInt32 nBytesRead = 0;
        success = ReadProcessMemory(processHandle, (IntPtr)address, buffer, (uint)size, ref nBytesRead);
        return buffer;
    }

    public unsafe static bool MemReadBytesToStruct(IntPtr processHandle, uint address, byte* pBytes, int size)
    {
        bool success;
        byte[] buffer = new byte[size];
        UInt32 nBytesRead = 0;
        success = ReadProcessMemory(processHandle, (IntPtr)address, buffer, (uint)size, ref nBytesRead);

        for (int i = 0, j = buffer.Length; i < j; i++)
        {
            *pBytes++ = buffer[i];
        }
        return success;
    }

    public static int MemReadInt(IntPtr processHandle, int address)
    {
        bool success;
        byte[] buffer = new byte[4];
        UInt32 nBytesRead = 0;
        success = ReadProcessMemory(processHandle, (IntPtr)address, buffer, 4, ref nBytesRead);
        return BitConverter.ToInt32(buffer, 0);
    }


    public static uint MemReadUInt(IntPtr processHandle, uint address)
    {
        bool success;
        byte[] buffer = new byte[4];
        UInt32 nBytesRead = 0;
        success = ReadProcessMemory(processHandle, (IntPtr)address, buffer, 4, ref nBytesRead);
        return BitConverter.ToUInt32(buffer, 0);
    }

    // Returns the data referenced by a nested pointer, i.e., [[[[someAddress]+24]+28]+4]
    public static uint MemReadUIntNested(IntPtr processHandle, uint firstAddr, params uint[] p)
    {
        uint val = MemReadUInt(processHandle, firstAddr);

        for (int i = 0; i < p.Length; i++)
        {
            val = MemReadUInt(processHandle, val + p[i]);
        }

        return val;
    }

    // Resolves a nested pointer, i.e., [[[[someAddress]+24]+28]+4]
    // To return the data referenced by the pointer (uint only) use 0 as the last param.
    public static uint ResolveNestedPointer(IntPtr processHandle, uint firstAddr, params uint[] p)
    {
        uint val = MemReadUInt(processHandle, firstAddr);

        for (int i = 0; i < p.Length - 1; i++)
        {
            val = MemReadUInt(processHandle, val + p[i]);
        }

        return (uint)(val + p[p.Length - 1]);
    }


    public static float MemReadFloat(IntPtr processHandle, int address)
    {
        bool success;
        byte[] buffer = new byte[4];
        UInt32 nBytesRead = 0;
        success = ReadProcessMemory(processHandle, (IntPtr)address, buffer, 4, ref nBytesRead);
        return BitConverter.ToSingle(buffer, 0);
    }

    public static string MemReadUnicode(IntPtr processHandle, uint address, uint maxSize, ref uint _bytesRead)
    {
        bool success;
        byte[] buffer = new byte[maxSize];
        UInt32 nBytesRead = 0;
        success = ReadProcessMemory(processHandle, (IntPtr)address, buffer, maxSize, ref nBytesRead);
        _bytesRead = nBytesRead;
        return ByteArrayToString(buffer, EncodingType.Unicode);
    }

    // This one will be a bit slower, but will hopefully eliminate returning empty strings where 
    // the address + size is not in accessible memory, resulting in a failure from ReadProcessMemory.
    // Bytes are read in chunks of [chunkSize] size and chunks are scanned for 0x00, 0x00 (i.e., 
    // end of string). If this is not found, we read another chunk, repeating until the string 
    // terminator is found.
    // This might also help with reading enormous strings (e.g., complete chat listing) until I find 
    // a better solution for working around the maximum memory size that can be read
    public static string MemReadUnicodeToEnd(IntPtr processHandle, uint address, uint maxSize, uint chunkSize, ref uint _bytesRead, bool doubleTerminator = false)
    {
        bool success;
        byte[] buffer = new byte[chunkSize];
        byte[] bigString = new byte[maxSize];
        UInt32 nBytesRead = 0;
        bool exit = false;
        uint i, j, currLength = 0;
        do
        {
            success = ReadProcessMemory(processHandle, (IntPtr)(address + currLength), buffer, chunkSize, ref nBytesRead);
            _bytesRead = nBytesRead;

            System.Buffer.BlockCopy(buffer, 0, bigString, (int)currLength, (int)chunkSize);
            currLength += chunkSize;
            if (!doubleTerminator)
            {
                for (i = 0, j = chunkSize - 2; i < j; i++)
                {
                    if (buffer[i] == 0 && buffer[i + 1] == 0)
                    {
                        // We found the string terminator
                        exit = true;
                        break;
                    }
                }
            }
            else    // Full chat listing can contain more than two consecutive 0x00 bytes because of item links.
            {
                for (i = 0, j = chunkSize - 3; i < j; i++)
                {
                    if (buffer[i] == 0 && buffer[i + 1] == 0 && buffer[i + 2] == 0)
                    {
                        // We found the string terminator
                        exit = true;
                        break;
                    }
                }
            }
        } while (!exit && currLength < maxSize);
        _bytesRead = currLength - chunkSize + i;
        return ByteArrayToString(bigString, EncodingType.Unicode);
    }

    public static object MemReadStruct(IntPtr processHandle, int address, Type anyType)
    {
        int rawsize = Marshal.SizeOf(anyType);
        bool success;
        byte[] buffer = new byte[rawsize];
        UInt32 nBytesRead = 0;
        success = ReadProcessMemory(processHandle, (IntPtr)address, buffer, (UInt32)rawsize, ref nBytesRead);
        return RawDeserialize(buffer, 0, anyType);
    }


    private static object RawDeserialize(byte[] rawData, int position, Type anyType)
    {
        int rawsize = Marshal.SizeOf(anyType);
        if (rawsize > rawData.Length)
            return null;
        IntPtr buffer = Marshal.AllocHGlobal(rawsize);
        Marshal.Copy(rawData, position, buffer, rawsize);
        object retobj = Marshal.PtrToStructure(buffer, anyType);
        Marshal.FreeHGlobal(buffer);
        return retobj;
    }


    private static byte[] RawSerialize(object anything)
    {
        int rawSize = Marshal.SizeOf(anything);
        IntPtr buffer = Marshal.AllocHGlobal(rawSize);
        Marshal.StructureToPtr(anything, buffer, false);
        byte[] rawDatas = new byte[rawSize];
        Marshal.Copy(buffer, rawDatas, 0, rawSize);
        Marshal.FreeHGlobal(buffer);
        return rawDatas;
    }

    public unsafe static string FixedByteArrayToString(byte* bytes, uint maxLength, EncodingType enc = EncodingType.Unicode)
    {
        byte[] temp = new byte[maxLength];
        for (int i = 0; i < maxLength; i++)
        {
            temp[i] = *(bytes + i);
        }
        return ByteArrayToString(temp, enc);
    }

    public static string ByteArrayToString(byte[] bytes, EncodingType encodingType = EncodingType.Unicode)
    {
        int arraySize = bytes.Length;
        // Redim array to be 2 bytes bigger and fill the last two bytes with 0x00
        Array.Resize(ref bytes, arraySize + 2);
        bytes[arraySize - 1] = 0;
        bytes[arraySize - 2] = 0;

        System.Text.Encoding encoding = null;
        string result = "";
        switch (encodingType)
        {
            case EncodingType.ASCII:
                encoding = new ASCIIEncoding();
                break;
            case EncodingType.Unicode:
                encoding = new UnicodeEncoding();
                break;
            case EncodingType.UTF7:
                encoding = new UTF7Encoding();
                break;
            case EncodingType.UTF8:
                encoding = new UTF8Encoding();
                break;
            case EncodingType.GBK:
                encoding = Encoding.GetEncoding("GBK"); ;
                break;
        }

        for (int i = 0; i < bytes.Length; i += 2)
        {
            if (bytes[i] == 0 && bytes[i + 1] == 0)
            {
                result = encoding.GetString(bytes, 0, i);
                break;
            }
        }

        return result;
    }

    public unsafe static string ByteArrayToStringByRef(byte* bytes, int maxSize, EncodingType encodingType)
    {
        byte[] bArray = new byte[maxSize];
        int count = 0;


        while (!(*bytes == 0 && *(bytes + 1) == 0) && count < maxSize - 2)
        {
            byte x = *bytes;
            byte y = *(bytes + 1);

            bArray[count] = *bytes;
            bArray[count + 1] = *(bytes + 1);
            bytes += 2;
            count += 2;
        }

        return ByteArrayToString(bArray, encodingType);
    }



    public static byte[] StringToByteArray(string str, EncodingType encodingType)
    {
        Encoding encoding = null;
        switch (encodingType)
        {
            case EncodingType.ASCII:
                encoding = new ASCIIEncoding();
                break;
            case EncodingType.Unicode:
                encoding = new UnicodeEncoding();
                break;
            case EncodingType.UTF7:
                encoding = new UTF7Encoding();
                break;
            case EncodingType.UTF8:
                encoding = new UTF8Encoding();
                break;
        }
        return encoding.GetBytes(str);
    }


}
