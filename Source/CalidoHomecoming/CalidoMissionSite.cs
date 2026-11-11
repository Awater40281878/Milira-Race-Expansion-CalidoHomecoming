using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CalidoHomecoming
{
    public sealed class CalidoMissionSite : Settlement
    {
        public int campaignBeat = -1;
        public override MapGeneratorDef MapGeneratorDef => campaignBeat >= 15
            ? DefDatabase<MapGeneratorDef>.GetNamed("Calido_ScriptoriumGround") : base.MapGeneratorDef;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref campaignBeat, "campaignBeat", -1);
        }

        public override void PostMapGenerate()
        {
            base.PostMapGenerate();
            Current.Game?.GetComponent<CalidoStoryState>()?.PopulateMissionSite(this, Map);
        }
    }
}
