using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace PwLib
{
    public class Mem
    {
        private static class Kernel32Dll
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

            [DllImport("Kernel32.dll")]
            public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] lpBuffer, UInt32 nSize, ref UInt32 lpNumberOfBytesRead);

            [DllImport("kernel32.dll")]
            public static extern Int32 CloseHandle(IntPtr hObject);

            [DllImport("kernel32.dll")]
            public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UInt32 nSize, ref UInt32 lpNumberOfBytesWritten);

            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, FreeType dwFreeType);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

            [DllImport("kernel32.dll")]
            public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, UInt32 flAllocationType, UInt32 flProtect);
        }

        public enum EncodingType { ASCII, Unicode, UTF7, UTF8, GBK }

        [Flags]
        private enum FreeType { Decommit = 0x4000, Release = 0x8000 }

        private const UInt32 Infinite = 0xFFFFFFFF;
        private const UInt32 WaitAbandoned = 0x00000080;
        private const UInt32 WaitObject0 = 0x00000000;
        private const UInt32 WaitTimeout = 0x00000102;

        private readonly IntPtr _processHandle;

        public Mem(PwClient client)
        {
            _processHandle = client.Handle;
        }

        private bool WriteProcessMemory(IntPtr lpBaseAddress, byte[] lpBuffer, UInt32 nSize)
        {
            uint lpNumberOfBytesWritten = 0;
            return Kernel32Dll.WriteProcessMemory(_processHandle, lpBaseAddress, lpBuffer, nSize, ref lpNumberOfBytesWritten);
        }

        private bool ReadProcessMemory(IntPtr lpBaseAddress, [In, Out] byte[] lpBuffer, UInt32 nSize)
        {
            uint lpNumberOfBytesRead = 0;
            return Kernel32Dll.ReadProcessMemory(_processHandle, lpBaseAddress, lpBuffer, nSize, ref lpNumberOfBytesRead);
        }

        public int AllocateMemory(int memorySize)
        {
            return (int)Kernel32Dll.VirtualAllocEx(_processHandle, (IntPtr)0, (uint)memorySize, 0x1000, 0x40);
        }

        public IntPtr CreateRemoteThread(int address)
        {
            return Kernel32Dll.CreateRemoteThread(_processHandle, (IntPtr)0, 0, (IntPtr)address, (IntPtr)0, 0, (IntPtr)0);
        }

        public void WaitForSingleObject(IntPtr threadHandle)
        {
            Kernel32Dll.WaitForSingleObject(threadHandle, Infinite);
        }

        public bool FreeMemory(int address, int memorySize)
        {
            return Kernel32Dll.VirtualFreeEx(_processHandle, (IntPtr)address, memorySize, FreeType.Release);
        }

        public int CloseHandle(IntPtr handle)
        {
            return Kernel32Dll.CloseHandle(handle);
        }

        #region Writing
        public bool WriteBytes(byte[] value, int address, params int[] p)
        {
            address = ResolveNestedPointer(address, p);

            return WriteProcessMemory((IntPtr)address, value, (uint)value.Length);
        }

        public bool WriteInt(int value, int address, params int[] p)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            return WriteBytes(buffer, address, p);
        }

        public bool WriteUInt(uint value, int address, params int[] p)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            return WriteBytes(buffer, address, p);
        }

        public bool WriteShort(short value, int address, params int[] p)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            return WriteBytes(buffer, address, p);
        }

        public bool WriteByte(byte value, int address, params int[] p)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            return WriteBytes(buffer, address, p);
        }

        public bool WriteFloat(float value, int address, params int[] p)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            return WriteBytes(buffer, address, p);
        }

        public bool WriteStruct<T>(T value, int address, params int[] p) where T : struct
        {
            byte[] buffer = RawSerialize(value);
            return WriteBytes(buffer, address, p);
        }
        #endregion

        #region Reading
        public byte[] ReadBytes(int size, int address, params int[] p)
        {
            address = ResolveNestedPointer(address, p);

            var buffer = new byte[size];
            ReadProcessMemory((IntPtr)address, buffer, (uint)size);
            return buffer;
        }

        public int ReadInt(int address, params int[] p)
        {
            var buffer = ReadBytes(sizeof(int), address, p);
            return BitConverter.ToInt32(buffer, 0);
        }

        public uint ReadUInt(int address, params int[] p)
        {
            var buffer = ReadBytes(sizeof(uint), address, p);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public short ReadShort(int address, params int[] p)
        {
            var buffer = ReadBytes(sizeof(short), address, p);
            return BitConverter.ToInt16(buffer, 0);
        }

        public byte ReadByte(int address, params int[] p)
        {
            return ReadBytes(1, address, p)[0];
        }

        public bool ReadBool(int address, params int[] p)
        {
            return ReadBytes(1, address, p)[0] != 0;
        }

        public float ReadFloat(int address, params int[] p)
        {
            var buffer = ReadBytes(sizeof(float), address, p);
            return BitConverter.ToSingle(buffer, 0);
        }

        public double ReadDouble(int address, params int[] p)
        {
            var buffer = ReadBytes(sizeof(double), address, p);
            return BitConverter.ToSingle(buffer, 0);
        }

        public string ReadString(int maxSize, int address, params int[] p)
        {
            var buffer = ReadBytes(maxSize, address, p);
            return ByteArrayToString(buffer);
        }

        // This one will be a bit slower, but will hopefully eliminate returning empty strings where 
        // the address + size is not in accessible memory, resulting in a failure from ReadProcessMemory.
        // Bytes are read in chunks of [chunkSize] size and chunks are scanned for 0x00, 0x00 (i.e., 
        // end of string). If this is not found, we read another chunk, repeating until the string 
        // terminator is found.
        // This might also help with reading enormous strings (e.g., complete chat listing) until I find 
        // a better solution for working around the maximum memory size that can be read
        private string ReadStringToEnd(int address, bool doubleTerminator)
        {
            const int chunkSize = 0x10;
            const int maxSize = 0x1000;
            byte[] bigString = new byte[maxSize];
            bool exit = false;
            int currLength = 0;
            do
            {
                byte[] buffer = ReadBytes(chunkSize, address + currLength);
                Buffer.BlockCopy(buffer, 0, bigString, currLength, chunkSize);
                currLength += chunkSize;

                if (!doubleTerminator)
                {
                    for (int i = 0, j = chunkSize - 2; i < j; i++)
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
                    for (int i = 0, j = chunkSize - 3; i < j; i++)
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

            return ByteArrayToString(bigString);
        }

        public string ReadStringToEnd(int address, params int[] p)
        {
            address = ResolveNestedPointer(address, p);

            return ReadStringToEnd(address, false);
        }

        public string ReadStringToEndDoubleTerminator(int address, params int[] p)
        {
            address = ResolveNestedPointer(address, p);

            return ReadStringToEnd(address, true);
        }

        public T ReadStruct<T>(int address, params int[] p) where T : struct
        {
            var type = typeof(T);
            int rawsize = Marshal.SizeOf(type);
            byte[] buffer = ReadBytes(rawsize, address, p);
            return RawDeserialize<T>(buffer, 0);
        }
        #endregion

        // Resolves a nested pointer, i.e., [[[[someAddress]+24]+28]+4]
        public int ResolveNestedPointer(int address, params int[] p)
        {
            for (int i = 0; i < p.Length; i++)
            {
                address = ReadInt(address) + p[i];
            }

            return address;
        }

        private static T RawDeserialize<T>(byte[] rawData, int position) where T : struct
        {
            var type = typeof(T);
            int rawsize = Marshal.SizeOf(type);
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawData, position, buffer, rawsize);
            var structure = (T)Marshal.PtrToStructure(buffer, type);
            Marshal.FreeHGlobal(buffer);
            return structure;
        }

        private static byte[] RawSerialize<T>(T structure) where T : struct
        {
            int rawSize = Marshal.SizeOf(structure);
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(structure, buffer, false);
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

            Encoding encoding = null;
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
}
