using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace PWChat
{
    public enum Scopes
    {
        Local, World, Squad, Faction, Whisper, S5, S6, Trade, Notification, System, GenInfo, LocalInfoB, LocalInfoC
    }

    public class ChatMessage
    {
        public uint Id { get; set; }
        public Scopes Scope { get; set; }
        public string Nickname { get; set; }
        public string Text { get; set; }
        private string _raw;
        public string Raw
        {
            get { return _raw; }
            set
            {
                _raw = value;
                if (_raw.Contains("><> &") && _raw.Contains("&: "))
                {
                    Nickname = _raw.GetBetween("><> &", "&: ");
                    Text = _raw.GetAfter("&: ");
                }
                else
                {
                    Nickname = string.Empty;
                    Text = _raw;
                }
                Text = Regex.Replace(Regex.Replace(Text, @".\<.*?\>\<.*?\>", "[:)]"), @"\^[0-9A-Fa-f]{6}", "");
            }
        }

        public uint ItemId { get; set; }

        public bool Equals(ChatMessage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Id == Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ChatMessage)) return false;
            return Equals((ChatMessage)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class ElementClient
    {
        private const uint BaseCall = 0xAF6DA4;
        private const uint NameOffset = 0x638;


        public int Pid { get; set; }
        public IntPtr MainWindowHandle { get; set; }
        public IntPtr Handle { get; set; }
        public uint PlayerBase { get; set; }
        public string Nickname { get; set; }

        public static ElementClient[] GetClients()
        {
            var processes = Process.GetProcessesByName("elementclient");
            return processes.Select(pr =>
            {
                IntPtr handle = MemFunctions.OpenProcess(pr.Id);
                uint playerBase = MemFunctions.ResolveNestedPointer(handle, BaseCall, 0x1C, 0x34);
                uint namePtr = MemFunctions.ResolveNestedPointer(handle, playerBase, NameOffset, 0);
                uint bytesRead = 0;
                return new ElementClient
                {
                    Pid = pr.Id,
                    Handle = handle,
                    PlayerBase = playerBase,
                    Nickname = MemFunctions.MemReadUnicode(handle, namePtr, 0x32, ref bytesRead),
                };
            }).ToArray();
        }
    }

    public class PW
    {
        private const uint FetchCount = 10;

        private const uint ChatObjectsBase = 0xAFBEE8; // v551
        private const uint LastChatObject = 0xAFBEF4; // v551
        private const uint SendChatCall = 0x007E8930; // v551 sendChat function address
        private const uint BaseCall = 0xAF6DA4; // v551
        private const uint ChatBoxKeyHandlerCall = 0x00808360; // v551 chatBoxKeyHandler function address

        private readonly ElementClient _client;

        private List<ChatMessage> _lastMessages;

        public delegate void NewMessageEventHandler(ChatMessage message);
        public event NewMessageEventHandler NewMessage;

        public PW(ElementClient client)
        {
            _client = client;

            NewMessage += cm => { };

            _lastMessages = new List<ChatMessage>();
            var timer = new Timer(s =>
            {
                var newMessages = GetMessages();
                foreach (var chatMessage in newMessages.Except(_lastMessages))
                {
                    NewMessage(chatMessage);
                }
                _lastMessages = newMessages;
            });
            timer.Change(500, 500);
        }

        private unsafe List<ChatMessage> GetMessages()
        {
            var list = new List<ChatMessage>();

            uint bytesRead = 0;
            // Static base pointer to chat object array
            uint chatBasePointer = MemFunctions.ResolveNestedPointer(_client.Handle, ChatObjectsBase, 0);

            // Number between 0-199 (Number of messages that can be shown in the ingame chat window)
            uint lastChatOffset = MemFunctions.ResolveNestedPointer(_client.Handle, LastChatObject, 0);

            // Calculate index of first message to show
            uint firstChatToShow = FetchCount > lastChatOffset ? 0 : (lastChatOffset - FetchCount);
            for (uint chatOffset = firstChatToShow; chatOffset < lastChatOffset; chatOffset++)
            {
                // More efficient to read the whole chat structure into an object in one hit.
                // Object is kinda like a Union in old school C / C++ (see the object definition)
                var chatObj = new Chatobj();

                // Structure object byte overlay pointer
                byte* lpChatObj = &chatObj.bytes[0];

                // To pass to mem functions (number of bytes to read)
                int sizeofChatObj = Marshal.SizeOf(chatObj);

                // Read chatObj structure from PW memory into our object
                Mem.MemReadBytesToStruct(_client.Handle, chatBasePointer + (0x1C * chatOffset), lpChatObj, sizeofChatObj);

                // Actual message entry in object is just a pointer, so go retrieve the message
                uint chatPointer = MemFunctions.ResolveNestedPointer(_client.Handle, chatBasePointer + (0x1C * (chatOffset)) + 0x8, 0);
                string str = MemFunctions.MemReadUnicode(_client.Handle, chatObj.p_msg, 256, ref bytesRead).Replace("\r", "\r\n");

                list.Add(new ChatMessage
                {
                    Id = chatObj.msgId,
                    Scope = (Scopes)chatObj.msgScope,
                    Raw = str,
                    ItemId = chatObj.itemId,
                });
            }

            return list;
        }

        public void SendMessage(string message)
        {
            SetChatText(message);
            SendChat();
        }

        #region sendChat
        /**********************************************************************************************************
         * Injects into the clients sendChat() function.
         * This function will send whatever text is visible in the chat input box.
         * Use setChatText() to write text to the chat input box before using this command.
         * 
        **********************************************************************************************************/
        private int _sendChatOpcodeAddress;
        private readonly byte[] _sendChatOpcode = new byte[]
        {
            0x60,                                   // PUSHAD
            
            0xB8, 0xC0, 0xFC, 0xA9, 0x00,           // MOV EAX,ElementC.00A9FCC0    ;  ASCII "speak"
            0x50,                                   // PUSH EAX
            0xBB, 0x01, 0x01, 0x00, 0x00,           // MOV EBX, 101
            0xBE, 0x00, 0x00, 0x00, 0x00,           // MOV ESI,chatSystemClassPtr   ;  [[[[[[baseCall]+1C]+18]+8]+C4]+20]
            0x8B, 0x16,                             // MOV EDX,DWORD PTR DS:[ESI]
            0x8B, 0xCE,                             // MOV ECX,ESI
            0xE8, 0x00, 0x00, 0x00, 0x00,	        // CALL sendChat()

            0x61,                                   // POPAD
            0xC3                                    // RETN
        };

        private void SendChat()
        {
            // If opcode has already been loaded into client memory, don't load it again.
            if (_sendChatOpcodeAddress == 0)
            {
                LoadSendChatOpcode();
            }

            //Run the opcode
            IntPtr threadHandle = MemFunctions.CreateRemoteThread(_client.Handle, _sendChatOpcodeAddress);
            //Wait for opcode to be done
            MemFunctions.WaitForSingleObject(threadHandle);
            //Close the thread
            MemFunctions.CloseProcess(threadHandle);
        }

        private void LoadSendChatOpcode()
        {
            //Allocate memory for the opcode to call the sendChat function
            _sendChatOpcodeAddress = MemFunctions.AllocateMemory(_client.Handle, _sendChatOpcode.Length);
            //Write the opcode to memory
            MemFunctions.MemWriteBytes(_client.Handle, _sendChatOpcodeAddress, _sendChatOpcode);

            //Calculate relative call address of sendChat()
            uint relAddress = SendChatCall - (uint)_sendChatOpcodeAddress - 12 - (uint)(_sendChatOpcode.Length / 2);
            //Insert the functionAddress in opcode
            byte[] functionAddress = BitConverter.GetBytes(relAddress);
            MemFunctions.MemWriteBytes(_client.Handle, _sendChatOpcodeAddress + 22, functionAddress);

            uint chatClassPtr = MemFunctions.ResolveNestedPointer(_client.Handle, BaseCall, 0x1C, 0x18, 0x8, 0xC4, 0x20, 0);
            byte[] chatClassPtrBytes = BitConverter.GetBytes(chatClassPtr);
            MemFunctions.MemWriteBytes(_client.Handle, _sendChatOpcodeAddress + 13, chatClassPtrBytes);
        }
        #endregion

        #region setChatText
        /**********************************************************************************************************
         * Injects into the clients setChatText() function.
         * This function will set the chat input box text to the passed string.
         * Use sendChat() to physically send the message after using this command.
         * 
        **********************************************************************************************************/
        private int _setChatTextOpcodeAddress;
        private readonly byte[] _setChatTextOpcode = new byte[]
        {
            0x60,                                   // PUSHAD
            0xB9, 0x00, 0x00, 0x00, 0x00,           // MOV ECX, {stringPointer}
            0xBF, 0x00, 0x00, 0x00, 0x00,           // MOV EDI, {chatBoxObjPtr}
            0x8B, 0x07,                             // MOV EAX, DWORD PTR DS:[EDI]
            0x51,                                   // PUSH ECX
            0x8B, 0xCF,                             // MOV ECX,EDI
            0xFF, 0x50, 0x44,                       // CALL DWORD PTR DS:[EAX+44]
            0x61,                                   // POPAD
            0xC3                                    // RETN
        };

        private void LoadSetChatTextOpcode()
        {
            //Allocate memory for the opcode to call the setChatText function
            _setChatTextOpcodeAddress = MemFunctions.AllocateMemory(_client.Handle, _setChatTextOpcode.Length);
            //Write the opcode to memory
            MemFunctions.MemWriteBytes(_client.Handle, _setChatTextOpcodeAddress, _setChatTextOpcode);
            // @TODO: Get the 0x44 with regex? (CALL DWORD PTR DS:[EAX+44])
        }

        // Just updates the variable information in the opcode without reallocating the whole opcode
        // to another memory location
        private void UpdateSetChatTextOpcode(string str)
        {
            Encoding unicode = Encoding.Unicode;
            byte[] unicodeBytes = unicode.GetBytes(str);

            //Allocate memory for the chat message
            int chatMsgAddress = MemFunctions.AllocateMemory(_client.Handle, unicodeBytes.Length);
            // Write the message to memory
            MemFunctions.MemWriteBytes(_client.Handle, chatMsgAddress, unicodeBytes);

            // Write the address of the string pointer
            byte[] stringAddr = BitConverter.GetBytes(chatMsgAddress);
            MemFunctions.MemWriteBytes(_client.Handle, _setChatTextOpcodeAddress + 2, stringAddr);

            // Get the pointer to the chat input box object
            uint chatBoxObjPtr = MemFunctions.ResolveNestedPointer(_client.Handle, BaseCall, 0x1C, 0x18, 0x8, 0xC4, 0x20, 0x1C4, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x08, 0);
            byte[] chatBoxObjPtrBytes = BitConverter.GetBytes(chatBoxObjPtr);
            MemFunctions.MemWriteBytes(_client.Handle, _setChatTextOpcodeAddress + 7, chatBoxObjPtrBytes);

        }

        private void SetChatText(string str)
        {
            if (_setChatTextOpcodeAddress == 0)
            {
                LoadSetChatTextOpcode();
            }
            UpdateSetChatTextOpcode(str);

            //Run the opcode
            IntPtr threadHandle = MemFunctions.CreateRemoteThread(_client.Handle, _setChatTextOpcodeAddress);
            //Wait for opcode to be done
            MemFunctions.WaitForSingleObject(threadHandle);
            //Close the thread
            MemFunctions.CloseProcess(threadHandle);
        }
        #endregion
        
        #region chatAppendChar
        /**********************************************************************************************************
         * Injects into the clients chatInputKeyHandler() function.
         * This function sends 'keystrokes' to the chat box.
         * This is basically here because I messed up the original function to send a full string
         * to the chat box so thought it didn't work and found this workaround.
         * Might still be useful for something? It accepts backspaces and control characters etc
        **********************************************************************************************************/
        private int _appendCharOpcodeAddress;
        private readonly byte[] _appendCharOpcode = new byte[]
        {
            0x60,                                   // PUSHAD
            0xBF, 0x00, 0x00, 0x00, 0x00,           // MOV EDI, {ASCII key code}
            0x57,                                   // PUSH EDI
            0xBE, 0x00, 0x00, 0x00, 0x00,           // MOV ESI, {chatBoxObjPtr}
            0x8B, 0xCE,                             // MOV ECX,ESI
            0xE8, 0x00, 0x00, 0x00, 0x00,	        // CALL chatBoxKeyHandler()
            0x61,                                   // POPAD
            0xC3                                    // RETN
        };

        private int LoadAppendCharOpcode()
        {
            //Allocate memory for the opcode to call the sendChat function
            _appendCharOpcodeAddress = MemFunctions.AllocateMemory(_client.Handle, _appendCharOpcode.Length);

            //Write the opcode to memory
            MemFunctions.MemWriteBytes(_client.Handle, _appendCharOpcodeAddress, _appendCharOpcode);

            //Calculate relative call address of chatBoxKeyHandler()
            uint relAddress = ChatBoxKeyHandlerCall - (uint)_appendCharOpcodeAddress - 9 - (uint)(_appendCharOpcode.Length / 2);
            //Insert the functionAddress in opcode
            byte[] functionAddress = BitConverter.GetBytes(relAddress);
            MemFunctions.MemWriteBytes(_client.Handle, _appendCharOpcodeAddress + 15, functionAddress);

            // Get and load the pointer to the chat system class
            uint chatEntryBoxObjPtr = MemFunctions.ResolveNestedPointer(_client.Handle, BaseCall, 0x1C, 0x18, 0x8, 0xC4, 0x20, 0x1C4, 0xC, 0xC, 0xC, 0xC, 0xC, 0xC, 0xC, 0xC, 0x8, 0);
            byte[] chatEntryBoxObjPtrBytes = BitConverter.GetBytes(chatEntryBoxObjPtr);
            MemFunctions.MemWriteBytes(_client.Handle, _appendCharOpcodeAddress + 8, chatEntryBoxObjPtrBytes);

            return _appendCharOpcodeAddress;
        }

        public void ChatAppendChar(int addr, byte b)
        {
            byte[] keyStrokeBytes = BitConverter.GetBytes((uint)b);
            MemFunctions.MemWriteBytes(_client.Handle, addr + 2, keyStrokeBytes);

            //Run the opcode
            IntPtr threadHandle = MemFunctions.CreateRemoteThread(_client.Handle, _appendCharOpcodeAddress);
            //Wait for opcode to be done
            MemFunctions.WaitForSingleObject(threadHandle);
            //Close the thread
            MemFunctions.CloseProcess(threadHandle);
        }
        #endregion

        private static class Mem
        {
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


            private enum EncodingType
            {
                ASCII,
                Unicode,
                UTF7,
                UTF8,
                GBK
            }

            public static IntPtr OpenProcess(int pId)
            {
                return OpenProcess(2035711, 0, (UInt32)pId);
            }

            public static byte[] MemReadBytes(IntPtr processHandle, int address, int size)
            {
                var buffer = new byte[size];
                UInt32 nBytesRead = 0;
                ReadProcessMemory(processHandle, (IntPtr)address, buffer, (uint)size, ref nBytesRead);
                return buffer;
            }

            public static unsafe bool MemReadBytesToStruct(IntPtr processHandle, uint address, byte* pBytes, int size)
            {
                var buffer = new byte[size];
                UInt32 nBytesRead = 0;
                bool success = ReadProcessMemory(processHandle, (IntPtr)address, buffer, (uint)size, ref nBytesRead);

                for (int i = 0, j = buffer.Length; i < j; i++)
                {
                    *pBytes++ = buffer[i];
                }
                return success;
            }

            private static uint MemReadUInt(IntPtr processHandle, uint address)
            {
                var buffer = new byte[4];
                UInt32 nBytesRead = 0;
                ReadProcessMemory(processHandle, (IntPtr)address, buffer, 4, ref nBytesRead);
                return BitConverter.ToUInt32(buffer, 0);
            }

            public static string MemReadUnicode(IntPtr processHandle, uint address, uint maxSize, out uint bytesRead)
            {
                var buffer = new byte[maxSize];
                UInt32 nBytesRead = 0;
                ReadProcessMemory(processHandle, (IntPtr)address, buffer, maxSize, ref nBytesRead);
                bytesRead = nBytesRead;
                return ByteArrayToString(buffer, EncodingType.Unicode);
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

                return val + p[p.Length - 1];
            }

            public static string ByteArrayToString(byte[] bytes)
            {
                return ByteArrayToString(bytes, EncodingType.Unicode);
            }

            private static string ByteArrayToString(byte[] bytes, EncodingType encodingType)
            {
                // Redim array to be 2 bytes bigger and fill the last two bytes with 0x00

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
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        private unsafe struct Chatobj
        {
            // Overlay array of bytes like in a C Union to allow faster updating of the whole structure
            [FieldOffset(0x00)]
            public fixed byte bytes[0x1C];
            // Normal fields
            [FieldOffset(0x00)]
            public uint uk1;
            [FieldOffset(0x04)]
            public byte msgScope;    // see messageTypes array
            [FieldOffset(0x05)]
            public byte smileySet;
            [FieldOffset(0x06)]
            public byte uk3;
            [FieldOffset(0x07)]
            public byte uk4;
            [FieldOffset(0x08)]
            public uint p_msg;       // Unformatted message, containing
            // colouring info, name of message sender
            // and message
            [FieldOffset(0x0C)]
            public uint itemId;      // ID of item linked in message
            [FieldOffset(0x10)]
            public uint msgId;       // Unique msg ID
            [FieldOffset(0x14)]
            public uint uk5;
            [FieldOffset(0x18)]
            public uint uk6;
        }
    }
}
