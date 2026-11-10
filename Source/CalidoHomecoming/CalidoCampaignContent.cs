namespace CalidoHomecoming
{
    public sealed class CampaignBeat
    {
        public readonly string Chapter;
        public readonly string Title;
        public readonly string Body;
        public readonly string HeroName;
        public readonly string HeroTitle;
        public readonly string Specialty;

        public CampaignBeat(string chapter, string title, string body, string heroName, string heroTitle, string specialty)
        {
            Chapter = chapter; Title = title; Body = body; HeroName = heroName; HeroTitle = heroTitle; Specialty = specialty;
        }
    }

    public static class CalidoCampaignContent
    {
        public static readonly CampaignBeat[] Beats =
        {
            new CampaignBeat("第二章・斷翼商路", "失蹤車隊", "追查斷翼木牌，救出被押往北道的亞人俘虜。", "卡西婭", "醫師", "Calido_Hero_Medicine"),
            new CampaignBeat("第二章・斷翼商路", "黑市名冊", "潛入地下拍賣場，取得鎖鏈商路的買家名冊。", "賽奧法諾", "談判官", "Calido_Hero_Social"),
            new CampaignBeat("第二章・斷翼商路", "公會懸賞", "將奴隸販子活捉交給冒險者公會，而非就地處決。", "尼基塔絲", "獵手", "Calido_Hero_Ranged"),
            new CampaignBeat("第二章・斷翼商路", "斷鏈之夜", "摧毀商路中繼站，選擇安置獲救者而不是把他們變成新兵。", "歐多西婭", "建築師", "Calido_Hero_Construction"),
            new CampaignBeat("第三章・天空的證人", "巡察使前站", "一支自稱卡利多前站的地面隊伍要求接管所有白翼者。", "普羅科匹婭", "近衛", "Calido_Hero_Melee"),
            new CampaignBeat("第三章・天空的證人", "被刪除的航標", "修復三座低科技訊號塔，拼回天空封鎖前的最後座標。", "伊琳娜", "研究員", "Calido_Hero_Research"),
            new CampaignBeat("第三章・天空的證人", "假冒執行官", "識破使用地面武器冒充母城執行官的獵奴隊。", "安娜斯塔西婭", "劍士", "Calido_Hero_Melee"),
            new CampaignBeat("第三章・天空的證人", "翎衡抵達", "巡察使翎衡降落地面；她的裝備被強制降級，並要求正式聽證。", "佐伊", "記錄官", "Calido_Hero_Social"),
            new CampaignBeat("第四章・聽證會", "南渡證詞", "提交留言冊、獲救抄經員與斷翼商路名冊。", "狄奧多拉", "農藝師", "Calido_Hero_Plants"),
            new CampaignBeat("第四章・聽證會", "被供奉者的證詞", "證明地面米莉拉既不是神，也不是卡利多可隨意回收的財產。", "瑪麗娜", "工匠", "Calido_Hero_Crafting"),
            new CampaignBeat("第四章・聽證會", "交叉質問", "用切斷地面節點的命令反問巡察使：是誰讓偽神諭得以壟斷消息。", "尤多基婭", "審計官", "Calido_Hero_Social"),
            new CampaignBeat("第四章・聽證會", "有限登城權", "取得登城資格；卡利多科技仍需逐項校準，而非一次全部解鎖。", "阿加莎", "狙擊手", "Calido_Hero_Sniper"),
            new CampaignBeat("第五章・卡利多門階", "民用授權", "完成母城身分校準，解除生活、醫療與運輸設備封鎖。", "克莉絲蒂娜", "廚師", "Calido_Hero_Cooking"),
            new CampaignBeat("第五章・卡利多門階", "防具授權", "通過地面戰鬥紀錄審核，解除個人防具封鎖。", "芭芭拉", "盾衛", "Calido_Hero_Melee"),
            new CampaignBeat("第五章・卡利多門階", "武器授權", "證明地面盟友不受卡利多軍法管轄，解除高階武器封鎖。", "卡琳妮", "射手", "Calido_Hero_Ranged"),
            new CampaignBeat("第五章・卡利多門階", "米莉安授權", "調查三台特殊機械生物的來源，恢復有限米莉安指揮權。", "佩拉吉婭", "機械師", "Calido_Hero_Crafting"),
            new CampaignBeat("第六章・偽神諭案", "抄經所潛入", "以抄寫員、送餐者或被捕信徒身分進入蝕羽密教抄經所。", "克謝妮婭", "潛行者", "Calido_Hero_Melee"),
            new CampaignBeat("第六章・偽神諭案", "原稿與改稿", "找出把卡利多行政命令改寫成神諭的原始模板。", "塔季揚娜", "學者", "Calido_Hero_Research"),
            new CampaignBeat("第六章・偽神諭案", "墮天使契約", "拒絕獻祭，卻保留以自身健康為代價的短暫復生烙印。", "葉夫普拉克西婭", "守望者", "Calido_Hero_Ranged"),
        };
    }
}
