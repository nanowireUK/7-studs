using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SevenStuds.Models
{
    public class GameLog
    {
        // Used to record the progress of a game so that it can later be replayed (mainly intended for retesting scenarios)
        public string roomId { get; set; } // Records room name
        public string administrator { get; set; } // Records name of administrator
        public int pauseAfter { get; set; } // Used only by ActionReplay ... enables user to request replayed game to be paused after a specific numbered action
        public int indexOfLastReplayedAction { get; set; } 
        public DateTimeOffset startTimeUtc { get; set; }
        public DateTimeOffset endTimeUtc { get; set; }
        public List<string> playersInOrderAtStartOfGame { get; set; } // Records player names in the order that the game was played
        public List<GameLogAction> actions { get; set; }
        public List<Deck> decks { get; set; } // Records start state of each deck used (so it can be redealt in the same order)
        public GameLog() {
            this.startTimeUtc = DateTimeOffset.Now;
            this.playersInOrderAtStartOfGame = new List<string>();
            this.actions = new List<GameLogAction>();
            this.decks = new List<Deck>();
            this.pauseAfter = 0; 
            this.indexOfLastReplayedAction = -1; // means that next one will be 0
        }

        public void LogEndOfHand(Deck deckUsedForHandJustEnded) {
            // We'll only know for sure that this is the end of the overall game if someone starts a new one instead of continuing with this one
            this.endTimeUtc = DateTimeOffset.Now;
            this.decks.Add(deckUsedForHandJustEnded);
            //this._GameLog.decks.Add(this.CardPack.Clone());
                        
        }
        public string AsJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string jsonString = JsonSerializer.Serialize(this, options);
            return jsonString;
        }  
    }
}