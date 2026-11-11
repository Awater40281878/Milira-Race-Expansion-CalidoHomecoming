using UnityEngine;
using Verse;

namespace CalidoHomecoming
{
    public sealed class Dialog_CalidoAuthorization : Window
    {
        private readonly CalidoStoryState state;
        private Vector2 scroll;

        public override Vector2 InitialSize => new Vector2(760f, 650f);

        public Dialog_CalidoAuthorization(CalidoStoryState state)
        {
            this.state = state;
            doCloseX = true;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 34f), "卡利多授權終端・地面校準版");
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 38f, inRect.width, 50f), "高科技並不是研究完成就能使用。每一份授權都需要完成對應章節、取得藍圖，並由地面設備完成校準。普通冷兵器與低階熱兵器不受此清單限制。");

            Rect outer = new Rect(0f, 96f, inRect.width, inRect.height - 146f);
            Rect view = new Rect(0f, 0f, outer.width - 18f, 750f);
            Widgets.BeginScrollView(outer, ref scroll, view);
            float y = 0f;
            DrawCard(view.width, ref y, "I・民用授權", state.CivilianBlueprintUnlocked,
                "前置：第五章「民用授權」結算後自動取得民用藍圖。\n解鎖：米莉拉生活、醫療與運輸設備。\n目前狀態：" + StateText(state.CivilianBlueprintUnlocked));
            DrawCard(view.width, ref y, "II・防具授權", state.ArmorBlueprintUnlocked,
                "前置：第五章「防具授權」結算後自動取得防具藍圖。\n解鎖：卡利多個人防具與校準穿戴。\n目前狀態：" + StateText(state.ArmorBlueprintUnlocked));
            DrawCard(view.width, ref y, "III・武器授權", state.WeaponBlueprintUnlocked,
                "前置：第五章「武器授權」結算後自動取得武器藍圖。\n解鎖：高階米莉拉武器；地面米莉拉才能裝備。\n目前狀態：" + StateText(state.WeaponBlueprintUnlocked));
            DrawCard(view.width, ref y, "IV・米莉安授權", state.MilianBlueprintUnlocked,
                "前置：第五章「米莉安授權」完成；前往中繼站，擊敗無訊、斷鏈、靜潮並帶回證物。\n解鎖：有限米莉安指揮與相關設備。\n目前狀態：" + StateText(state.MilianBlueprintUnlocked));
            DrawCard(view.width, ref y, "有限登城權", state.CampaignBeat >= 12,
                "前置：第四章聽證會結束。\n解鎖：南渡與卡利多往返航班。\n目前狀態：" + StateText(state.CampaignBeat >= 12));
            Widgets.EndScrollView();

            string next = state.CalidoTechUnlocked ? "所有地面校準完成；可從留言冊搭乘卡利多航班。" : state.CampaignBeat < 12
                ? "下一步：推進主線至第四章結算，取得有限登城權。"
                : "下一步：完成目前的第五章任務以取得對應藍圖。";
            Widgets.Label(new Rect(0f, inRect.height - 42f, inRect.width - 130f, 38f), next);
            if (Widgets.ButtonText(new Rect(inRect.width - 120f, inRect.height - 44f, 120f, 36f), "關閉")) Close();
        }

        private static void DrawCard(float width, ref float y, string title, bool unlocked, string text)
        {
            float height = Text.CalcHeight(text, width - 40f) + 58f;
            Rect rect = new Rect(0f, y, width, height);
            Widgets.DrawMenuSection(rect);
            GUI.color = unlocked ? new Color(0.45f, 1f, 0.65f) : new Color(1f, 0.72f, 0.35f);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(16f, y + 10f, width - 32f, 28f), title + (unlocked ? "・已授權" : "・鎖定"));
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(16f, y + 40f, width - 32f, height - 46f), text);
            y += height + 12f;
        }

        private static string StateText(bool unlocked) => unlocked ? "已授權，可校準使用。" : "尚未授權。";
    }
}
