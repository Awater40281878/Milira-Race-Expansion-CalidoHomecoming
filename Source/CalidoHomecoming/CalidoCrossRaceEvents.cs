using System.Collections.Generic;

namespace CalidoHomecoming
{
    public sealed class CrossRaceEventEntry
    {
        public int Id;
        public string Race;
        public string Title;
        public string Speaker;
        public string Body;
        public string PortraitSheet;
    }

    public static class CalidoCrossRaceEvents
    {
        public static readonly List<CrossRaceEventEntry> All = Build();

        private static List<CrossRaceEventEntry> Build()
        {
            var result = new List<CrossRaceEventEntry>(160);
            AddRace(result, "美狐", "UI/Portraits/MihoFemaleGrid4x4",
                new[] { "月讀", "霜尾", "青燈", "紫苑", "夜見", "鈴音", "白露", "霞月", "墨羽", "星羅", "冬華", "千代", "玄葉", "玉藻", "夕凪", "月白" },
                "月紋商路", "月祭與祖靈", "靈狐藥草");
            AddRace(result, "奇羅", "UI/Portraits/KiiroFemaleGrid4x4",
                new[] { "米菈", "露可", "芙恩", "妮亞", "柯蕾", "莎芙", "緹妮", "艾露", "帕米", "蘿安", "希芙", "梅爾", "塔露", "菲婭", "諾拉", "伊芙" },
                "兜帽旅隊", "森林餐桌禮", "貓薄荷與野莓");
            AddRace(result, "沃芬", "UI/Portraits/WolfeinFemaleGrid4x4",
                new[] { "艾妲", "赫爾嘉", "賽琳", "諾克絲", "維拉", "芙蕾", "格蕾塔", "蕾娜", "奧黛", "卡雅", "潔西卡", "伊爾瑪", "泰莎", "瑪格", "希爾德", "羅莎" },
                "黑紅物流線", "公司契約禮", "軍規醫療包");
            AddRace(result, "萌螈", "UI/Portraits/AxolotlFemaleGrid4x4",
                new[] { "緋蓮", "白鰓", "桃汐", "蘭珠", "赤羽", "清漣", "紫蘇", "花鱗", "珊瑚", "雨棠", "紅璃", "澄心", "朱砂", "月汐", "玉漣", "白蘭" },
                "河灣船隊", "水燈與換鰓禮", "河藻止血膏");
            return result;
        }

        private static void AddRace(List<CrossRaceEventEntry> list, string race, string sheet, string[] names, string caravan, string custom, string medicine)
        {
            string[] titles =
            {
                "失聯的旅隊", "急需藥材", "禮俗誤會", "逃亡者的證詞", "臨時市集",
                "交換工藝", "失蹤的信使", "共同狩獵", "邊境使節", "被封住的遺跡"
            };
            string[] cores =
            {
                $"一支{caravan}在南渡附近失去消息，最後留下的路標卻被人改成米莉拉神諭。",
                $"一名傷者需要{medicine}以外的地面藥物。來訪者願意用本族情報交換。",
                $"居民把{custom}誤當成對米莉拉的祭祀，引起來訪者不滿。",
                "一名逃離鎖鏈商路的族人帶來買家名冊，但奴隸商人宣稱她是罪犯。",
                $"{caravan}希望在留言柱旁開設一天市集，並要求公會保證交易安全。",
                "來訪工匠願意公開一種低科技工藝，但拒絕讓卡利多設備掃描樣品。",
                "送往各族王國的南渡抄本沒有抵達，信使留下的腳印在強盜路障前消失。",
                "附近出現危險野獸，來訪獵手提議共同追蹤並平分獵物。",
                "王國使節要求南渡說明：米莉拉究竟是居民、神明，還是卡利多的外交官。",
                "一處遺跡刻有本族文字與卡利多環印，教會和公會都要求優先調查。"
            };
            string[] variants =
            {
                "暴雨將在今晚封住道路。",
                "對方只願意在不攜帶高科技武器的情況下談判。",
                "白羽聖堂與蝕羽密教同時派人旁聽。",
                "冒險者公會願意作證，但要求所有條件寫進留言冊。"
            };

            int raceOffset = list.Count;
            for (int theme = 0; theme < 10; theme++)
            {
                for (int variant = 0; variant < 4; variant++)
                {
                    int local = theme * 4 + variant;
                    list.Add(new CrossRaceEventEntry
                    {
                        Id = raceOffset + local,
                        Race = race,
                        Title = race + "事件：" + titles[theme] + "・" + (variant + 1),
                        Speaker = names[local % names.Length] + "（" + race + "）",
                        Body = cores[theme] + "\n\n" + variants[variant],
                        PortraitSheet = sheet
                    });
                }
            }
        }
    }
}
