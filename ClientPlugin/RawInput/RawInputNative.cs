using ClientPlugin.RawInput.Enums;
using ClientPlugin.RawInput.Structs;
using System;
using System.Runtime.InteropServices;

namespace ClientPlugin.RawInput
{
    internal class RawInputNative
    {
        [DllImport("User32.dll")]
        public static extern bool RegisterRawInputDevices(
        [In] RawInputDevice[] pRawInputDevices,
        uint uiNumDevices,
        uint cbSize);

        [DllImport("User32.dll")]
        public static extern uint GetRawInputData(
            IntPtr hRawInput,
            RawInputDataType uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader);
    }
}
