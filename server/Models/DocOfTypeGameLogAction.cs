using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class DocOfTypeGameLogAction : DatabaseGameItem
    {
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
