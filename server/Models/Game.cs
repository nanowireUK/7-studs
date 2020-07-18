using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json;
using System;

namespace SevenStuds.Models
{
    public class Game
    {
        [Required]
        // Fixed game properties
        public string id { get; }
        public int InitialChipQuantity { get; set; }
        public int Ante { get; set; }
        // Game state 
        public string LastEvent { get; set; }
        public string NextAction { get; set; }
        public int CurrentHand { get; set; } // 0 = game not yet started
        public int ZeroBasedIndexOfParticipantDealingThisHand { get; set; } // Rotates from player 0
        public int ZeroBasedIndexOfParticipantToTakeNextAction { get; set; } // Determined by cards showing (at start of round) then on player order
        public List<Participant> Participants { get; set; } // ordered list of participants (order represents order around the table)
        //public List<Event> Events { get; set; } // ordered list of events associated with the game
        public List<List<int>> Pots { get; set; } // pots built up in the current hand (over multiple rounds of betting)



        //public List<int> contributionsPerPlayer;
        //private List<string> UndealtCards { get; set; } // cards not yet assigned to specific players in this hand
        private Deck CardPack { get; set; }

        public Game(string gameId) {
            id = gameId;
            Participants = new List<Participant>(); // start with empty list of participants
            InitialChipQuantity = 1000;
            Ante = 1;
            CurrentHand = 0;
            CardPack = new Deck(true);
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
        public void InitialiseGame()
        {
            foreach ( Participant p in Participants )
            {
                p.UncommittedChips = this.InitialChipQuantity;
            }
            // Randomise player order here? Pick a random player, delete and add at end, repeat a few times
            int players = Participants.Count;
            for (int i = 0; i < players; i++) {
                Participant p = Participants[i]; // Get reference to player to be moved
                Participants.RemoveAt(i); // Remove it from the current list
                Participants.Insert(0, p); // Move to front of the queue
            }

            InitialiseHand(); // Start the first hand
        }

        public void InitialiseHand()
        {
            CurrentHand++;
            ZeroBasedIndexOfParticipantDealingThisHand = (CurrentHand - 1) % Participants.Count; // client could work this out too 

            // Set up the pack again
            CardPack.Shuffle(); // refreshes the pack and shuffles it

            // this.UndealtCards = new List<string> {
            //     "2c","3c","4c","5c","6c","7c","8c","9c","10c","Jc","Qc","Kc","Ac",
            //     "2d","3d","4d","5d","6d","7d","8d","9d","10d","Jd","Qd","Kd","Ad",
            //     "2h","3h","4h","5h","6h","7h","8h","9h","10h","Jh","Qh","Kh","Ah",
            //     "2s","3s","4s","5s","6s","7s","8s","9s","10s","Js","Qs","Ks","As"};
            
            this.Pots = new List<List<int>>();
            this.Pots.Add(new List<int>());

            foreach ( Participant p in Participants )
            {
                p.StartNewHand(this);
                this.Pots[0].Add(this.Ante);
            }

            // Determine who starts betting. Start to left of dealer and check everyone for highest visible hand
            int ZbiLeftOfDealer = ( this.ZeroBasedIndexOfParticipantDealingThisHand + 1 ) % Participants.Count;
            int ZbiOfFirstToBet = ZbiLeftOfDealer; // Assume left of dealer will bet unless anyone has a better hand.
            for (int i = 0; i < Participants.Count - 1; i++) {
                int ZbiOfNextPlayerToCheck = ( ZbiLeftOfDealer + 1 + i ) % Participants.Count;
                if ( Participants[ZbiOfNextPlayerToCheck].VisibleHandRank < Participants[ZbiOfFirstToBet].VisibleHandRank ) {
                    ZbiOfFirstToBet = ZbiOfNextPlayerToCheck; // This player has a better hand 
                }
            }
            this.ZeroBasedIndexOfParticipantToTakeNextAction = ZbiOfFirstToBet;
        }

        public Card DealCard() {
            return this.CardPack.NextCard(); 
        }

        // private string PickRandomCardFromDeck() {
        //     int cardCount = this.UndealtCards.Count;
        //     Random r = new Random();
        //     int rInt = r.Next(0, cardCount - 1); //for ints
        //     string selectedCard = this.UndealtCards[rInt];
        //     this.UndealtCards.RemoveAt(rInt);
        //     return selectedCard;
        // }
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