using UnityEngine;
using Verse;

namespace CalidoHomecoming
{
    public sealed class Dialog_BorrowedName : Window
    {
        private readonly CalidoStoryState state;
        private readonly bool replayOnly;
        private int page;

        public override Vector2 InitialSize => new Vector2(780f, 560f);

        public Dialog_BorrowedName(CalidoStoryState state, bool replayOnly = false)
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
            Widgets.Label(new Rect(0f, 0f, inRect.width, 38f), "借來的名字");
            Text.Font = GameFont.Small;

            Rect portrait = new Rect(0f, 52f, 150f, 150f);
            CalidoPortraits.Draw(portrait, null, page == 0 ? "渡口少女" : "南渡記錄人", page == 0 ? true : (bool?)null);

            Rect body = new Rect(170f, 52f, inRect.width - 170f, 310f);
            Widgets.DrawMenuSection(body);
            Widgets.Label(body.ContractedBy(18f), DialogueText());

            if (page == 0)
            {
                if (Widgets.ButtonText(new Rect(170f, 390f, 180f, 42f), "先聽她說完"))
                {
                    page = 1;
                }
            }
            else if (replayOnly)
            {
                if (Widgets.ButtonText(new Rect(inRect.width - 180f, inRect.height - 50f, 180f, 42f), "返回留言冊"))
                {
                    Close();
                }
            }
            else
            {
                DrawChoiceButton(inRect, 0, "告訴她真正的名字", CalidoPrologueChoice.GiveName);
                DrawChoiceButton(inRect, 1, "先借用她給的稱呼", CalidoPrologueChoice.BorrowName);
                DrawChoiceButton(inRect, 2, "拒絕留下名字", CalidoPrologueChoice.RefuseName);
            }
        }

        private string DialogueText()
        {
            if (page == 0)
            {
                return "河對岸的人沒有立刻跪下，也沒有拔出武器。那名少女只是抱緊被雨浸濕的冊子，盯著你的羽翼看了很久。\n\n「如果妳是從天上來的，總該有一個能讓我們寫進冊子裡的名字。」";
            }

            return "她把冊子翻到一張被水泡皺的空頁。\n\n「不願意說也沒關係。渡口常替不知道名字的人先借一個。等妳哪天願意了，再把它還回來。」\n\n遠處有人把你稱作神明，也有人只把門閂得更緊。少女等待你的回答。";
        }

        private void DrawChoiceButton(Rect inRect, int index, string label, CalidoPrologueChoice choice)
        {
            float width = (inRect.width - 20f) / 3f;
            Rect rect = new Rect(index * (width + 10f), inRect.height - 62f, width, 52f);
            if (Widgets.ButtonText(rect, label))
            {
                state.CompletePrologue(choice);
                Close();
            }
        }
    }
}
