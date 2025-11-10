using ClientPlugin.RawInput.Enums;
using System;
using System.Runtime.InteropServices;

namespace ClientPlugin.RawInput.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    struct RawInputHeader
    {
        public RawInputDeviceType dwType;
        public uint dwSize;
        public IntPtr hDevice;
        public IntPtr wParam;
    }
}
