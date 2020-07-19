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
        public string GameId { get; }
        public int InitialChipQuantity { get; set; }
        public int Ante { get; set; }
        // Game state 
        public string LastEvent { get; set; }
        public string NextAction { get; set; }
        public int HandsPlayedIncludingCurrent { get; set; } // 0 = game not yet started
        public int IndexOfParticipantDealingThisHand { get; set; } // Rotates from player 0
        public int IndexOfParticipantToTakeNextAction { get; set; } // Determined by cards showing (at start of round) then on player order
        public int _CardsDealtIncludingCurrent { get; set; } // 0 = hand not started
        public int _IndexOfLastPlayerToRaise { get; set; } 
        public int _IndexOfLastPlayerToStartChecking { get; set; } 
        public bool _CheckIsAvailable { get; set; } 
        public List<List<int>> Pots { get; set; } // pots built up in the current hand (over multiple rounds of betting)
        public List<Participant> Participants { get; set; } // ordered list of participants (order represents order around the table)
        //public List<Event> Events { get; set; } // ordered list of events associated with the game
        public List<Boolean> CardPositionIsVisible { get; } = new List<Boolean>{false, false, true, true, true, true, false};

        //public List<int> contributionsPerPlayer;
        //private List<string> UndealtCards { get; set; } // cards not yet assigned to specific players in this hand
        private Deck CardPack { get; set; }

        public Game(string gameId) {
            GameId = gameId;
            Participants = new List<Participant>(); // start with empty list of participants
            InitialChipQuantity = 1000;
            Ante = 1;
            HandsPlayedIncludingCurrent = 0;
            CardPack = new Deck(true);
            SevenStuds.Models.ServerState.GameList.Add(GameId, this); // Maps the game id to the game itself (possibly better than just iterating through a list?)            
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
            HandsPlayedIncludingCurrent++;
            IndexOfParticipantDealingThisHand = (HandsPlayedIncludingCurrent - 1) % Participants.Count; // client could work this out too


            // Set up the pack again
            CardPack.Shuffle(); // refreshes the pack and shuffles it

            this.Pots = new List<List<int>>();
            this.Pots.Add(new List<int>());

            foreach (Participant p in Participants)
            {
                p.StartNewHand(this);
                this.Pots[0].Add(this.Ante); // TO DO: only works if player is still in
            }
            this.IndexOfParticipantToTakeNextAction = GetIndexOfPlayerToBetFirst();
            _IndexOfLastPlayerToRaise = -1;
            _IndexOfLastPlayerToStartChecking = -1; 
            _CheckIsAvailable = true;
            _CardsDealtIncludingCurrent = MaxCardsDealtSoFar();
         }

        public void DealNextRound()
        {
            _CardsDealtIncludingCurrent += 1;
            foreach (Participant p in Participants)
            {
                p.PrepareForNextBettingRound(this, _CardsDealtIncludingCurrent);
            }
            this.IndexOfParticipantToTakeNextAction = GetIndexOfPlayerToBetFirst();
            _CheckIsAvailable = true;

        }

        private int MaxCardsDealtSoFar() {
            int maxCardsDealtSoFar = 0;
            foreach (Participant p in Participants)
            {
                if ( p.Hand.Count > maxCardsDealtSoFar ) {
                    maxCardsDealtSoFar = p.Hand.Count;
                }  
            }   
            return maxCardsDealtSoFar  ;      
        }
        int GetIndexOfPlayerToBetFirst()
        {
            // Determine who starts betting in a given round (i.e. after a round of cards have been dealt)
            // Don't assume this is the start of the hand, i.e. this could be after first three cards, or fourth through to seventh.
            // Start to left of dealer and check everyone (who is still in) for highest visible hand
            // Assumption: a player is still in if they have money in the pot and have not folded.
            // Assumption: someone else other than the dealer must still be in otherwise the hand has ended. 
            // Note: the dealer could be out too.
            int ZbiLeftOfDealer = (this.IndexOfParticipantDealingThisHand + 1) % Participants.Count;
            int ZbiOfFirstToBet = -1;
            for (int i = 0; i < Participants.Count; i++) // Note dealer needs to be checked too, but last
            {
                int ZbiOfNextPlayerToInspect = (ZbiLeftOfDealer + 1 + i) % Participants.Count;
                if (
                    Participants[ZbiOfNextPlayerToInspect].HasFolded == false // i.e. player has not folded out of this hand
                    && this.ChipsInThePotForSpecifiedPlayer(ZbiOfNextPlayerToInspect) > 0 // i.e. player was in the hand to start off with
                    && ( // players hand is the first to be checked or is better than any checked so far
                        ZbiOfFirstToBet == -1
                        || Participants[ZbiOfNextPlayerToInspect]._VisibleHandRank < Participants[ZbiOfFirstToBet]._VisibleHandRank
                    )
                )
                {
                    ZbiOfFirstToBet = ZbiOfNextPlayerToInspect; // This player is still in and has a better hand 
                }
            }
            return ZbiOfFirstToBet;
        }  

        public int GetIndexOfPlayerToBetNext(int currentPlayer)
        {
            // Determine who is next to bet after current player (may be -1 if no players left who can bet, i.e. end of round)
            for (int i = 0; i < Participants.Count - 1 ; i++) // Check all except current player
            { 
                int ZbiOfNextPlayerToInspect = (currentPlayer + 1 + i) % Participants.Count;
                if ( ZbiOfNextPlayerToInspect == _IndexOfLastPlayerToRaise 
                    || ( this._CheckIsAvailable && ZbiOfNextPlayerToInspect == _IndexOfLastPlayerToStartChecking ) ) 
                {
                    // Have got back round to last player who raised or started a round of checking, so this is the end of the round 
                    return -1; 
                }
                if ( Participants[ZbiOfNextPlayerToInspect].HasFolded == false // i.e. player has not folded out of this hand
                    && this.ChipsInThePotForSpecifiedPlayer(ZbiOfNextPlayerToInspect) > 0 ) 
                {
                    // i.e. player was still in the hand to start off with
                    return ZbiOfNextPlayerToInspect;
                }
            }
            return -1;
        }  

        public int ChipsInThePotForSpecifiedPlayer (int PlayerIndex ) {
            int totalCommitted = 0;
            foreach ( List<int> pot in this.Pots ) {
                totalCommitted += pot[PlayerIndex];
            }
            return totalCommitted;
        }   

        public int MaxChipsInThePotForAnyPlayer () {
            int currentMax = 0;
            for (int i = 0; i < this.Participants.Count; i++) {
                int playerTotal = 0;
                for (int j = 0; j < this.Pots.Count; j++) {
                    playerTotal += this.Pots[j][i];
                }
                if (playerTotal > currentMax) {
                    currentMax = playerTotal;
                }
            }
            return currentMax;
        }    
        public void AddAmountToCurrentPotForSpecifiedPlayer  (int playerIndex, int amt){
             this.Pots[this.Pots.Count-1][playerIndex] += amt;
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

        public int PlayerIndexFromName(string SearchName) {
            for (int i = 0; i < Participants.Count ; i++) {
                if ( Participants[i].Name == SearchName ) {
                    return i;
                }
            }
            return -1;
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