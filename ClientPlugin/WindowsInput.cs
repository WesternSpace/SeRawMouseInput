using ParallelTasks;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VRage;
using VRage.Input;
using VRage.Platform.Windows.Forms;
using VRage.Platform.Windows.Input;
using VRageMath;
using static ClientPlugin.WindowsInput;

namespace ClientPlugin
{
    internal class WindowsInput : IVRageInput, IVRageInput2
    {
        public enum KeyboardType
        {
            Raw,
            DirectInput,
            Windows
        }

        public enum MouseType
        {
            Raw,
            DirectInput,
            Windows
        }

        private MyWindowsWindows windowManager;

        private KeyboardType activeKeyboardType = KeyboardType.DirectInput;
        private MouseType activeMouseType = MouseType.Raw;

        public KeyboardType ActiveKeyboardType
        {
            get
            {
                return activeKeyboardType;
            }
            set
            {
                activeKeyboardType = value;
                OnKeyboardTypeChanged(value);
            }
        }

        public MouseType ActiveMouseType
        {
            get
            {
                return activeMouseType;
            }
            set
            {
                activeMouseType = value;
                OnMouseTypeChanged(value);
            }
        }

        public Vector2 MousePosition 
        {
            get => (windowManager.Window as IVRageInput).MousePosition;
            set => (windowManager.Window as IVRageInput).MousePosition = value;
        }

        public Vector2 MouseAreaSize => (windowManager.Window as IVRageInput).MouseAreaSize;

        public bool MouseCapture    
        { 
            get => (windowManager.Window as IVRageInput).MouseCapture; 
            set => (windowManager.Window as IVRageInput).MouseCapture = value;
        }

        public bool ShowCursor 
        { 
            get => (windowManager.Window as MyGameWindow).ShowCursor; 
            set => (windowManager.Window as MyGameWindow).ShowCursor = value; 
        }

        public int KeyboardDelay => SystemInformation.KeyboardDelay;
        public int KeyboardSpeed => SystemInformation.KeyboardSpeed;

        public uint[] DeveloperKeys => [0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF];

        public bool IsCorrectlyInitialized { get; private set; }

        // To be removed in future version of plugin 
        private MyDirectInput directInput;

        public void Init(MyWindowsWindows windows)
        {
            windowManager = windows;
            OnKeyboardTypeChanged(activeKeyboardType);
            OnMouseTypeChanged(activeMouseType);
            directInput = new MyDirectInput(windows);
            IsCorrectlyInitialized = directInput.IsCorrectlyInitialized;
        }

        private void OnKeyboardTypeChanged(KeyboardType keyboardType)
        {
            if (keyboardType == KeyboardType.Raw)
            {
                RawInput.RAWINPUTDEVICE[] device = new RawInput.RAWINPUTDEVICE[1];
                device[0].usUsagePage = RawInput.HID_USAGE_PAGE_GENERIC;
                device[0].usUsage = RawInput.HID_USAGE_GENERIC_KEYBOARD;

                if (RawInput.RegisterRawInputDevices(device, 1, (uint)Marshal.SizeOf(typeof(RawInput.RAWINPUTDEVICE))))
                {
                    return;
                }

                keyboardType = KeyboardType.DirectInput;
            }
        }

        private void OnMouseTypeChanged(MouseType mouseType)
        {
            if (mouseType == MouseType.Raw)
            {
                RawInput.RAWINPUTDEVICE[] device = new RawInput.RAWINPUTDEVICE[1];
                device[0].usUsagePage = RawInput.HID_USAGE_PAGE_GENERIC;
                device[0].usUsage = RawInput.HID_USAGE_GENERIC_MOUSE;

                if (RawInput.RegisterRawInputDevices(device, 1, (uint)Marshal.SizeOf(typeof(RawInput.RAWINPUTDEVICE))))
                {
                    return;
                }

                mouseType = MouseType.DirectInput;
            }
        }

        public void AddChar(char ch)
        {
            (windowManager.Window as IVRageInput).AddChar(ch);
        }

        public void Dispose()
        {
            directInput.Dispose();
        }

        public List<string> EnumerateJoystickNames()
        {
            return directInput.EnumerateJoystickNames();
        }

        public unsafe void GetAsyncKeyStates(byte* data)
        {
            directInput.GetAsyncKeyStates(data);
        }

        public void GetBufferedTextInput(ref List<char> currentTextInput)
        {
            (windowManager.Window as IVRageInput).GetBufferedTextInput(ref currentTextInput);
        }

        public void GetJoystickState(ref MyJoystickState state)
        {
            directInput.GetJoystickState(ref state);
        }

        public void GetMouseState(out MyMouseState state)
        {
            if (activeMouseType == MouseType.Raw)
            {
                RawInput.LockMutexMouse.WaitOne();

                state = RawInput.MouseState;

                RawInput.MouseState.X = 0;
                RawInput.MouseState.Y = 0;
                RawInput.MouseState.ScrollWheelValue = 0;

                RawInput.LockMutexMouse.ReleaseMutex();

                return;
            }

            directInput.GetMouseState(out state);
        }

        public string InitializeJoystickIfPossible(string joystickInstanceName)
        {
            return directInput.InitializeJoystickIfPossible(joystickInstanceName);
        }

        public bool IsJoystickAxisSupported(MyJoystickAxesEnum axis)
        {
            return directInput.IsJoystickAxisSupported(axis);
        }

        public bool IsJoystickConnected()
        {
            return directInput.IsJoystickConnected();
        }

        public void ShowVirtualKeyboardIfNeeded(Action<string> onSuccess, Action onCancel = null, string defaultText = null, string title = null, int maxLength = 0)
        {
            directInput.ShowVirtualKeyboardIfNeeded(onSuccess, onCancel, defaultText, title, maxLength);  
        }
    }
}
