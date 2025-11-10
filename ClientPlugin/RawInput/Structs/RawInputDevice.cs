using ClientPlugin.RawInput.Enums;
using System;
using System.Runtime.InteropServices;

namespace ClientPlugin.RawInput.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    struct RawInputDevice
    {
        public HidUsagePage usUsagePage;
        public HidUsageId usUsage;
        public uint dwFlags;
        public IntPtr hwndTarget;
    }
}
