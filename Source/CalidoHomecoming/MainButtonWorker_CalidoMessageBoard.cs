using RimWorld;
using Verse;

namespace CalidoHomecoming
{
    public sealed class MainButtonWorker_CalidoMessageBoard : MainButtonWorker_ToggleTab
    {
        public override bool Visible
        {
            get
            {
                CalidoStoryState state = Current.Game?.GetComponent<CalidoStoryState>();
                return state?.ScenarioEnabled == true && base.Visible;
            }
        }
    }
}
