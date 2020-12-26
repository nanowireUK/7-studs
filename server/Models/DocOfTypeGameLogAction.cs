using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

// Example:
// 
// {
//     "gameId": "7Studs Main Event-26/12/2020 15:49:29 +00:00",
//     "id": "GameHeader0",
//     "roomId": "7Studs Main Event",
//     "docType": "GameHeader",
//     "docSeq": 0,
//     "administrator": "J",
//     "startTimeUtc": "2020-12-26T15:49:29.4621595+00:00",
//     "endTimeUtc": "9999-12-31T23:59:59.9999999+00:00",
//     "playersInOrderAtStartOfGame": [
//         "J",
//         "R"
//     ],
//     "_rid": "lOoMAOEwpagJAAAAAAAAAA==",
//     "_self": "dbs/lOoMAA==/colls/lOoMAOEwpag=/docs/lOoMAOEwpagJAAAAAAAAAA==/",
//     "_etag": "\"00000000-0000-0000-db9e-b45068fb01d6\"",
//     "_attachments": "attachments/",
//     "_ts": 1608997773
// }

namespace SevenStuds.Models
{
    public class DocOfTypeGameLogAction
    {
        public string gameId { get; set; } // Composite key made up of roomId and startTimeUtc separated by a '-'
        public string id { get; set; } // Composite key made up of docType and docSeq with no space between
        public string roomId { get; set; }
        public string docType { get; set; }
        public int docSeq { get; set; }
        public GameLogAction action { get; set; }
 
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
