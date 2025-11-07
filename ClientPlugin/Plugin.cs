using ClientPlugin.Settings;
using ClientPlugin.Settings.Layouts;
using HarmonyLib;
using ParallelTasks;
using Sandbox;
using Sandbox.Engine.Utils;
using Sandbox.Game.Localization;
using Sandbox.Graphics.GUI;
using System;
using System.Reflection;
using VRage;
using VRage.Input;
using VRage.Platform.Windows;
using VRage.Platform.Windows.Forms;
using VRage.Plugins;

namespace ClientPlugin
{
    // ReSharper disable once UnusedType.Global
    public class Plugin : IPlugin, IDisposable
    {
        public const string Name = "SeRawMouseInput";
        public static Plugin Instance { get; private set; }
        private SettingsGenerator settingsGenerator;

        public Plugin()
        {
            Harmony harmony = new Harmony(Name);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            (MyVRage.Platform as MyVRagePlatform).Input = Singleton<WindowsInput>.Instance;
            Singleton<WindowsInput>.Instance.Init(MyVRage.Platform.Windows as MyWindowsWindows);

            MySandboxGame.Static.UpdateMouseCapture();

            // Reinitialize MyInput

            var gamecontrols = (MyInput.Static as MyVRageInput).m_defaultGameControlsList;

            MyInput.UnloadData();
            MyInput.Initialize(new MyVRageInput(MyVRage.Platform.Input, new MyKeysToString(), gamecontrols, MyFakes.ENABLE_F12_MENU, delegate
            {
                MyFakes.ENABLE_F12_MENU = true;
            }));
            MyInput.Static.SetMousePositionScale(MySandboxGame.Config.SpriteMainViewportScale);

            MyInput.Static.LoadContent();
            MyInput.Static.LoadData(MySandboxGame.Config.ControlsGeneral, MySandboxGame.Config.ControlsButtons);
            MySandboxGame.Static.InitJoystick();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void Init(object gameInstance)
        {
            Instance = this;
            Instance.settingsGenerator = new SettingsGenerator();

            // TODO: Put your one time initialization code here.
        }

        public void Dispose()
        {
            // TODO: Save state and close resources here, called when the game exits (not guaranteed!)
            // IMPORTANT: Do NOT call harmony.UnpatchAll() here! It may break other plugins.

            Instance = null;
        }

        public void Update()
        {
            // TODO: Put your update code here. It is called on every simulation frame!
        }

        // ReSharper disable once UnusedMember.Global
        public void OpenConfigDialog()
        {
            Instance.settingsGenerator.SetLayout<Simple>();
            MyGuiSandbox.AddScreen(Instance.settingsGenerator.Dialog);
        }

        //TODO: Uncomment and use this method to load asset files
        /*public void LoadAssets(string folder)
        {

        }*/
    }
}