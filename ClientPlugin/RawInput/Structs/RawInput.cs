using System.Runtime.InteropServices;

namespace ClientPlugin.RawInput.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    struct RawInput
    {
        [FieldOffset(0)]
        public RawInputHeader header;
        [FieldOffset(24)]
        public RawMouse mouse;
        [FieldOffset(24)]
        public RawKeyboard keyboard;
    }
}
