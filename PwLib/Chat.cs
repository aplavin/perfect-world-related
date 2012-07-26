using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using PwLib.Structs;

namespace PwLib
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

    public class Chat
    {
        private const int FetchCount = 10;

        private const int ChatObjectsBase = 0xAFBEE8; // v551
        private const int LastChatObject = 0xAFBEF4; // v551
        private const int SendChatCall = 0x7E8930; // v551 sendChat function address
        private const int ChatBoxKeyHandlerCall = 0x808360; // v551 chatBoxKeyHandler function address

        private readonly PwClient _client;

        private List<ChatMessage> _lastMessages;

        public delegate void NewMessageEventHandler(ChatMessage message);
        public event NewMessageEventHandler NewMessage;

        public Chat(PwClient client)
        {
            _client = client;

            _lastMessages = new List<ChatMessage>();
            var timer = new Timer(s =>
            {
                if (NewMessage == null)
                    return;

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

            // Static base pointer to chat object array
            int chatBasePointer = _client.Mem.ResolveNestedPointer(ChatObjectsBase, 0);

            // Number between 0-199 (Number of messages that can be shown in the ingame chat window)
            int lastChatOffset = _client.Mem.ResolveNestedPointer(LastChatObject, 0);

            // Calculate index of first message to show
            int firstChatToShow = FetchCount > lastChatOffset ? 0 : (lastChatOffset - FetchCount);
            for (int chatOffset = firstChatToShow; chatOffset < lastChatOffset; chatOffset++)
            {
                var chatObj = _client.Mem.ReadStruct<ChatMessageStruct>(chatBasePointer + (0x1C * chatOffset));

                // Actual message entry in object is just a pointer, so go retrieve the message
                string str = _client.Mem.ReadString(256, (int)chatObj.pMsg).Replace("\r", "\r\n");

                list.Add(new ChatMessage
                {
                    Id = chatObj.msgId,
                    Scope = (Scopes)chatObj.MsgScope,
                    Raw = str,
                    ItemId = chatObj.ItemId,
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
            IntPtr threadHandle = _client.Mem.CreateRemoteThread(_sendChatOpcodeAddress);
            //Wait for opcode to be done
            _client.Mem.WaitForSingleObject(threadHandle);
            //Close the thread
            _client.Mem.CloseHandle(threadHandle);
        }

        private void LoadSendChatOpcode()
        {
            //Allocate memory for the opcode to call the sendChat function
            _sendChatOpcodeAddress = _client.Mem.AllocateMemory(_sendChatOpcode.Length);
            //Write the opcode to memory
            _client.Mem.WriteBytes(_sendChatOpcode, _sendChatOpcodeAddress);

            //Calculate relative call address of sendChat()
            uint relAddress = SendChatCall - (uint)_sendChatOpcodeAddress - 12 - (uint)(_sendChatOpcode.Length / 2);
            //Insert the functionAddress in opcode
            byte[] functionAddress = BitConverter.GetBytes(relAddress);
            _client.Mem.WriteBytes(functionAddress, _sendChatOpcodeAddress + 22);

            int chatClassPtr = _client.Mem.ResolveNestedPointer(_client.Addresses.BaseAddress, 0x1C, 0x18, 0x8, 0xC4, 0x20, 0);
            byte[] chatClassPtrBytes = BitConverter.GetBytes(chatClassPtr);
            _client.Mem.WriteBytes(chatClassPtrBytes, _sendChatOpcodeAddress + 13);
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
            _setChatTextOpcodeAddress = _client.Mem.AllocateMemory(_setChatTextOpcode.Length);
            //Write the opcode to memory
            _client.Mem.WriteBytes(_setChatTextOpcode, _setChatTextOpcodeAddress);
            // @TODO: Get the 0x44 with regex? (CALL DWORD PTR DS:[EAX+44])
        }

        // Just updates the variable information in the opcode without reallocating the whole opcode
        // to another memory location
        private void UpdateSetChatTextOpcode(string str)
        {
            Encoding unicode = Encoding.Unicode;
            byte[] unicodeBytes = unicode.GetBytes(str);

            //Allocate memory for the chat message
            int chatMsgAddress = _client.Mem.AllocateMemory(unicodeBytes.Length);
            // Write the message to memory
            _client.Mem.WriteBytes(unicodeBytes, chatMsgAddress);

            // Write the address of the string pointer
            byte[] stringAddr = BitConverter.GetBytes(chatMsgAddress);
            _client.Mem.WriteBytes(stringAddr, _setChatTextOpcodeAddress + 2);

            // Get the pointer to the chat input box object
            int chatBoxObjPtr = _client.Mem.ResolveNestedPointer(_client.Addresses.BaseAddress, 0x1C, 0x18, 0x8, 0xC4, 0x20, 0x1C4, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x08, 0);
            byte[] chatBoxObjPtrBytes = BitConverter.GetBytes(chatBoxObjPtr);
            _client.Mem.WriteBytes(chatBoxObjPtrBytes, _setChatTextOpcodeAddress + 7);

        }

        private void SetChatText(string str)
        {
            if (_setChatTextOpcodeAddress == 0)
            {
                LoadSetChatTextOpcode();
            }
            UpdateSetChatTextOpcode(str);

            //Run the opcode
            IntPtr threadHandle = _client.Mem.CreateRemoteThread(_setChatTextOpcodeAddress);
            //Wait for opcode to be done
            _client.Mem.WaitForSingleObject(threadHandle);
            //Close the thread
            _client.Mem.CloseHandle(threadHandle);
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
            _appendCharOpcodeAddress = _client.Mem.AllocateMemory(_appendCharOpcode.Length);

            //Write the opcode to memory
            _client.Mem.WriteBytes(_appendCharOpcode, _appendCharOpcodeAddress);

            //Calculate relative call address of chatBoxKeyHandler()
            uint relAddress = ChatBoxKeyHandlerCall - (uint)_appendCharOpcodeAddress - 9 - (uint)(_appendCharOpcode.Length / 2);
            //Insert the functionAddress in opcode
            byte[] functionAddress = BitConverter.GetBytes(relAddress);
            _client.Mem.WriteBytes(functionAddress, _appendCharOpcodeAddress + 15);

            // Get and load the pointer to the chat system class
            int chatEntryBoxObjPtr = _client.Mem.ResolveNestedPointer(_client.Addresses.BaseAddress, 0x1C, 0x18, 0x8, 0xC4, 0x20, 0x1C4, 0xC, 0xC, 0xC, 0xC, 0xC, 0xC, 0xC, 0xC, 0x8, 0);
            byte[] chatEntryBoxObjPtrBytes = BitConverter.GetBytes(chatEntryBoxObjPtr);
            _client.Mem.WriteBytes(chatEntryBoxObjPtrBytes, _appendCharOpcodeAddress + 8);

            return _appendCharOpcodeAddress;
        }

        public void ChatAppendChar(int addr, byte b)
        {
            byte[] keyStrokeBytes = BitConverter.GetBytes((uint)b);
            _client.Mem.WriteBytes(keyStrokeBytes, addr + 2);

            //Run the opcode
            IntPtr threadHandle = _client.Mem.CreateRemoteThread(_appendCharOpcodeAddress);
            //Wait for opcode to be done
            _client.Mem.WaitForSingleObject(threadHandle);
            //Close the thread
            _client.Mem.CloseHandle(threadHandle);
        }
        #endregion
    }
}
