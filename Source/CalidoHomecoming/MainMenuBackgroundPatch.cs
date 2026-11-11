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
        [HarmonyPatch(typeof(UI_BackgroundMain), "BackgroundOnGUI")]
        public static class MenuPortraitLayout
        {
            private static Texture2D portrait;

            public static void Postfix(UI_BackgroundMain __instance)
            {
                if (Event.current.type != EventType.Repaint || protagonistBackground == null ||
                    OverrideBackgroundField.GetValue(__instance) != protagonistBackground) return;
                portrait ??= ContentFinder<Texture2D>.Get("UI/MainMenu/CalidoPortraitLayer", true);
                if (portrait == null) return;

                float w = UI.screenWidth;
                float h = UI.screenHeight;
                // Reuse the unobstructed sky half behind the separately positioned figure.
                GUI.DrawTextureWithTexCoords(new Rect(0, 0, w / 2, h), protagonistBackground,
                    new Rect(1f, 0f, -0.5f, 1f));
                GUI.DrawTextureWithTexCoords(new Rect(w / 2, 0, w / 2, h), protagonistBackground,
                    new Rect(0.5f, 0f, 0.5f, 1f));
                // Only the left 42% of the supplied transparent canvas contains art.
                float figureWidth = Mathf.Min(w * 0.44f, h * portrait.width / portrait.height * 0.42f);
                float figureHeight = figureWidth * portrait.height / (portrait.width * 0.42f);
                GUI.DrawTextureWithTexCoords(new Rect(w * 0.45f - figureWidth / 2, h - figureHeight,
                    figureWidth, figureHeight), portrait, new Rect(0f, 0f, 0.42f, 1f), true);
            }
        }
    }
}
