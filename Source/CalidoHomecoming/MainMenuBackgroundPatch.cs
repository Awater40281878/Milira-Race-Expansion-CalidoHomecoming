using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;

namespace CalidoHomecoming
{
    // Keeps the main-menu replacement entirely inside this mod. The field is
    // reset by RimWorld whenever the menu is rebuilt, so we apply it whenever
    // MainMenuDrawer provides its background instance.
    [HarmonyPatch(typeof(MainMenuDrawer), "get_BackgroundMain")]
    public static class CalidoMainMenuBackgroundPatch
    {
        private static readonly FieldInfo OverrideBackgroundField = AccessTools.Field(typeof(UI_BackgroundMain), "overrideBGImage");
        private static Texture2D protagonistBackground;

        public static void Postfix(UI_BackgroundMain __result)
        {
            if (__result == null || OverrideBackgroundField == null) return;
            protagonistBackground ??= ContentFinder<Texture2D>.Get("UI/MainMenu/CalidoProtagonist", true);
            if (protagonistBackground != null)
                OverrideBackgroundField.SetValue(__result, protagonistBackground);
        }
    }
}
