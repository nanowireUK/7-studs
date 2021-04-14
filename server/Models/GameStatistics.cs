using System;

namespace SocialPokerClub.Models
{
    public class GameStatistics
    {
        // Used to present some basic statistics of the game
        public DateTimeOffset timeNowUtc { get; set; }
        public DateTimeOffset startTimeUtc { get; set; }
        public int gameMinutes { get; set; }
        public int handsPlayed { get; set; }
        public GameStatistics() {} // Parameterless constructor for use by JSON serlialiser/deserialiser
        public GameStatistics(Game g)
        {
            this.UpdateStatistics(g); // initial population of values
        }
        public void UpdateStatistics(Game g) {
            timeNowUtc = DateTimeOffset.UtcNow;
            startTimeUtc = g.StartTimeUTC;
            gameMinutes = Convert.ToInt32( (DateTimeOffset.UtcNow - g.StartTimeUTC).TotalMinutes);
            handsPlayed = g.HandsPlayedIncludingCurrent;
        }
    }
}