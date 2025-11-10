using System.Runtime.InteropServices;

namespace ClientPlugin.RawInput.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    struct RawKeyboard
    {
        public ushort MakeCode;
        public ushort Flags;
        public ushort Reserved;
        public ushort VKey;
        public uint Message;
        public uint ExtraInformation;
    }
}
