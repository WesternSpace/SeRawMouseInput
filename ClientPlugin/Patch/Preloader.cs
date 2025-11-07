using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

public static class Preloader
{
    public static IEnumerable<string> TargetDLLs { get; } = ["VRage.Platform.Windows.dll", "Sandbox.Game.dll"];

    public static void Patch(ref AssemblyDefinition asmDef)
    {
        var module = asmDef.MainModule;

        if (module.Name == "VRage.Platform.Windows.dll")
        {
            var type = module.Types.First(t => t.Name == "MyWindowsWindows");
            var method = type.Methods.First(m => m.Name == "CreateWindow");

            // Patches the code in MyWindowsWindows.CreateWindow()
            // to not set MyVRagePlatform.Input to the game window. 
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                if (method.Body.Instructions[i].OpCode == OpCodes.Ldfld
                    && method.Body.Instructions[i].Operand is FieldReference fr
                    && fr.Name == "m_platform")
                {
                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i + 2].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i + 3].OpCode = OpCodes.Nop;

                    break;
                }
            }
        }
        else if (module.Name == "Sandbox.Game.dll")
        {
            var type = module.Types.First(t => t.Name == "MySandboxGame");
            var method = type.Methods.First(m => m.Name == "InitializeRenderThread");

            // Removes the call to MySandboxGame.UpdateMouseCapture()
            // as it relies on MyVRagePlatform.Input, which is currently
            // null before plugin runs.
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                if (method.Body.Instructions[i].OpCode == OpCodes.Call
                    && method.Body.Instructions[i].Operand is MethodReference mr
                    && mr.Name == "UpdateMouseCapture")
                {
                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;

                    break;
                }
            }
        }
    }
}
