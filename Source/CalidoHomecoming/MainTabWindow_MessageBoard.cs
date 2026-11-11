using UnityEngine;
using RimWorld;
using Verse;

namespace CalidoHomecoming
{
    public sealed class MainTabWindow_MessageBoard : MainTabWindow
    {
        private Vector2 scrollPosition;

        public override Vector2 RequestedTabSize => new Vector2(900f, 700f);

        public override void DoWindowContents(Rect inRect)
        {
            CalidoStoryState state = Current.Game?.GetComponent<CalidoStoryState>();

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 36f), "南渡留言冊");
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 40f, inRect.width, 28f), "地面上的消息不會憑空傳遍世界；只有親歷者、記錄人與抵達南渡的旅客會更新這本冊子。");

            Rect outer = new Rect(0f, 78f, inRect.width, inRect.height - 130f);
            // Long-form campaign, hero and cross-race entries can exceed the
            // original fixed canvas. A generous canvas prevents late chapters
            // from being clipped while retaining RimWorld's native scroll view.
            Rect view = new Rect(0f, 0f, outer.width - 18f, 5200f);
            Widgets.BeginScrollView(outer, ref scrollPosition, view);

            float y = 0f;
            if (state?.PrologueComplete == true)
            {
                Map home = Find.AnyPlayerHomeMap;
                int done = state.StartingGuideCompletedMask;
                string shelter = (done & 1) != 0 ? "完成" : "待完成";
                string storage = (done & 2) != 0 ? "完成" : "待完成";
                string kitchen = (done & 4) != 0 ? "完成" : "待完成";
                string wealth = (done & 8) != 0 ? "完成" : "待完成";
                float currentWealth = home?.wealthWatcher?.WealthBuildings ?? 0f;
                DrawEntry(view.width, ref y, "初期指引：在南渡落腳",
                    $"[{shelter}] 休息處：建造有門牆及完整屋頂的房間，放入一張床。\n" +
                    $"[{storage}] 倉庫：設置儲存區，至少一格位於完整屋頂的室內。可和其他房間共用。\n" +
                    $"[{kitchen}] 備餐：建造營火、燃料爐或電爐。\n" +
                    $"[{wealth}] 基礎設施：建築財富達到 {CalidoStartingGuide.BuildingWealthTarget:0} 銀，目前 {currentWealth:0} 銀。\n" +
                    "木牆、門、床與基本設施都能逐步累積；各項可按任何順序完成。完成狀態約每遊戲 20 秒更新。");
            }
            DrawEntry(view.width, ref y, "序幕：借來的名字", state?.PrologueComplete == true
                ? "已完成。渡口替這名陌生的米莉拉留下了第一頁記錄。"
                : "尚未完成。建立殖民地後，序幕會自動開始。");

            if (state?.PrologueComplete == true)
            {
                string choice = ChoiceText(state.PrologueChoice);
                DrawEntry(view.width, ref y, "第一次留言", choice);
                if (Widgets.ButtonText(new Rect(18f, y, 180f, 34f), "重讀序幕對話"))
                {
                    state.ReopenPrologue();
                }
                y += 48f;
            }

            if (state?.FeatherEventComplete == true)
            {
                DrawEntry(view.width, ref y, "井邊的羽毛", CalidoStoryState.FeatherOutcomeText(state.FeatherChoice));
                if (Widgets.ButtonText(new Rect(18f, y, 180f, 34f), "重讀事件對話"))
                {
                    state.ReopenFeatherEvent();
                }
                y += 48f;
            }
            else
            {
                DrawEntry(view.width, ref y, "第一個地方傳聞", state?.PrologueComplete == true
                    ? "有人在井邊撿到一根不屬於祭壇的白羽。事件會在短時間後主動找上門。"
                    : "完成序幕後才會出現。");
            }

            if (state?.MessagePostQuestComplete == true)
            {
                DrawEntry(view.width, ref y, "建設完成：南渡留言柱", "留言柱已完工。居民現在能用普通紙張和口述記錄留下委託，這也是之後地面任務的實體入口。");
            }
            else if (state?.MessagePostQuestAnnounced == true)
            {
                DrawEntry(view.width, ref y, "進行中：為南渡立一座留言柱", "從建築規劃的「雜項」分類建造南渡留言柱。需要 40 單位木材，完成後會自動更新任務。");
            }
            else
            {
                DrawEntry(view.width, ref y, "尚未釘上的委託", "第一個建設任務正在等待居民確認你如何看待『神明留下的東西』。這一列由存檔狀態驅動，不會因關閉視窗消失。");
            }

            if (state?.MessagePostQuestComplete == true)
            {
                string[] chapterSteps =
                {
                    "公會委託：失蹤的記錄員",
                    "第一位英雄：伊蕾希雅",
                    "強盜交涉：赤索路障",
                    "奴隸商路：查驗契約",
                    "卡利多線索：沉默不是神諭"
                };
                for (int i = 0; i < chapterSteps.Length; i++)
                {
                    string status = state.ChapterOneStep > i ? "已完成" : state.ChapterOneStep == i && !state.ChapterOneComplete ? "下一項／進行中" : "尚未開始";
                    DrawEntry(view.width, ref y, chapterSteps[i], status);
                }

                DrawEntry(view.width, ref y, "第一章結算", state.ChapterOneComplete
                    ? "已完成。第二章《斷翼商路》已解鎖。"
                    : "完成五項南渡事件後結算。談判選項會影響過程，但不會讓主線永久中斷。");
            }

            if (state?.ChapterOneComplete == true)
            {
                DrawEntry(view.width, ref y, "長篇主線進度", state.CampaignBeat >= CalidoCampaignContent.Beats.Length
                    ? "全六章完成。墮天使烙印已取得；歸燈由白羽聖堂的守燈儀式授予。"
                    : $"目前完成 {state.CampaignBeat}/{CalidoCampaignContent.Beats.Length} 項後續任務。下一項：{CalidoCampaignContent.Beats[state.CampaignBeat].Chapter}・{CalidoCampaignContent.Beats[state.CampaignBeat].Title}" +
                      (state.ActiveCampaignBeat >= 0 ? (state.ActiveCampaignCombat ? "\n目前狀態：行動中，必須清除仍有戰鬥能力的敵人。" : "\n目前狀態：調查／建設程序進行中。") : ""));
                for (int i = 0; i < CalidoCampaignContent.Beats.Length; i++)
                {
                    CampaignBeat beat = CalidoCampaignContent.Beats[i];
                    DrawEntry(view.width, ref y, beat.Chapter + "／" + beat.Title,
                        i < state.CampaignBeat ? "已完成，" + beat.HeroName + "（" + beat.HeroTitle + "）已加入。" : i == state.CampaignBeat ? "進行中或等待觸發。" : "尚未解鎖。");
                }
                DrawEntry(view.width, ref y, "地面科技封鎖", state.CalidoTechUnlocked
                    ? "四類科技授權已完成，高階米莉拉裝備現可校準使用。"
                    : $"仍在封鎖中。普通服飾、非高階戰鬥防具、普通冷兵器與低階熱兵器不受穿戴限制；指定的米莉拉高階戰甲與高科技武器需要授權。\n民用：{Mark(state.CivilianBlueprintUnlocked)}　防具：{Mark(state.ArmorBlueprintUnlocked)}　武器：{Mark(state.WeaponBlueprintUnlocked)}　米莉安：{Mark(state.MilianBlueprintUnlocked)}");
                DrawEntry(view.width, ref y, "英雄個人記錄", $"已完成 {state.HeroPersonalLinesCompleted}/20 條。每位英雄加入後會在南渡提出一項個人委託；選擇會改變相關陣營好感與主線的救援／強硬傾向。");
            }

            if (state?.MessagePostQuestComplete == true)
            {
                DrawEntry(view.width, ref y, "四族隨機事件", $"已記錄 {state.CrossRaceEventsSeen}/160 條。美狐、奇羅、沃芬與萌螈各有40條；全部出現後才會重置抽取池。");
                DrawEntry(view.width, ref y, "鎖鏈商路救援記錄", $"已解救 {state.RescuedCaptives} 人。連續救援會提高鎖鏈商路的報復值（目前 {state.SlaveCaravanNotoriety}/3）；資助返程者會提高其原屬種族陣營好感。世界地圖旅行時仍可能再次發現奴隸商隊。");
            }
            Widgets.EndScrollView();

            if (state?.ChurchVigilAvailable == true && Widgets.ButtonText(new Rect(0f, inRect.height - 42f, 140f, 38f), "白羽聖堂・守燈"))
                state.OpenChurchVigil();
            if (state?.CampaignBeat >= 12 && Widgets.ButtonText(new Rect(146f, inRect.height - 42f, 150f, 38f), "卡利多航班"))
                CalidoFlight.Open();
            if (Widgets.ButtonText(new Rect(inRect.width - 360f, inRect.height - 42f, 180f, 38f), "卡利多授權終端"))
            {
                Find.WindowStack.Add(new Dialog_CalidoAuthorization(state));
            }
            if (Widgets.ButtonText(new Rect(inRect.width - 170f, inRect.height - 42f, 170f, 38f), "關閉"))
            {
                Close();
            }
        }

        private static string Mark(bool unlocked) => unlocked ? "已授權" : "未授權";

        private static void DrawEntry(float width, ref float y, string title, string body)
        {
            float height = Text.CalcHeight(body, width - 36f) + 58f;
            Rect rect = new Rect(0f, y, width, height);
            Widgets.DrawMenuSection(rect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(18f, y + 10f, width - 36f, 28f), title);
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(18f, y + 40f, width - 36f, height - 46f), body);
            y += height + 12f;
        }

        private static string ChoiceText(CalidoPrologueChoice choice)
        {
            switch (choice)
            {
                case CalidoPrologueChoice.GiveName:
                    return "她說出記憶中隱約浮現的名字，並不確定那是否完整。記錄人把它寫在空白頁上。";
                case CalidoPrologueChoice.BorrowName:
                    return "她暫時借用了渡口給的稱呼。那不是假名，而是一筆尚待本人補完的記錄。";
                case CalidoPrologueChoice.RefuseName:
                    return "她拒絕留下名字。記錄人保留空頁，並沒有因此把她寫成敵人。";
                default:
                    return "這一頁仍等待回答。";
            }
        }
    }
}
