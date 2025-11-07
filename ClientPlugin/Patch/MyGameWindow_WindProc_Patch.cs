using HarmonyLib;
using System.Windows.Forms;
using VRage.Platform.Windows.Forms;

namespace ClientPlugin.Patch
{
    [HarmonyPatch(typeof(MyGameWindow), "WndProc")]
    internal class MyGameWindow_WindProc_Patch
    {
        private static bool Prefix(ref Message m)
        {
            if (m.Msg != RawInput.WM_INPUT)
                return true;

            RawInput.WinProc(ref m);
            return false;
        }
    }
}
