using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CalidoHomecoming
{
    public sealed class CalidoHeroStories : GameComponent
    {
        private List<int> chapters = new List<int>();
        private List<int> trust = new List<int>();
        private List<int> nextTicks = new List<int>();
        private int openHero = -1;

        public CalidoHeroStories(Game game) { }

        private void EnsureSlots()
        {
            chapters ??= new List<int>();
            trust ??= new List<int>();
            nextTicks ??= new List<int>();
            while (chapters.Count < 20) chapters.Add(0);
            while (trust.Count < 20) trust.Add(0);
            while (nextTicks.Count < 20) nextTicks.Add(-1);
        }

        public void Begin(string name, bool supported)
        {
            EnsureSlots();
            int index = Current.Game.GetComponent<CalidoHeroRegistry>()?.IndexOf(name) ?? -1;
            if (index < 0 || chapters[index] != 0) return;
            chapters[index] = 1;
            trust[index] = supported ? 1 : -1;
            nextTicks[index] = Find.TickManager.TicksGame + 180000;
        }

        public override void GameComponentTick()
        {
            if (Find.TickManager == null || Find.TickManager.TicksGame % 2500 != 0 || openHero >= 0) return;
            if (Current.Game.GetComponent<CalidoStoryState>()?.ScenarioEnabled != true) return;
            EnsureSlots();
            CalidoHeroRegistry registry = Current.Game.GetComponent<CalidoHeroRegistry>();
            for (int i = 0; i < 20; i++)
            {
                if (chapters[i] < 1 || chapters[i] > 2 || nextTicks[i] > Find.TickManager.TicksGame) continue;
                Pawn hero = registry?.FindHero(i);
                if (hero == null || hero.Dead || hero.Faction != Faction.OfPlayer) { nextTicks[i] = -1; chapters[i] = 4; continue; }
                if (!hero.Spawned || hero.Map?.IsPlayerHome != true || hero.Downed) continue;
                Open(i, hero);
                return;
            }
        }

        private void Open(int index, Pawn hero)
        {
            openHero = index;
            HeroPersonalLine line = CalidoHeroPersonalContent.Get(CalidoHeroRegistry.HeroName(index));
            string body = chapters[index] == 1 ? Middle[index] : Ending[index];
            string history = trust[index] > 0
                ? "「我記得上一次，你願意聽我把話說完。」"
                : "「上一次我們沒有達成共識，但我想再試著說一次。」";
            ThingDef supplies = DefDatabase<ThingDef>.GetNamed(line.Supply);
            int needed = System.Math.Max(1, line.Amount / 2);
            Find.WindowStack.Add(new Dialog_ChapterOneEncounter(line.Name + "・個人記錄", line.Name,
                body + "\n\n" + history,
                new Dialog_ChapterOneEncounter.Choice("提供協助", "為下一步準備物資：" + needed + " × " + supplies.LabelCap, () =>
                {
                    if (SpendSupplies(supplies, needed)) Resolve(index, hero, true);
                }, () => CountSupplies(supplies) >= needed),
                new Dialog_ChapterOneEncounter.Choice("坦白說出疑慮", "誠實表達分歧，保持彼此的獨立判斷。", () => Resolve(index, hero, false)),
                new Dialog_ChapterOneEncounter.Choice("改天再談", "三天後再繼續。", () => { openHero = -1; nextTicks[index] = Find.TickManager.TicksGame + 180000; })));
        }

        private static IEnumerable<Thing> EligibleSupplies(ThingDef def)
        {
            foreach (Map map in Find.Maps)
                if (map.IsPlayerHome)
                    foreach (Thing thing in map.listerThings.ThingsOfDef(def))
                        if (thing.Faction == Faction.OfPlayer || thing.IsInAnyStorage())
                            yield return thing;
        }

        private static int CountSupplies(ThingDef def)
        {
            int count = 0;
            foreach (Thing thing in EligibleSupplies(def)) count += thing.stackCount;
            return count;
        }

        private static bool SpendSupplies(ThingDef def, int count)
        {
            if (CountSupplies(def) < count) return false;
            var things = new List<Thing>(EligibleSupplies(def));
            foreach (Thing thing in things)
            {
                int take = System.Math.Min(count, thing.stackCount);
                thing.SplitOff(take).Destroy();
                count -= take;
                if (count == 0) break;
            }
            return true;
        }

        private void Resolve(int index, Pawn hero, bool supported)
        {
            trust[index] += supported ? 1 : -1;
            ThoughtDef memory = DefDatabase<ThoughtDef>.GetNamedSilentFail(supported ? "Calido_HeroSupported" : "Calido_HeroDisappointed");
            if (memory != null) hero.needs?.mood?.thoughts?.memories?.TryGainMemory(memory);
            chapters[index]++;
            nextTicks[index] = chapters[index] < 3 ? Find.TickManager.TicksGame + 240000 : -1;
            openHero = -1;
            if (chapters[index] == 3)
                Find.LetterStack.ReceiveLetter("一段關係的記錄", hero.LabelShortCap +
                    (trust[index] > 0 ? "願意把更多心事交給南渡，也會記得一路得到的幫助。"
                        : "珍惜彼此仍能坦誠討論，雖然有些分歧仍留待時間解答。"), LetterDefOf.PositiveEvent, hero);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref chapters, "heroStoryChapters", LookMode.Value);
            Scribe_Collections.Look(ref trust, "heroStoryTrust", LookMode.Value);
            Scribe_Collections.Look(ref nextTicks, "heroStoryNextTicks", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit) { openHero = -1; EnsureSlots(); }
        }

        private static readonly string[] Middle =
        {
            "伊蕾希雅把地圖攤開，指著一條被她反覆擦去的路。那條路上有她沒能送到的貨，也有她一直不敢寫下的名字。",
            "卡西婭收到一張沾著藥粉的回信。傷者已能行走，但她發現當年的診療紀錄少了一頁。",
            "賽奧法諾帶回一封未拆的證人信。信封上沒有名字，只有一道她熟悉的封蠟。",
            "尼基塔絲發現指認者留下的路標被人調換。她想回去確認，卻不願讓另一個人替自己冒險。",
            "歐多西婭畫了一張避難棚的草圖，又在角落寫下拆籠者的名字。她不確定倖存者是否願意看見那些材料。",
            "普羅科匹婭終於收到舊同袍回覆，訊號卻來自一個早已停用的呼號。",
            "伊琳娜把兩份航標紀錄疊在一起。其中一段靜音並非損壞，而是被人刻意留白。",
            "安娜斯塔西婭從樣本中認出一位舊識的習慣。她需要決定如何向可能受牽連的人說明。",
            "佐伊收到被記錄者的來信。對方承認自己說過那些話，卻懇求不要公開家人的名字。",
            "狄奧多拉拿出剩下的種子，發現有一包並不屬於她種過的品種。包裝內夾著一封簡短的謝函。",
            "瑪麗娜的羽環被送回來了。原主說，她希望有人先替自己保管到能重新踏上旅途的那一天。",
            "尤多基婭核對完名單後，發現一筆數字被兩個不同的人改過。她想在公開以前找到被刪去的證人。",
            "阿加莎收到那位傳令兵的口信。對方沒有要求寬恕，只問她是否願意聽完命令的來歷。",
            "克莉絲蒂娜正在調整湯的味道。她說母城的配方少了一樣東西，卻直到在南渡共桌才想起來。",
            "芭芭拉找到破盾的另一半，背面刻著一名撤離者的留言。她猶豫是否該把它掛在所有人都看得見的地方。",
            "卡琳妮的學員在靶紙上畫了自己的家。她意識到這場訓練對他們而言不只是學會射擊。",
            "佩拉吉婭修好了殘破的記憶迴路。裡面沒有戰鬥命令，只有一段反覆播放的求救座標。",
            "克謝妮婭收到家屬傳回的空白紙。紙張看似沒有訊息，折痕卻與抄經所的傳遞方式一模一樣。",
            "塔季揚娜發現兩份原稿共有的錯字。它讓她想到，最初的抄寫者也許故意留下了線索。",
            "葉夫普拉克西婭檢查封存的契約時，聽見有人在門外停下腳步。那人留下了一個沒有署名的問題。"
        };

        private static readonly string[] Ending =
        {
            "伊蕾希雅將最後一個名字寫進道路紀錄，把地圖推到桌子中央。「我想把這條路留給願意回家的人。」",
            "卡西婭補齊了遺失的診療頁，卻把病人的名字留給對方親自決定是否公開。",
            "賽奧法諾終於拆開信。她沒有立刻念出內容，只請南渡替寫信的人保留一個安全的位置。",
            "尼基塔絲帶回了真正的路標。她將它立在門口，約定往後先確認歸途，再追逐目標。",
            "歐多西婭完成了避難棚的設計。她留下了一扇朝向道路的門，說那是為還沒抵達的人準備的。",
            "普羅科匹婭回覆了舊呼號。她無法確定對方是否聽見，但終於不必再假裝自己沒有等待。",
            "伊琳娜在航標旁放了一本公開紀錄簿，讓任何人都能核對下一次收到的訊號。",
            "安娜斯塔西婭將樣本交給證人自行保管。她說，一把武器留下的痕跡不應替人決定整個故事。",
            "佐伊完成兩份記錄：一份供眾人查閱，一份留給當事人。她請你一起確認空白並非遺漏。",
            "狄奧多拉把陌生種子分成兩份，一份播下，一份留待來年。她邀請大家在旁邊寫上自己的名字。",
            "瑪麗娜為羽環準備了一個能上鎖的小盒。鑰匙留在原主手裡，南渡只負責保管。",
            "尤多基婭在名單末尾補上一欄：誰有權修改記錄。這一次，旁邊有不只一個人的簽名。",
            "阿加莎收起槍，將一封未封口的回信交給你。她說，有些目標需要先用名字稱呼。",
            "克莉絲蒂娜把新配方交給所有願意學的人。餐桌上沒有主賓位，每個位置都一樣靠近爐火。",
            "芭芭拉把盾掛在出口旁。「它擋住過危險，也提醒我們記得替別人留一條路。」",
            "卡琳妮讓學員收起武器，先在地圖上標記避難方向。她說，知道何時不射擊也是訓練。",
            "佩拉吉婭將求救訊號存入獨立的記憶匣。她答應有一天會帶著它，找到能回應的人。",
            "克謝妮婭把空白紙送還給家屬。紙上沒有寫出住址，但已經多了一個可以信任的聯絡方式。",
            "塔季揚娜保留了那個共同的錯字，並在旁邊註明它如何被發現。讀者可以自己追索原稿。",
            "葉夫普拉克西婭沒有打開契約，只在封存箱旁留下一張紙，告訴來訪者有其他求助的地方。"
        };
    }
}
