using HarmonyLib;
using DriveIt.UI;

namespace DriveIt.Patches
{
    [HarmonyPatch]
    internal class KeyOverrideHandler
    {
        [HarmonyPatch(typeof(GameKeyShortcuts), "Escape")]
        [HarmonyPrefix]
        static bool Prefix1() => EscPatch();

        [HarmonyPatch(typeof(GameKeyShortcuts), "SteamEscape")]
        [HarmonyPrefix]
        static bool Prefix2() => EscPatch();

        [HarmonyPatch(typeof(KeyShortcuts), "SimulationPause")]
        [HarmonyPrefix]
        static bool Prefix3() => PausePatch();

        static bool EscPatch() =>
            // cancel calling Escape() if DriveIt consumes it
            !DriveController.instance.OnEsc() && !MainPanel.instance.OnEsc();

        static bool PausePatch() =>
            // cancel calling SimulationPause() if DriveIt consumes it
            !DriveCam.instance.OnPause();
    }
}