using System.Text.Json;
using System.Text.Json.Serialization;

namespace SevenStuds.Models
{
    public abstract class DatabaseGameItem
    {
        public string gameId { get; set; } // Partition Key: composite key made up of roomId and startTimeUtc separated by a '-'
        public string id { get; set; } // Unique document id: composite key made up of docType and docSeq with no space between
        public string roomId { get; set; }
        public string docType { get; set; }
        public int docSeq { get; set; }
 
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
