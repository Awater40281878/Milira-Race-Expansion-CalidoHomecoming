using HarmonyLib;
using RimWorld;

namespace CalidoHomecoming
{
    // A pawn kind can request a title from a faction excluded by the story roster.
    // There is no issuer for that title; continue generating the pawn without it.
    [HarmonyPatch(typeof(Pawn_RoyaltyTracker), nameof(Pawn_RoyaltyTracker.SetTitle))]
    internal static class StoryRoyalTitlePatch
    {
        private static bool Prefix(Faction faction)
        {
            return faction != null || !ScenPart_CalidoStory.StorySelected;
        }
    }
}
