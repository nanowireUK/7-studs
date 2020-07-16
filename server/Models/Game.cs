using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json;
using System;

namespace SevenStuds.Models
{
    public class Game
    {
        public Game(string gameId) {
            id = gameId;
            Participants = new List<Participant>(); // start with empty list of participants
            InitialChipQuantity = 1000;
            Ante = 1;
            SevenStuds.Models.ServerState.GameList.Add(id, this); // Maps the game id to the game itself (possibly better than just iterating through a list?)            
        }

        public static Game FindOrCreateGame(string gameId) {
            if ( SevenStuds.Models.ServerState.GameList.ContainsKey(gameId) ) {
                return (Game) SevenStuds.Models.ServerState.GameList[gameId];
            }
            else {
                return new Game(gameId);
            }
        }
        public void Initialise()
        {
            foreach ( Participant p in Participants )
            {
                p.UncommittedChips = this.InitialChipQuantity;
            }
            InitialiseHand(); // Start the first hand
        }

        public void InitialiseHand()
        {
            // Set up the pack again
            this.UndealtCards = new List<string> {
                "2C","3C","4C","5C","6C","7C","8C","9C","10C","JC","QC","KC","AC",
                "2D","3D","4D","5D","6D","7D","8D","9D","10D","JD","QD","KD","AD",
                "2H","3H","4H","5H","6H","7H","8H","9H","10H","JH","QH","KH","AH",
                "2S","3S","4S","5S","6S","7S","8S","9S","10S","JS","QS","KS","AS"};
            
            foreach ( Participant p in Participants )
            {
                p.ChipsCommittedToCurrentBettingRound = this.Ante;
                p.UncommittedChips = this.InitialChipQuantity - this.Ante;
                p.HasFolded = false;
                p.Cards = new List<string>();
                p.Cards.Add(this.PickRandomCardFromDeck()); // 1st card
                p.Cards.Add(this.PickRandomCardFromDeck()); // 2nd card
                p.Cards.Add(this.PickRandomCardFromDeck()); // 3rd card
            }
            this.Pots = new List<Pot>();
            this.Pots.Add(new Pot(this.Participants.Count));
        }

        private string PickRandomCardFromDeck() {
            int cardCount = this.UndealtCards.Count;
            Random r = new Random();
            int rInt = r.Next(0, cardCount - 1); //for ints
            string selectedCard = this.UndealtCards[rInt];
            this.UndealtCards.RemoveAt(rInt);
            return selectedCard;
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

        [Required]
        // Fixed game properties
            
        public string id { get; }
        public int InitialChipQuantity { get; set; }
        public int Ante { get; set; }
        public string LastEvent { get; set; }
        public int IndexOfParticipantDealingThisHand { get; set; }
        public int IndexOfParticipantToTakeNextAction { get; set; }

        // Game state 
        public List<Participant> Participants { get; set; } // ordered list of participants (order represents order around the table)
        //public List<Event> Events { get; set; } // ordered list of events associated with the game
        public List<Pot> Pots { get; set; } // pots built up in the current hand (over multiple rounds of betting)
        private List<string> UndealtCards { get; set; } // cards not yet assigned to specific players in this hand
        
        //public int IndexOfLastEvent { get; set; }
        //public string NextAction { get; set; } // Just text ... the clients could potentially work this out from the game state

    }
}