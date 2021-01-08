using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SevenStuds.Models
{
    public abstract class DatabaseGameItem
    {
        public string docRoomId { get; set; }
        public string docGameId { get; set; } // Partition Key: composite key made up of roomId and startTimeUtc separated by a '-'
        public string id { get; set; } // Unique document id: composite key made up of docType and docSeq with no space between
        public string docType { get; set; }
        public int docSeq { get; set; }
        public DateTimeOffset docDateUtc { get; set; }
        public DatabaseGameItem () {
            docDateUtc = DateTimeOffset.UtcNow;
        }
 
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
