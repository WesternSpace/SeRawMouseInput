using ClientPlugin.Settings;
using HarmonyLib;
using Sandbox.Game.Gui;

namespace ClientPlugin.Patch
{
    [HarmonyPatch(typeof(MyGuiScreenOptionsControls), "CloseScreenAndSave")]
    internal class MyGuiScreenOptionsControls_CloseScreenAndSave_Patch
    {
        private static void Prefix()
        {
            Config.Current.MouseMode = (WindowsInput.MouseMode)MyGuiScreenOptionsControls_AddGeneralControls_Patch.MouseModeCombobox.GetSelectedIndex();
            ConfigStorage.Save(Config.Current);
        }
    }
}
