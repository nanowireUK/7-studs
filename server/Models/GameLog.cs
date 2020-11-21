using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SevenStuds.Models
{
    public class GameLog
    {
        // Used to record the progress of a game so that it can later be replayed (mainly intended for retesting scenarios)
        public DateTimeOffset startTimeUtc { get; set; }
        public DateTimeOffset endTimeUtc { get; set; }
        public string administrator { get; set; } // Records name of administrator
        public List<string> playersInOrderAtStartOfGame { get; set; } // Records player names in the order that the game was played
        public List<GameLogAction> actions { get; set; }
        public List<Deck> decks { get; set; } // Records start state of each deck used (so it can be redealt in the same order)
        public GameLog() {
            this.startTimeUtc = DateTimeOffset.Now;
            this.playersInOrderAtStartOfGame = new List<string>();
            this.decks = new List<Deck>();
            this.actions = new List<GameLogAction>();
        }

        public void RecordProvisionalEndTime() {
            // We'll only know for sure that this is the end of the game if someone starts a new one instead of continuing with this one
            this.endTimeUtc = DateTimeOffset.Now;
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