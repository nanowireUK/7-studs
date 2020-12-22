using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class DocOfTypeGameHeader
    {
        [JsonProperty(PropertyName = "id")]
        public string gameId { get; set; } // Composite key made up of roomId and startTimeUtc separated by a '-'
        public string docId { get; set; } // Composite key made up of docType and docSeq with no space between
        public string roomId { get; set; }
        public string docType { get; set; }
        public int docSeq { get; set; }
        public string administrator { get; set; }
        public DateTimeOffset startTimeUtc { get; set; }
        public DateTimeOffset endTimeUtc { get; set; }
        public List<string> playersInOrderAtStartOfGame { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
