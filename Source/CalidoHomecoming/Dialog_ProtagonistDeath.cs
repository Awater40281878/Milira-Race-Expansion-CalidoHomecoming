using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CalidoHomecoming
{
    public sealed class Dialog_ProtagonistDeath : Window
    {
        private readonly CalidoStoryState state;
        private readonly float openedAt;

        public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);

        public Dialog_ProtagonistDeath(CalidoStoryState state)
        {
            this.state = state;
            openedAt = Time.realtimeSinceStartup;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnCancel = false;
            doCloseX = false;
            draggable = false;
            resizeable = false;
            layer = WindowLayer.Super;

            SoundDef deathSound = DefDatabase<SoundDef>.GetNamedSilentFail("Calido_YouDied");
            if (deathSound != null)
            {
                SoundStarter.PlayOneShotOnCamera(deathSound, Find.CurrentMap);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Widgets.DrawBoxSolid(inRect, new Color(0f, 0f, 0f, 0.92f));
            float elapsed = Time.realtimeSinceStartup - openedAt;
            DrawDeathAnimation(inRect, elapsed);

            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;
            if (elapsed < 5f)
            {
                Widgets.Label(new Rect(0f, inRect.height * 0.68f, inRect.width, 35f), "天命中斷。請直視終局。 " + (5 - elapsed).ToString("0.0"));
            }
            else
            {
                Widgets.Label(new Rect(0f, inRect.height * 0.54f, inRect.width, 80f), "主角已死亡。你可以讀取舊存檔，或保留屍體並設法復活她。\n在主角復活以前，主線、授權與大部分劇情內容都會停止。\n不要遺失主角屍體。\n\n你要如何處理命運？");
                float width = 240f;
                if (Widgets.ButtonText(new Rect(inRect.width * 0.5f - width - 12f, inRect.height * 0.68f, width, 48f), "讀檔"))
                {
                    Close();
                    Find.WindowStack.Add(new Dialog_SaveFileList_Load());
                }
                if (Widgets.ButtonText(new Rect(inRect.width * 0.5f + 12f, inRect.height * 0.68f, width, 48f), "不讀檔，繼續世界"))
                {
                    state.AcknowledgeProtagonistDeath();
                    Close();
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static void DrawDeathAnimation(Rect inRect, float elapsed)
        {
            const int frameRate = 25;
            const int frameCount = 125;
            int frame = Mathf.Clamp(Mathf.FloorToInt(elapsed * frameRate), 0, frameCount - 1);
            Texture2D texture = ContentFinder<Texture2D>.Get("UI/Overlays/YouDied/frame_" + frame.ToString("000"), false);
            if (texture == null)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Medium;
                GUI.color = new Color(0.85f, 0.05f, 0.05f);
                Widgets.Label(new Rect(0f, inRect.height * 0.30f, inRect.width, 90f), "YOU DIED");
                GUI.color = Color.white;
                return;
            }

            float scale = Mathf.Min(inRect.width / texture.width, inRect.height / texture.height) * 0.76f;
            float width = texture.width * scale;
            float height = texture.height * scale;
            Rect rect = new Rect(inRect.center.x - width / 2f, inRect.height * 0.17f, width, height);
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
        }
    }
}
