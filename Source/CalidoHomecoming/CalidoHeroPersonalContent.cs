using System.Collections.Generic;

namespace CalidoHomecoming
{
    public sealed class HeroPersonalLine
    {
        public readonly string Name;
        public readonly string Title;
        public readonly string Request;
        public readonly string AllyFaction;

        public readonly string ActionLabel, Supply;
        public readonly int Amount;

        public HeroPersonalLine(string action, string supply, int amount, string name, string title, string request, string allyFaction)
        {
            ActionLabel = action; Supply = supply; Amount = amount;
            Name = name; Title = title; Request = request; AllyFaction = allyFaction;
        }
    }

    public static class CalidoHeroPersonalContent
    {
        private static readonly Dictionary<string, HeroPersonalLine> Lines = new Dictionary<string, HeroPersonalLine>
        {
            ["伊蕾希雅"] = new HeroPersonalLine("補登失蹤者姓名", "WoodLog", 10, "伊蕾希雅", "渡口守望者", "她想替第一批被救下的人補齊名字，而不是只寫下人數。", "Calido_AdventurersGuild"),
            ["卡西婭"] = new HeroPersonalLine("撥藥救治腿傷", "MedicineIndustrial", 3, "卡西婭", "醫師", "她請求把藥留給腿傷最重的獲救者，即使這會減少南渡庫存。", "Calido_AxolotlRiverfolk"),
            ["賽奧法諾"] = new HeroPersonalLine("支付證人護送費", "Silver", 60, "賽奧法諾", "談判官", "她找到一份能指向買家的帳冊，但公會要求先保護證人。", "Calido_AdventurersGuild"),
            ["尼基塔絲"] = new HeroPersonalLine("出資保護指認者", "Silver", 40, "尼基塔絲", "獵手", "她要求留下一名活口，讓被獵者能在公開場合指認押運官。", "Calido_KiiroKingdom"),
            ["歐多西婭"] = new HeroPersonalLine("搭建避難棚", "Steel", 50, "歐多西婭", "建築師", "她想把拆下的鐵籠改成避難棚，而不是拿去換取鋼材。", "Calido_MihoKingdom"),
            ["普羅科匹婭"] = new HeroPersonalLine("派人核對求援訊號", "MealSurvivalPack", 2, "普羅科匹婭", "近衛", "她收到舊同袍的求援訊號，卻懷疑那是巡察使留下的試探。", "Calido_MiliraChurch"),
            ["伊琳娜"] = new HeroPersonalLine("複製航標紀錄", "ComponentIndustrial", 2, "伊琳娜", "研究員", "她希望公開航標的低頻紀錄，讓每個地面居民都能核對天空的說法。", "Calido_AdventurersGuild"),
            ["安娜斯塔西婭"] = new HeroPersonalLine("比對刀痕樣本", "Steel", 12, "安娜斯塔西婭", "劍士", "她要追查假執行官留下的劍痕，但不想再把無辜的白翼者當嫌犯。", "Calido_MiliraChurch"),
            ["佐伊"] = new HeroPersonalLine("刊印完整供詞", "WoodLog", 15, "佐伊", "記錄官", "她請主角決定：是否把一名背叛者的供詞原樣釘上留言柱。", "Calido_AdventurersGuild"),
            ["狄奧多拉"] = new HeroPersonalLine("撥米給過冬難民", "RawRice", 40, "狄奧多拉", "農藝師", "她想把收成留給過冬的難民，代價是殖民地短期少一批商品作物。", "Calido_AxolotlRiverfolk"),
            ["瑪麗娜"] = new HeroPersonalLine("送還修好的羽環", "Silver", 20, "瑪麗娜", "工匠", "她修復了一枚被供奉的羽環，卻想把它交還原主，而非做成聖物。", "Calido_MiliraChurch"),
            ["尤多基婭"] = new HeroPersonalLine("印製聽證名單", "WoodLog", 25, "尤多基婭", "審計官", "她掌握一筆巡察使的虛報名單，需要你決定是否在聽證會上公開。", "Calido_AdventurersGuild"),
            ["阿加莎"] = new HeroPersonalLine("追查傳令兵身分", "MealSurvivalPack", 3, "阿加莎", "狙擊手", "她拒絕在屋頂射殺一名逃跑的傳令兵，除非你確認其攜帶真正的命令。", "Calido_WolfeinKingdom"),
            ["克莉絲蒂娜"] = new HeroPersonalLine("開設共食長桌", "MealSimple", 5, "克莉絲蒂娜", "廚師", "她用母城配方煮出第一鍋熱湯，想邀請不同種族共桌，而非只獎勵有功者。", "Calido_MihoKingdom"),
            ["芭芭拉"] = new HeroPersonalLine("修復避難入口", "Steel", 35, "芭芭拉", "盾衛", "她想把一面破盾掛在入口，提醒守衛先保護撤離者。", "Calido_WolfeinKingdom"),
            ["卡琳妮"] = new HeroPersonalLine("設立民間射手訓練班", "Steel", 30, "卡琳妮", "射手", "她收到武器授權的空白表格，要求主角決定誰有資格先學會使用它。", "Calido_KiiroKingdom"),
            ["佩拉吉婭"] = new HeroPersonalLine("修復求救記憶迴路", "ComponentIndustrial", 4, "佩拉吉婭", "機械師", "她從米莉安殘骸裡聽見求救迴路，想先修復記憶，而不是拆取零件。", "Calido_OrbitalMilira"),
            ["克謝妮婭"] = new HeroPersonalLine("護送名簿上的家屬", "Silver", 70, "克謝妮婭", "潛行者", "她帶回一本抄經所名簿，裡面有被迫抄寫者的住址。", "Calido_FallenCult"),
            ["塔季揚娜"] = new HeroPersonalLine("發行匿名原稿對照本", "WoodLog", 18, "塔季揚娜", "學者", "她找出兩份相互矛盾的神諭原稿，請主角決定公開哪一份、如何保護抄寫者。", "Calido_FallenCult"),
            ["葉夫普拉克西婭"] = new HeroPersonalLine("封存墮天使契約", "Steel", 25, "葉夫普拉克西婭", "守望者", "她要把墮天使契約封存，避免下一個絕望的人把它當成捷徑。", "Calido_FallenCult")
        };

        public static HeroPersonalLine Get(string name)
        {
            return name != null && Lines.TryGetValue(name, out HeroPersonalLine line) ? line : null;
        }
    }
}
