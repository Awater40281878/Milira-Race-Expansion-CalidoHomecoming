using UnityEngine;
using Verse;

namespace CalidoHomecoming
{
    [StaticConstructorOnStartup]
    public static class CalidoPortraits
    {
        private const string DefaultPath = "UI/Portraits/DefaultGroundSpeaker";
        private const string FallbackGridPath = "UI/Portraits/NPCFallbackGrid4x4";
        private static Texture2D defaultGroundSpeaker;
        private static Texture2D fallbackGrid;

        public static Texture2D DefaultGroundSpeaker => defaultGroundSpeaker ??= ContentFinder<Texture2D>.Get(DefaultPath, true);
        public static Texture2D FallbackGrid => fallbackGrid ??= ContentFinder<Texture2D>.Get(FallbackGridPath, true);

        public static Texture2D GetOrDefault(string texturePath)
        {
            if (!string.IsNullOrEmpty(texturePath) && !texturePath.StartsWith("sheet:"))
            {
                Texture2D texture = ContentFinder<Texture2D>.Get(texturePath, false);
                if (texture != null) return texture;
            }
            return DefaultGroundSpeaker;
        }

        public static void Draw(Rect rect, string texturePath = null, string identityKey = null, bool? female = null)
        {
            Widgets.DrawMenuSection(rect);
            Rect inner = rect.ContractedBy(6f);
            if (!string.IsNullOrEmpty(texturePath) && !texturePath.StartsWith("sheet:"))
            {
                GUI.DrawTexture(inner, GetOrDefault(texturePath), ScaleMode.ScaleToFit, true);
                return;
            }

            Texture2D sheet = FallbackGrid;
            if (!string.IsNullOrEmpty(texturePath) && texturePath.StartsWith("sheet:"))
                sheet = ContentFinder<Texture2D>.Get(texturePath.Substring(6), true);
            int index = PortraitIndex(identityKey, female);
            int column = index % 4;
            int rowFromTop = index / 4;
            Rect uv = new Rect(column * 0.25f, 1f - ((rowFromTop + 1) * 0.25f), 0.25f, 0.25f);
            GUI.DrawTextureWithTexCoords(inner, sheet, uv, true);
        }

        public static bool? GuessFemale(string identity)
        {
            if (string.IsNullOrEmpty(identity)) return null;
            string[] femaleNames =
            {
                "伊蕾希雅", "卡西婭", "賽奧法諾", "尼基塔絲", "歐多西婭", "普羅科匹婭", "伊琳娜", "安娜斯塔西婭", "佐伊",
                "狄奧多拉", "瑪麗娜", "尤多基婭", "阿加莎", "克莉絲蒂娜", "芭芭拉", "卡琳妮", "佩拉吉婭", "克謝妮婭", "塔季揚娜", "葉夫普拉克西婭", "少女", "母親", "女"
            };
            foreach (string marker in femaleNames) if (identity.Contains(marker)) return true;
            string[] maleMarkers = { "頭目", "押運官", "長老", "男", "大司祭" };
            foreach (string marker in maleMarkers) if (identity.Contains(marker)) return false;
            return null;
        }

        private static int PortraitIndex(string key, bool? female)
        {
            uint hash = 2166136261u;
            string value = string.IsNullOrEmpty(key) ? "Calido_DefaultNPC" : key;
            foreach (char c in value)
            {
                hash ^= c;
                hash *= 16777619u;
            }
            int withinGender = (int)(hash % 8u);
            if (female == true) return withinGender;
            if (female == false) return 8 + withinGender;
            return (int)(hash % 16u);
        }
    }
}
