using System;

namespace SevenStuds.Models
{
    public class LeavingRecord
    {
        public string LeavingParticipantName { get; set; }
        public DateTimeOffset EndOfRelevanceToGame_UTC { get; set; } // This should be the earliest of when they went bankrupt or when they physically left the room
        public string LeavingParticipantLevelSignalRGroupName { get; set; }
        public int ChipsAtEndOfGame { get; set; }
        public Boolean HasBeenPartOfGame { get; set; }
        public Boolean WasSpectator { get; set; }
        public LeavingRecord( 
            string argLeavingParticipantName, 
            DateTimeOffset argEndOfRelevanceToGame_UTC,
            string argGroupName, 
            int argChipsAtEndOfGame, 
            Boolean argHasBeenPartOfGame, 
            Boolean argWasSpectator )
        {
            this.LeavingParticipantName = argLeavingParticipantName;
            this.EndOfRelevanceToGame_UTC = argEndOfRelevanceToGame_UTC;
            this.LeavingParticipantLevelSignalRGroupName = argGroupName;
            this.ChipsAtEndOfGame = argChipsAtEndOfGame;
            this.HasBeenPartOfGame = argHasBeenPartOfGame;
            this.WasSpectator = argWasSpectator;
        }
    }
}