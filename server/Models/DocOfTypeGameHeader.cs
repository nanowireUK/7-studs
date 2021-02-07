using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class DocOfTypeGameHeader : DatabaseGameItem
    {
        public string administrator { get; set; }
        public DateTimeOffset startTimeUtc { get; set; }
        public DateTimeOffset endTimeUtc { get; set; }
        public List<string> playersInOrderAtStartOfGame { get; set; }
        public List<string> playersStartingBlind { get; set; }
        public LobbySettings lobbySettings { get; set; }

        public override string ToString()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            options.Converters.Add(new JsonStringEnumConverter(null /*JsonNamingPolicy.CamelCase*/));
            string jsonString = JsonSerializer.Serialize(this, options);
            return jsonString;
        }
    }
}
