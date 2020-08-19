using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SevenStuds.Models
{
    public class GameLog
    {
        // Used to record the progress of a game so that it can later be replayed (mainly intended for retesting scenarios)
        public List<string> playersInOrder { get; set; } // Records player names in the order that the game was played
        public List<GameLogAction> actions { get; set; }
        public List<Deck> decks { get; set; } // Records start state of each deck used (so it can be redealt in the same order)
        public GameLog() {
            this.playersInOrder = new List<string>();
            this.decks = new List<Deck>();
            this.actions = new List<GameLogAction>();
        }
        public string AsJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            //options.Converters.Add(new JsonStringEnumConverter(null /*JsonNamingPolicy.CamelCase*/));
            string jsonString = JsonSerializer.Serialize(this, options);
            return jsonString;
        }  
    }
}