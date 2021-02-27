using System;
using System.Text.Json;

namespace SocialPokerClub.Models
{
    public class MetricsSummary
    {
        // A data structure that represents the most recent set of statistics (used for Azure monitor and for presentation to the players)
        public DateTimeOffset ReadingTimestamp { get; set; }
        public int ActiveRooms { get; set; }
        public long RUsInLastMinute { get; set; }
        public long RUsInLastHour { get; set; }
        public double RUsOverall = ServerState.OurDB.ServerTotalConsumedRUs;
        public int MovesInLastMinute { get; set; }
        public int MovesInLastHour { get; set; }
        public int MovesOverall { get; set; }

        public MetricsSummary()
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