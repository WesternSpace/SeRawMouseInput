using ClientPlugin.Patch;
using ParallelTasks;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VRage;
using VRage.Input;
using VRage.Utils;
using static ClientPlugin.WindowsInput;

namespace ClientPlugin
{
    internal static class RawInput
    {
        public const int WM_INPUT = 0x00FF;
        private const int RID_INPUT = 0x10000003;
        private const int RIM_TYPEMOUSE = 0;
        private const int RIM_TYPEKEYBOARD = 1;
        public const ushort HID_USAGE_PAGE_GENERIC = 0x01;
        public const ushort HID_USAGE_GENERIC_MOUSE = 0x02;
        public const ushort HID_USAGE_GENERIC_KEYBOARD = 0x06;

        private const ushort RI_MOUSE_WHEEL = 0x0400;
        public const uint RIDEV_INPUTSINK = 0x00000100;
        public const ushort RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001;
        public const ushort RI_MOUSE_LEFT_BUTTON_UP = 0x0002;
        public const ushort RI_MOUSE_RIGHT_BUTTON_DOWN = 0x0004;
        public const ushort RI_MOUSE_RIGHT_BUTTON_UP = 0x0008;
        public const ushort RI_MOUSE_MIDDLE_BUTTON_DOWN = 0x0010;
        public const ushort RI_MOUSE_MIDDLE_BUTTON_UP = 0x0020;
        public const ushort RI_MOUSE_BUTTON_4_DOWN = 0x0040;
        public const ushort RI_MOUSE_BUTTON_4_UP = 0x0080;
        public const ushort RI_MOUSE_BUTTON_5_DOWN = 0x0100;
        public const ushort RI_MOUSE_BUTTON_5_UP = 0x0200;

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct RAWMOUSE
        {
            [FieldOffset(0)]
            public ushort usFlags;
            [FieldOffset(4)]
            public uint ulButtons;
            [FieldOffset(4)]
            public ushort usButtonFlags;
            [FieldOffset(6)]
            public ushort usButtonData;
            [FieldOffset(8)]
            public uint ulRawButtons;
            [FieldOffset(12)]
            public int lLastX;
            [FieldOffset(16)]
            public int lLastY;
            [FieldOffset(20)]
            public uint ulExtraInformation;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct RAWINPUT
        {
            [FieldOffset(0)]
            public RAWINPUTHEADER header;
            [FieldOffset(24)]
            public RAWMOUSE mouse;
            [FieldOffset(24)]
            public RAWKEYBOARD keyboard;
        }

        [DllImport("User32.dll")]
        public static extern bool RegisterRawInputDevices(
        [In] RAWINPUTDEVICE[] pRawInputDevices,
        uint uiNumDevices,
        uint cbSize);

        [DllImport("User32.dll")]
        static extern uint GetRawInputData(
            IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader);

        public static Mutex LockMutexMouse = new();

        public static MyMouseState MouseState = default;

        private static int prevX = 0;
        private static int prevY = 0;

        public static void WinProc(ref Message msg)
        {
            if (Singleton<WindowsInput>.Instance.ActiveMouseType != MouseType.Raw)
            {
                return;
            }

            uint dwSize = 0;
            GetRawInputData(msg.LParam, RID_INPUT, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));

            if (dwSize == 0)
            {
                return;
            }

            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);

            try
            {
                if (GetRawInputData(msg.LParam, RID_INPUT, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) == dwSize)
                {
                    RAWINPUT rawInput = Marshal.PtrToStructure<RAWINPUT>(buffer);

                    if (rawInput.header.dwType == RIM_TYPEKEYBOARD)
                    {
                        return;
                    }
                    else if (rawInput.header.dwType == RIM_TYPEMOUSE)
                    {
                        LockMutexMouse.WaitOne();

                        ProcessMouseData(rawInput.mouse, ref MouseState);

                        LockMutexMouse.ReleaseMutex();
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static MyMouseState ProcessMouseData(RAWMOUSE data, ref MyMouseState state)
        {
            if ((data.usFlags & 0x01) != 0) // absolute
            {
                int absX = data.lLastX;
                int absY = data.lLastY;

                if ((data.usFlags & 0x02) != 0)
                {
                    absX = (int)((float)absX / 65535 * SystemInformation.VirtualScreen.Width);
                    absY = (int)((float)absY / 65535 * SystemInformation.VirtualScreen.Height);
                }

                state.X += (absX - prevX);
                state.Y += (absY - prevY);

                prevX = absX;
                prevY = absY;
            }
            else
            {
                state.X += data.lLastX;
                state.Y += data.lLastY;
            }

            if ((data.usButtonFlags & RI_MOUSE_WHEEL) != 0)
            {
                state.ScrollWheelValue += (short)data.usButtonData;
            }

            if ((data.usButtonFlags & RI_MOUSE_LEFT_BUTTON_UP) != 0)
            {
                state.LeftButton = false;
            }
            if ((data.usButtonFlags & RI_MOUSE_LEFT_BUTTON_DOWN) != 0)
            {
                state.LeftButton = true;
            }

            if ((data.usButtonFlags & RI_MOUSE_RIGHT_BUTTON_UP) != 0)
            {
                state.RightButton = false;
            }
            if ((data.usButtonFlags & RI_MOUSE_RIGHT_BUTTON_DOWN) != 0)
            {
                state.RightButton = true;
            }

            if ((data.usButtonFlags & RI_MOUSE_MIDDLE_BUTTON_UP) != 0)
            {
                state.MiddleButton = false;
            }
            if ((data.usButtonFlags & RI_MOUSE_MIDDLE_BUTTON_DOWN) != 0)
            {
                state.MiddleButton = true;
            }

            if ((data.usButtonFlags & RI_MOUSE_BUTTON_4_UP) != 0)
            {
                state.XButton1 = false;
            }
            if ((data.usButtonFlags & RI_MOUSE_BUTTON_4_DOWN) != 0)
            {
                state.XButton1 = true;
            }

            if ((data.usButtonFlags & RI_MOUSE_BUTTON_5_UP) != 0)
            {
                state.XButton2 = false;
            }
            if ((data.usButtonFlags & RI_MOUSE_BUTTON_5_DOWN) != 0)
            {
                state.XButton2 = true;
            }

            return state;
        }
    }
}
