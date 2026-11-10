using UnityEngine;
using Verse;

namespace CalidoHomecoming
{
    public sealed class Dialog_FeatherAtWell : Window
    {
        private readonly CalidoStoryState state;
        private readonly bool replayOnly;

        public override Vector2 InitialSize => new Vector2(820f, 570f);

        public Dialog_FeatherAtWell(CalidoStoryState state, bool replayOnly = false)
        {
            this.state = state;
            this.replayOnly = replayOnly;
            doCloseX = replayOnly;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 38f), "井邊的羽毛");
            Text.Font = GameFont.Small;

            Rect portrait = new Rect(0f, 50f, 190f, 340f);
            CalidoPortraits.Draw(portrait, null, "井邊孩子", null);
            Rect body = new Rect(205f, 50f, inRect.width - 205f, 340f);
            Widgets.DrawMenuSection(body);
            string text = "一名孩子在井邊撿到從你翅上脫落的白羽。母親要他立刻歸還，祭司說應該供在施濟院，市場商人則當場開價。\n\n孩子把羽毛藏在背後。\n\n「這是妳掉的。可是他們說，神明掉下來的東西不再屬於神明。」\n\n你怎麼回答？";
            if (replayOnly && state.FeatherEventComplete)
            {
                text += "\n\n結果：" + CalidoStoryState.FeatherOutcomeText(state.FeatherChoice);
            }
            Widgets.Label(body.ContractedBy(18f), text);

            if (replayOnly)
            {
                if (Widgets.ButtonText(new Rect(inRect.width - 180f, inRect.height - 50f, 180f, 42f), "返回留言冊"))
                {
                    Close();
                }
                return;
            }

            DrawChoice(inRect, 0, "請他歸還羽毛", "這是身體的一部分，不是無主的聖物。", CalidoFeatherChoice.ReturnIt);
            DrawChoice(inRect, 1, "讓他留下作紀念", "可以記住相遇，但不能拿它治病或換錢。", CalidoFeatherChoice.KeepAsCommonMemory);
            DrawChoice(inRect, 2, "禁止供奉與販售", "公開否定神蹟，承擔祭司與商人的反應。", CalidoFeatherChoice.ForbidWorship);
        }

        private void DrawChoice(Rect inRect, int index, string label, string tooltip, CalidoFeatherChoice choice)
        {
            float width = (inRect.width - 20f) / 3f;
            Rect rect = new Rect(index * (width + 10f), inRect.height - 88f, width, 72f);
            if (Widgets.ButtonText(rect, label))
            {
                state.CompleteFeatherEvent(choice);
                Close();
            }
            TooltipHandler.TipRegion(rect, tooltip);
        }
    }
}
