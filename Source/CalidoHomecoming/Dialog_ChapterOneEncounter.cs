using System;
using UnityEngine;
using Verse;

namespace CalidoHomecoming
{
    public sealed class Dialog_ChapterOneEncounter : Window
    {
        private readonly string title;
        private readonly string speaker;
        private readonly string body;
        private readonly Choice[] choices;
        private readonly string portraitPath;

        public override Vector2 InitialSize => new Vector2(860f, 590f);

        public Dialog_ChapterOneEncounter(string title, string speaker, string body, params Choice[] choices)
            : this(title, speaker, body, null, choices)
        {
        }

        public Dialog_ChapterOneEncounter(string title, string speaker, string body, string portraitPath, params Choice[] choices)
        {
            this.title = title;
            this.speaker = speaker;
            this.body = body;
            this.choices = choices;
            this.portraitPath = portraitPath;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
            forcePause = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 38f), title);
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 42f, inRect.width, 28f), speaker);

            Rect portraitRect = new Rect(0f, 78f, 205f, 330f);
            CalidoPortraits.Draw(portraitRect, portraitPath, speaker, CalidoPortraits.GuessFemale(speaker));
            Rect textRect = new Rect(220f, 78f, inRect.width - 220f, 330f);
            Widgets.DrawMenuSection(textRect);
            Widgets.Label(textRect.ContractedBy(20f), body);

            float width = (inRect.width - 20f) / choices.Length;
            for (int i = 0; i < choices.Length; i++)
            {
                Choice choice = choices[i];
                Rect button = new Rect(i * (width + 10f), inRect.height - 100f, width, 84f);
                bool enabled = choice.Enabled == null || choice.Enabled();
                if (Widgets.ButtonText(button, choice.Label, active: enabled))
                {
                    choice.Action?.Invoke();
                    Close();
                }
                if (!string.IsNullOrEmpty(choice.Tooltip))
                {
                    TooltipHandler.TipRegion(button, choice.Tooltip);
                }
            }
        }

        public sealed class Choice
        {
            public string Label;
            public string Tooltip;
            public Func<bool> Enabled;
            public Action Action;

            public Choice(string label, string tooltip, Action action, Func<bool> enabled = null)
            {
                Label = label;
                Tooltip = tooltip;
                Action = action;
                Enabled = enabled;
            }
        }
    }
}
