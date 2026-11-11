using System.Collections.Generic;
using Verse;

namespace CalidoHomecoming
{
    public sealed class Dialog_ProtagonistName : Dialog_NamePawn
    {
        private readonly CalidoStoryState state;
        private readonly CalidoPrologueChoice choice;

        public Dialog_ProtagonistName(Pawn pawn, CalidoStoryState state, CalidoPrologueChoice choice)
            : base(pawn, NameFilter.First | NameFilter.Nick | NameFilter.Last,
                NameFilter.First | NameFilter.Nick | NameFilter.Last,
                new Dictionary<NameFilter, List<string>>())
        {
            this.state = state;
            this.choice = choice;
            doCloseX = false;
            closeOnCancel = false;
            closeOnClickedOutside = false;
        }

        public override void PostClose()
        {
            base.PostClose();
            state.CompletePrologue(choice);
        }
    }
}
