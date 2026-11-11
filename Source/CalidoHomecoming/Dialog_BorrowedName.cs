using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;

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
            Pawn protagonist = Find.Maps.SelectMany(m => m.mapPawns.FreeColonistsSpawned)
                .FirstOrDefault(state.IsProtagonist);
            if (page <= 1 && protagonist != null)
                GUI.DrawTexture(portrait, PortraitsCache.Get(protagonist, new Vector2(150f, 150f), Rot4.South), ScaleMode.ScaleToFit);
            else
                CalidoPortraits.Draw(portrait, null, "渡口少女", true);

            Rect body = new Rect(170f, 52f, inRect.width - 170f, 310f);
            Widgets.DrawMenuSection(body);
            Widgets.Label(body.ContractedBy(18f), DialogueText());

            if (page < 4)
            {
                if (Widgets.ButtonText(new Rect(170f, 390f, 180f, 42f), "繼續"))
                {
                    page++;
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
                DrawChoiceButton(inRect, 0, "說出隱約想起的名字", CalidoPrologueChoice.GiveName);
                DrawChoiceButton(inRect, 1, "請她暫時借我名字", CalidoPrologueChoice.BorrowName);
                DrawChoiceButton(inRect, 2, "暫時不留名字", CalidoPrologueChoice.RefuseName);
            }
        }

        private string DialogueText()
        {
            if (page == 0)
            {
                return "「……頭好痛。」\n\n冰冷的泥土貼著臉頰，耳邊是水流和風吹過草叢的聲音。她睜開眼，陌生的天空在視線裡搖晃。\n\n「這裡……是哪裡？」";
            }

            if (page == 1)
            {
                return "她試著撐起身，羽翼不安地收攏。一把長槍落在觸手可及的地方，握住它時，身體竟比思緒更早記得該怎麼用。\n\n天空、一道刺眼的光，還有某個等著回覆的聲音……記憶在她想抓住的瞬間散開。\n\n「我為什麼會在這裡……？我又是誰？」";
            }
            if (page == 2)
                return "草叢另一頭傳來腳步聲。一名抱著冊子的少女停在幾步之外，目光從長槍移到白色羽翼，又小心地看向她的臉。\n\n「妳受傷了嗎？我看到那邊有東西從天上落下來……」\n\n米莉拉握緊槍柄，遲疑了一會兒，才輕聲回答：「我不知道。我什麼都想不起來。」";
            if (page == 3)
                return "少女沒有再靠近，只把水壺放在地上，向後退了一步。\n\n「這裡是南渡。妳可以先喝點水；天黑以後，河邊會很冷。」\n\n遠處傳來人聲。有人低聲叫她神明，也有人急忙關上門。米莉拉抬頭望向雲層，心底卻只有茫然。\n\n「我不知道自己是什麼……但我需要一個能遮雨的地方。」";
            return "少女翻開冊子，停在一張空白頁上。\n\n「我替妳留個位置吧。妳記得自己的名字嗎？」\n\n米莉拉努力回想。意識深處似乎有個模糊的稱呼，卻抓不住全貌。\n\n「想不起來也沒關係，」少女說，「渡口常替旅人先借一個稱呼。等妳想起來，再把它還回來。」";
        }

        private void DrawChoiceButton(Rect inRect, int index, string label, CalidoPrologueChoice choice)
        {
            float width = (inRect.width - 20f) / 3f;
            Rect rect = new Rect(index * (width + 10f), inRect.height - 62f, width, 52f);
            if (Widgets.ButtonText(rect, label))
            {
                Pawn protagonist = state.Protagonist;
                if (protagonist == null)
                {
                    Messages.Message("找不到劇本主角，請確認她仍在地圖上。", MessageTypeDefOf.RejectInput, false);
                    return;
                }
                Close();
                Find.WindowStack.Add(new Dialog_ProtagonistName(protagonist, state, choice));
            }
        }
    }
}
