using System;
using System.Text.Json;

namespace SocialPokerClub.Models
{
    public class SpcMetricsSummary
    {
        // A data structure that represents the most recent set of statistics (used for Azure monitor and for presentation to the players)
        public DateTimeOffset ReadingTimestamp { get; set; }
        public int RoomsActiveInLastHr { get; set; }
        public long RUsInLastMin { get; set; }
        public long RUsInLastHr { get; set; }
        public long RUsOverall { get; set; }
        public int MovesInLastMin { get; set; }
        public int MovesInLastHr { get; set; }
        public int MovesOverall { get; set; }

        public SpcMetricsSummary()
        {
        }
        public string AsJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
            };
            string jsonString = JsonSerializer.Serialize(this, options);
            return jsonString;
        }
    }
}