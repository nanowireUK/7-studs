using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class DocOfTypeGameHeader
    {
        public string gameId { get; set; } // Composite key made up of roomId and startTimeUtc separated by a '-'
        public string id { get; set; } // Composite key made up of docType and docSeq with no space between
        public string roomId { get; set; }
        public string docType { get; set; }
        public int docSeq { get; set; }
        public string administrator { get; set; }
        public DateTimeOffset startTimeUtc { get; set; }
        public DateTimeOffset endTimeUtc { get; set; }
        public List<string> playersInOrderAtStartOfGame { get; set; }

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
