using HarmonyLib;
using Sandbox.Game.Gui;
using Sandbox.Graphics.GUI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace ClientPlugin.Patch
{
    [HarmonyPatch(typeof(MyGuiScreenOptionsControls), "AddGeneralControls")]
    internal class MyGuiScreenOptionsControls_AddGeneralControls_Patch
    {
        public static MyGuiControlCombobox MouseModeCombobox { get; private set; }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var il = instructions.ToList();

            for (int i = 0; i < il.Count; i++)
            {
                if (il[i].opcode == OpCodes.Add 
                    && il[i + 1].opcode == OpCodes.Stloc_1
                    && il[i + 2].opcode == OpCodes.Ldarg_0)
                {
                    List<CodeInstruction> patch = [];
                    patch.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    patch.Add(new CodeInstruction(OpCodes.Ldloc_1));
                    patch.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MyGuiScreenOptionsControls_AddGeneralControls_Patch), "Patch")));
                    patch.Add(new CodeInstruction(OpCodes.Ldloc_1));
                    patch.Add(new CodeInstruction(OpCodes.Ldc_R4, 0.97f));
                    patch.Add(new CodeInstruction(OpCodes.Add));
                    patch.Add(new CodeInstruction(OpCodes.Stloc_1));

                    il.InsertRange(i + 3, patch);

                    break;
                }
            }

            return il;
        }

        public static void Patch(MyGuiScreenOptionsControls __instance, float offset)
        {
            __instance.m_allControls[MyGuiControlTypeEnum.General].Add(new MyGuiControlLabel(new Vector2?(__instance.m_controlsOriginLeft + offset * MyGuiConstants.CONTROLS_DELTA), null, "Mouse Type", null, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, float.PositiveInfinity, false, 0.7f));

            if (MouseModeCombobox == null)
            {
                MouseModeCombobox = new(__instance.m_controlsOriginRight + offset * MyGuiConstants.CONTROLS_DELTA - new Vector2(455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X / 2f, 0f));
                MouseModeCombobox.AddItem((long)WindowsInput.MouseMode.Raw, new StringBuilder("Raw"));
                MouseModeCombobox.AddItem((long)WindowsInput.MouseMode.DirectInput, new StringBuilder("Direct Input"));
                MouseModeCombobox.Size = new Vector2(452f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 0f);
            }

            __instance.m_allControls[MyGuiControlTypeEnum.General].Add(MouseModeCombobox);
            MouseModeCombobox.SelectItemByIndex((int)Config.Current.MouseMode);
        }
    }
}
