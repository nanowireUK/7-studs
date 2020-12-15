using System;

namespace SevenStuds.Models
{
    public class LeavingRecord
    {
        public string LeavingParticipantName { get; set; }
        public DateTimeOffset LeftAt_UTC { get; set; }
        public string LeavingParticipantLevelSignalRGroupName { get; set; }
        public int ChipsAtEndOfGame { get; set; }
        public Boolean HasBeenPartOfGame { get; set; }
        public Boolean WasSpectator { get; set; }
        public LeavingRecord( string argLeavingParticipantName, string argGroupName, int argChipsAtEndOfGame, Boolean argHasBeenPartOfGame, Boolean argWasSpectator )
        {
            this.LeavingParticipantName = argLeavingParticipantName;
            this.LeftAt_UTC = DateTimeOffset.Now;
            this.LeavingParticipantLevelSignalRGroupName = argGroupName;
            this.ChipsAtEndOfGame = argChipsAtEndOfGame;
            this.HasBeenPartOfGame = argHasBeenPartOfGame;
            this.WasSpectator = argWasSpectator;
        }
    }
}