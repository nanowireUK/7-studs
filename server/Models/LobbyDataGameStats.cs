using System;

namespace SevenStuds.Models
{
    public class LobbyDataGameStats
    {
        // Used to present some basic statistics of the game
        public DateTimeOffset timeNowUtc { get; set; } 
        public DateTimeOffset startTimeUtc { get; set; } 
        public int gameMinutes { get; set; } 
        public int handsPlayed { get; set; } 
        public LobbyDataGameStats(Game g)
        {      
            timeNowUtc = DateTimeOffset.Now;
            startTimeUtc = g.StartTime;
            gameMinutes = Convert.ToInt32( (DateTimeOffset.Now - g.StartTime).TotalMinutes);
            handsPlayed = g.HandsPlayedIncludingCurrent;
        }
    }
}