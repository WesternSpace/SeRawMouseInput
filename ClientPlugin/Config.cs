using ClientPlugin.Settings;
using ClientPlugin.Settings.Elements;
using ClientPlugin.Settings.Tools;
using ParallelTasks;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using VRage.Input;
using VRageMath;


namespace ClientPlugin
{
    public class Config : INotifyPropertyChanged
    {
        #region Options

        private WindowsInput.MouseMode mouseMode = WindowsInput.MouseMode.Raw;

        #endregion

        #region User interface

        public readonly string Title = "SE Raw Input Settings";

        [Separator]

        [Dropdown(label: "Mouse Mode")]
        public WindowsInput.MouseMode MouseMode
        {
            get => mouseMode;
            set
            {
                SetField(ref mouseMode, value);
                Singleton<WindowsInput>.Instance.ActiveMouseMode = value;
            }
        }

        [Separator]

        [Button(label: "Ok")]
        public void Button()
        {
            MyScreenManager.CloseScreen(typeof(SettingsScreen));
        }

        #endregion

        #region Property change notification boilerplate

        public static readonly Config Default = new Config();
        public static readonly Config Current = ConfigStorage.Load();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}