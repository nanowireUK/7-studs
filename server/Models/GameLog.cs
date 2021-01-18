using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

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
        public List<string> playersStartingBlind { get; set; }
        public List<Deck> decks { get; set; } // Records start state of each deck used (so it can be redealt in the same order)
        public List<GameLogAction> actions { get; set; }
        public GameLog() {
            this.startTimeUtc = DateTimeOffset.UtcNow;
            this.playersInOrderAtStartOfGame = new List<string>();
            this.playersStartingBlind = new List<string>();
            this.actions = new List<GameLogAction>();
            this.decks = new List<Deck>();
            this.pauseAfter = 0; 
            this.indexOfLastReplayedAction = -1; // means that next one will be 0
        }
        public void ListDecks() {
            foreach ( Deck d in decks ) {
                Console.WriteLine("Deck #{0}: {1}", d.DeckNumber, d.ToString()); 
            }
        }
        public async Task LogEndOfHand(Game g) {
            // We'll only know for sure that this is the end of the overall game if someone starts a new one instead of continuing with this one
            this.endTimeUtc = DateTimeOffset.UtcNow;
            // Log the deck and the complete game log to the DB
            // var dbTasks = new List<Task>();
            // dbTasks.Add(ServerState.OurDB.UpsertGameLog(g));
            // await Task.WhenAll(dbTasks); // Wait until all of the DB tasks completed
            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
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