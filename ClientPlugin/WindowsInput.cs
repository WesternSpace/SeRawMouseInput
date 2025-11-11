using ClientPlugin.RawInput;
using ClientPlugin.RawInput.Enums;
using ClientPlugin.RawInput.Structs;
using EmptyKeys.UserInterface.Input;
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

namespace ClientPlugin
{
    public class WindowsInput : IVRageInput, IVRageInput2
    {
        public enum KeyboardMode
        {
            Raw,
            DirectInput,
            Windows
        }

        public enum MouseMode
        {
            Raw,
            DirectInput,
            //Windows
        }

        private MyWindowsWindows windowManager;

        private KeyboardMode activeKeyboardMode = KeyboardMode.DirectInput;
        private MouseMode activeMouseMode = MouseMode.Raw;

        public KeyboardMode ActiveKeyboardMode
        {
            get
            {
                return activeKeyboardMode;
            }
            set
            {
                activeKeyboardMode = value;
                OnKeyboardModeChanged(value);
            }
        }

        public MouseMode ActiveMouseMode
        {
            get
            {
                return activeMouseMode;
            }
            set
            {
                activeMouseMode = value;
                OnMouseModeChanged(value);
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
            directInput = new MyDirectInput(windows);
            OnKeyboardModeChanged(activeKeyboardMode);
            OnMouseModeChanged(activeMouseMode);
            
            IsCorrectlyInitialized = directInput.IsCorrectlyInitialized;
        }

        private void OnKeyboardModeChanged(KeyboardMode keyboardMode)
        {
            if (keyboardMode == KeyboardMode.Raw)
            {
                RawInputDevice[] device = new RawInputDevice[1];
                device[0].usUsagePage = HidUsagePage.HID_USAGE_PAGE_GENERIC;
                device[0].usUsage = HidUsageId.HID_USAGE_GENERIC_KEYBOARD;

                if (RawInputNative.RegisterRawInputDevices(device, 1, (uint)Marshal.SizeOf(typeof(RawInputDevice))))
                {
                    return;
                }

                keyboardMode = KeyboardMode.DirectInput;
            }
        }

        private void OnMouseModeChanged(MouseMode mouseMode)
        {
            if (mouseMode == MouseMode.Raw)
            {
                if (directInput.m_mouse != null)
                {
                    directInput.m_mouse.Dispose();
                    directInput.m_mouse = null;
                }

                RawInputDevice[] device = new RawInputDevice[1];
                device[0].usUsagePage = HidUsagePage.HID_USAGE_PAGE_GENERIC;
                device[0].usUsage = HidUsageId.HID_USAGE_GENERIC_MOUSE;

                if (RawInputNative.RegisterRawInputDevices(device, 1, (uint)Marshal.SizeOf(typeof(RawInputDevice))))
                {
                    return;
                }

                mouseMode = MouseMode.DirectInput;
            }

            if (mouseMode == MouseMode.DirectInput && directInput.m_mouse == null)
            {
                directInput.m_mouse = new(directInput.m_directInput);
                directInput.m_mouse.SetCooperativeLevel(windowManager.WindowHandle, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground);
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
            if (activeMouseMode == MouseMode.Raw)
            {
                RawInputApi.LockMutexMouse.WaitOne();

                state = RawInputApi.MouseState;

                RawInputApi.MouseState.X = 0;
                RawInputApi.MouseState.Y = 0;
                RawInputApi.MouseState.ScrollWheelValue = 0;

                RawInputApi.LockMutexMouse.ReleaseMutex();

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
