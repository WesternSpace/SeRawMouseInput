using HarmonyLib;
using VRage.Input;
using VRage.Platform.Windows;

namespace ClientPlugin.Patch
{
    [HarmonyPatch(typeof(MyVRagePlatform), "Input2", MethodType.Getter)]
    internal class MyVRagePlatform_Input2_Getter_Patch
    {
        // This just forwards IVRageInput2 to the IVRageInput instace
        // For our purposes, this is WindowsInput, which implements IVRageInput2 as well
        private static bool Prefix(MyVRagePlatform __instance, ref IVRageInput2 __result)
        {
            __result = (WindowsInput)__instance.Input;
            return false;
        }
    }
}
