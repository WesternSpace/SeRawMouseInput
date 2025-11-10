using ClientPlugin.RawInput.Enums;
using ClientPlugin.RawInput.Structs;
using ParallelTasks;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using VRage.Input;

namespace ClientPlugin.RawInput
{
    internal static class RawInputApi
    {
        public const int WM_INPUT = 0x00FF;

        public static Mutex LockMutexMouse = new();

        public static MyMouseState MouseState = default;

        private static int prevX = 0;
        private static int prevY = 0;

        public static void WinProc(ref Message msg)
        {
            if (Singleton<WindowsInput>.Instance.ActiveMouseType != WindowsInput.MouseType.Raw)
            {
                return;
            }

            uint dwSize = 0;
            RawInputNative.GetRawInputData(msg.LParam, RawInputDataType.RID_INPUT, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(RawInputHeader)));

            if (dwSize == 0)
            {
                return;
            }

            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);

            try
            {
                if (RawInputNative.GetRawInputData(msg.LParam, RawInputDataType.RID_INPUT, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(RawInputHeader))) == dwSize)
                {
                    Structs.RawInput rawInput = Marshal.PtrToStructure<Structs.RawInput>(buffer);

                    if (rawInput.header.dwType == RawInputDeviceType.RIM_TYPEKEYBOARD)
                    {
                        return;
                    }
                    else if (rawInput.header.dwType == RawInputDeviceType.RIM_TYPEMOUSE)
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

        private static MyMouseState ProcessMouseData(RawMouse data, ref MyMouseState state)
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

                state.X += absX - prevX;
                state.Y += absY - prevY;

                prevX = absX;
                prevY = absY;
            }
            else
            {
                state.X += data.lLastX;
                state.Y += data.lLastY;
            }

            if ((data.usButtonFlags & MouseTransitionState.RI_MOUSE_WHEEL) != 0)
            {
                state.ScrollWheelValue += (short)data.usButtonData;
            }

            if ((data.usButtonFlags & MouseTransitionState.RI_MOUSE_LEFT_BUTTON_UP) != 0)
            {
                state.LeftButton = false;
            }
            if ((data.usButtonFlags & MouseTransitionState.RI_MOUSE_LEFT_BUTTON_DOWN) != 0)
            {
                state.LeftButton = true;
            }

            if ((data.usButtonFlags & MouseTransitionState.RI_MOUSE_RIGHT_BUTTON_UP) != 0)
            {
                state.RightButton = false;
            }
            if ((data.usButtonFlags & MouseTransitionState.RI_MOUSE_RIGHT_BUTTON_DOWN) != 0)
            {
                state.RightButton = true;
            }

            if ((data.usButtonFlags & MouseTransitionState.RI_MOUSE_MIDDLE_BUTTON_UP) != 0)
            {
                state.MiddleButton = false;
            }
            if ((data.usButtonFlags & MouseTransitionState.RI_MOUSE_MIDDLE_BUTTON_DOWN) != 0)
            {
                state.MiddleButton = true;
            }

            if ((data.usButtonFlags & MouseTransitionState.RI_MOUSE_BUTTON_4_UP) != 0)
            {
                state.XButton1 = false;
            }
            if ((data.usButtonFlags & MouseTransitionState.RI_MOUSE_BUTTON_4_DOWN) != 0)
            {
                state.XButton1 = true;
            }

            if ((data.usButtonFlags & MouseTransitionState.RI_MOUSE_BUTTON_5_UP) != 0)
            {
                state.XButton2 = false;
            }
            if ((data.usButtonFlags & MouseTransitionState.RI_MOUSE_BUTTON_5_DOWN) != 0)
            {
                state.XButton2 = true;
            }

            return state;
        }
    }
}
