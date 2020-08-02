using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        public List<string> HandCommentary { get; set; }
        public int HandsPlayedIncludingCurrent { get; set; } // 0 = game not yet started
        public int IndexOfParticipantDealingThisHand { get; set; } // Rotates from player 0
        public int IndexOfParticipantToTakeNextAction { get; set; } // Determined by cards showing (at start of round) then on player order
        public int _CardsDealtIncludingCurrent { get; set; } // 0 = hand not started
        public int _IndexOfLastPlayerToRaise { get; set; } 
        public int _IndexOfLastPlayerToStartChecking { get; set; } 
        public bool _CheckIsAvailable { get; set; }

        //public List<ActionEnum> _ActionsNowAvailableToCurrentPlayer { get; set; }
        //public List<ActionEnum> _ActionsNowAvailableToAnyPlayer { get; set; }
        public List<List<int>> Pots { get; set; } // pot(s) built up in the current hand (over multiple rounds of betting)
        public List<Participant> Participants { get; set; } // ordered list of participants (order represents order around the table)
        //public List<Event> Events { get; set; } // ordered list of events associated with the game
        private Dictionary<ActionEnum, ActionAvailability> _ActionAvailability { get; set; } // Can't be public as JSON serialiser can't handle it
        public List<ActionAvailability> ActionAvailabilityList { get; set; } // This list contains references to the same objects as in the Dictionary       
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
            HandCommentary = new List<string>();
            _ActionAvailability = new Dictionary<ActionEnum, ActionAvailability>();
            ActionAvailabilityList = new List<ActionAvailability>();
            foreach ( ActionEnum e in Enum.GetValues(typeof(ActionEnum)) ) 
            {
                SetActionAvailability(e, AvailabilityEnum.NotAvailable); // All commands initially unavailable
            }
            SetActionAvailability(ActionEnum.Join, AvailabilityEnum.AnyUnregisteredPlayer); // Open up JOIN to anyone who has not yet joined
        }
        public static Game FindOrCreateGame(string gameId) {
            if ( SevenStuds.Models.ServerState.GameList.ContainsKey(gameId) ) {
                return (Game) SevenStuds.Models.ServerState.GameList[gameId];
            }
            else {
                return new Game(gameId);
            }
        }

        public void SetActionAvailability(ActionEnum ac, AvailabilityEnum av) 
        {
            ActionAvailability aa;
            if ( _ActionAvailability.TryGetValue(ac, out aa) )
            {
                aa.Availability = av;
            }
            else 
            {
                aa = new ActionAvailability(ac, av);
                _ActionAvailability.Add(ac, aa); // Note we are adding the ActionAvailability object as the value
                ActionAvailabilityList.Add(aa); // Also add it to the list that is in included in the JSON export 

            }
        }

        public bool ActionIsAvailableToThisPlayerAtThisPoint( ActionEnum ac, int playerIndex ) 
        {
            return true;
        }
        public void InitialiseGame()
        {
            foreach ( Participant p in Participants )
            {
                p.UncommittedChips = this.InitialChipQuantity;
            }
            // Randomise player order: pick a random player, delete and move to front, repeat a few times
            int players = Participants.Count;
            for (int player = 0; player < players; player++) {
                Participant p = Participants[player]; // Get reference to player to be moved
                Participants.RemoveAt(player); // Remove it from the current list
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
                if ( p.UncommittedChips > 0 ) {
                    p.StartNewHandForActivePlayer(this);
                    this.Pots[0].Add(this.Ante); 
                }
                else {
                    p.StartNewHandForBankruptPlayer(this);
                    this.Pots[0].Add(0); // record their place in the pot, but with a zero contribution
                }
            }
            this.IndexOfParticipantToTakeNextAction = GetIndexOfPlayerToBetFirst();
            _IndexOfLastPlayerToRaise = -1;
            //_IndexOfLastPlayerToCall = -1; // not needed ... can be determined from amounts in pots
            _IndexOfLastPlayerToStartChecking = -1; 
            _CheckIsAvailable = true;
            _CardsDealtIncludingCurrent = MaxCardsDealtSoFar();
            SetActionAvailabilityBasedOnCurrentPlayer();
         }

        public void SetActionAvailabilityBasedOnCurrentPlayer() 
        {
            // This should be called whenever the action has passed from one player to another during a game (i.e. once game has started)
            int playerIndex = this.IndexOfParticipantToTakeNextAction;
            Participant p = this.Participants[playerIndex];

            // Decide whether the player can fold at this stage
            // (always available as player becomes inactive on folding and so will never be current player)
            SetActionAvailability(ActionEnum.Fold, AvailabilityEnum.ActivePlayerOnly); 

            // Decide whether the player can check at this stage
            // (possible until someone does something other than checking)
            SetActionAvailability(
                ActionEnum.Check, 
                _CheckIsAvailable ? AvailabilityEnum.ActivePlayerOnly: AvailabilityEnum.NotAvailable
            ); 

            // Decide whether player can call, raise or cover at this stage
            // (depends on their uncommitted funds vs how much they need to match the pot)
            int catchupAmount = MaxChipsInAllPotsForAnyPlayer() - ChipsInAllPotsForSpecifiedPlayer(playerIndex);
            // To raise they need more than the matching amount
            SetActionAvailability(
                ActionEnum.Raise, 
                p.UncommittedChips > catchupAmount ? AvailabilityEnum.ActivePlayerOnly: AvailabilityEnum.NotAvailable); 
            // To call the matching amount needs to be more than zero and they need at least the matching amount
            SetActionAvailability(
                ActionEnum.Call, 
                ( catchupAmount > 0 & p.UncommittedChips >= catchupAmount ) ? AvailabilityEnum.ActivePlayerOnly: AvailabilityEnum.NotAvailable); 
            // To cover they need less than the matching amount
            SetActionAvailability(
                ActionEnum.Cover, 
                p.UncommittedChips < catchupAmount ? AvailabilityEnum.ActivePlayerOnly: AvailabilityEnum.NotAvailable); 
        }
        public bool ActionIsAvailableToPlayer(ActionEnum actionType, int playerIndex) {
            // Check whether specified player is entitled to take this action at this stage (only valid during a started game)
            ActionAvailability aa = this._ActionAvailability.GetValueOrDefault(actionType);
            if ( aa.Availability == AvailabilityEnum.NotAvailable ) {
                return false;
            }
            if ( aa.Availability == AvailabilityEnum.AnyUnregisteredPlayer & playerIndex == -1 ) { 
                return true;
            }               
            if ( aa.Availability == AvailabilityEnum.AnyRegisteredPlayer & playerIndex != -1 ) {
                return true;
            }
            if ( aa.Availability == AvailabilityEnum.ActivePlayerOnly 
                & playerIndex == IndexOfParticipantToTakeNextAction
                & playerIndex != -1 ) 
            { 
                return true;
            }
            return false;
        }
        public void DealNextRound()
        {
            _CardsDealtIncludingCurrent += 1;
            foreach (Participant p in Participants)
            {
                p.PrepareForNextBettingRound(this, _CardsDealtIncludingCurrent);
            }
            this.IndexOfParticipantToTakeNextAction = GetIndexOfPlayerToBetFirst();
            _IndexOfLastPlayerToRaise = -1;
            //_IndexOfLastPlayerToCall = -1;
            _IndexOfLastPlayerToStartChecking = -1;             
            _CheckIsAvailable = true;
            SetActionAvailability(ActionEnum.Check, AvailabilityEnum.ActivePlayerOnly);             
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
            // Start to left of dealer and check everyone (who is still in, and including dealer) for highest visible hand
            // Assumption: a player is still in if they have money in the pot and have not folded.
            // Assumption: someone else other than the dealer must still be in otherwise the hand has ended. 
            // Note: the dealer could be out too.
            //int ZbiLeftOfDealer = (this.IndexOfParticipantDealingThisHand + 1) % Participants.Count;
            int ZbiOfFirstToBet = -1;
            for (int i = 0; i < Participants.Count; i++) 
            {
                int ZbiOfNextPlayerToInspect = (this.IndexOfParticipantDealingThisHand + 1 + i) % Participants.Count;
                if (
                    Participants[ZbiOfNextPlayerToInspect].HasFolded == false // i.e. player has not folded out of this hand
                    && Participants[ZbiOfNextPlayerToInspect].HasCovered == false // i.e. player has not covered the pot 
                    && this.ChipsInAllPotsForSpecifiedPlayer(ZbiOfNextPlayerToInspect) > 0 // i.e. player was in the hand to start off with
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

        public int CountOfPlayersLeftIn() {
           // Count how many players were in the round in the first place and have not folded in the meantime
            int stillIn = 0;
            for (int i = 0; i < Participants.Count; i++) 
            {
                 if ( 
                    Participants[i].HasFolded == false // i.e. player has not folded out of this hand
                    && this.ChipsInAllPotsForSpecifiedPlayer(i) > 0 // i.e. player was in the hand to start off with
                )
                {
                    stillIn += 1; // This player is still in (even if they are no longer able to bet because of covering a pot)
                }
            }
            return stillIn;
        }
        public int GetIndexOfPlayerToBetNext(int currentPlayer)
        {
            // Determine who is next to bet after current player (may be -1 if no players left who can bet, i.e. end of round)
            for (int i = 0; i < Participants.Count - 1 ; i++) // Check all except current player
            { 
                // Check for a complete round of checking
                int ZbiOfNextPlayerToInspect = (currentPlayer + 1 + i) % Participants.Count;
                if ( ZbiOfNextPlayerToInspect == _IndexOfLastPlayerToRaise 
                    || ( this._CheckIsAvailable && ZbiOfNextPlayerToInspect == _IndexOfLastPlayerToStartChecking ) ) 
                {
                    // Have got back round to last player who raised or started a round of checking, so this is the end of the round 
                    return -1; 
                }
                if ( Participants[ZbiOfNextPlayerToInspect].HasFolded == false // i.e. player has not folded out of this hand
                    && Participants[ZbiOfNextPlayerToInspect].HasCovered == false // i.e. player has not covered the pot
                    && this.ChipsInAllPotsForSpecifiedPlayer(ZbiOfNextPlayerToInspect) > 0 // player has been involved in this hand (i.e. is not out)
                ) {
                    return ZbiOfNextPlayerToInspect;
                }
            }
            return -1;
        }  

        public int ChipsInAllPotsForSpecifiedPlayer (int PlayerIndex ) {
            int totalCommitted = 0;
            foreach ( List<int> pot in this.Pots ) {
                totalCommitted += pot[PlayerIndex];
            }
            return totalCommitted;
        }   
        public int ChipsInSpecifiedPotForSpecifiedPlayer (int PotIndex, int PlayerIndex ) {
            return this.Pots[PotIndex][PlayerIndex];
        }           

        public int MaxChipsInAllPotsForAnyPlayer () {
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

        public int MaxChipsInSpecifiedPotForAnyPlayer (int pot) {
            int currentMax = 0;
            for (int i = 0; i < this.Participants.Count; i++) {

                if ( ChipsInSpecifiedPotForSpecifiedPlayer(pot, i) > currentMax) {
                    currentMax = ChipsInSpecifiedPotForSpecifiedPlayer(pot, i);
                }
            }
            return currentMax;
        }  
        public int TotalInSpecifiedPot (int pot) {
            int totalPot = 0;
            for (int player = 0; player < this.Participants.Count; player++) {
                totalPot += this.Pots[pot][player];
            }
            return totalPot;
        }    
        public void MoveAmountToPotForSpecifiedPlayer  (int playerIndex, int amt) {
            // Add amount to pots, filling up earlier pots before adding to open one,
            // and splitting the pot automatically if player comes up short of total pot so far
            int amountLeftToAdd = amt;
            this.Participants[playerIndex].UncommittedChips -= amt; // Reduce the player's pile of chips before adding them to the various pots
            AddCommentary("Adding "+ amountLeftToAdd +" to the pot (existing pot structure will be analysed)");
            for ( int pot = 0; pot < Pots.Count; pot++) {
                if ( amountLeftToAdd > 0) {
                    int maxContributionToThisPotByAnyPlayer = MaxChipsInSpecifiedPotForAnyPlayer(pot);
                    AddCommentary("Max chips currently in pot " + (pot+1) + " = " + maxContributionToThisPotByAnyPlayer);
                    int myExistingContributionToThisPot = ChipsInSpecifiedPotForSpecifiedPlayer (pot, playerIndex);
                    if ( pot == Pots.Count - 1) {
                        // This is the open pot
                        if ( amountLeftToAdd >= ( maxContributionToThisPotByAnyPlayer - myExistingContributionToThisPot ) ) {
                            // Player is calling or raising
                            AddCommentary("Adding "+ amountLeftToAdd +" to open pot");
                            this.Pots[pot][playerIndex] += amountLeftToAdd;
                            amountLeftToAdd = 0;
                        }
                        else {
                            // User is short of the amount required to stay in and is covering the pot
                            AddCommentary("Adding "+ amountLeftToAdd +" to open pot (should be as a result of covering the pot)");
                            this.Pots[pot][playerIndex] += amountLeftToAdd;
                            amountLeftToAdd = 0;
                            SplitPotAbovePlayersAmount(pot,  playerIndex); // Will change pot structure but this loop will end anyway as nothing left to add
                        }                        
                    }
                    else {
                        // We are currently filling pots that have already been superseded by the open pot
                        int shortfallForThisPot = ( maxContributionToThisPotByAnyPlayer - myExistingContributionToThisPot );
                        if ( shortfallForThisPot == 0 ) {
                            AddCommentary("Player is already fully paid in to pot #" + (pot+1));
                        }
                        else if ( amountLeftToAdd >= shortfallForThisPot ) {
                            // Player has at least enough to complete his commitment to this pot
                            AddCommentary("Adding "+ shortfallForThisPot +" to complete commitment to pot #" + (pot+1));
                            this.Pots[pot][playerIndex] += shortfallForThisPot;
                            amountLeftToAdd -= shortfallForThisPot;
                        }
                        else {
                            // Player does not have enough to complete their contribution to this pot
                            AddCommentary("Adding "+ amountLeftToAdd +" to partially satisfy commitment to pot #" + (pot+1));
                            this.Pots[pot][playerIndex] += amountLeftToAdd;
                            amountLeftToAdd = 0;
                            SplitPotAbovePlayersAmount(pot,  playerIndex); // Will change pot structure but this loop will end anyway as nothing left to add
                        }
                    }
                }
            }
        }

        public void SplitPotAbovePlayersAmount(int potIndex, int playerIndex) {
            // Find out how much the given player has in the given pot, and split the pot so that higher contributions are moved to a new pot
            int potLimit = ChipsInSpecifiedPotForSpecifiedPlayer(potIndex, playerIndex);
            AddCommentary("Splitting pot " + (potIndex+1) + " with contribution cap of " + potLimit);
            Pots.Insert(potIndex+1, new List<int>());
            for ( int player = 0; player < Pots[potIndex].Count; player++) {
                // Move any surplus amounts from old pot to new for each player
                if ( Pots[potIndex][player] > potLimit ) {
                    Pots[potIndex+1].Add( Pots[potIndex][player] - potLimit); // Move surplus to new pot
                    Pots[potIndex][player] = potLimit; //Limit current pot contribution
                }
                else {
                    Pots[potIndex+1].Add(0); // No surplus to move for this player
                }
            }
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

        public string ProcessEndOfHand(string Trigger) {
            // Something has triggered the end of the game. Distribute each pot according to winner(s) of that pot.
            // Start with oldest pot and work forwards. 
            // Only players who have contributed to a pot and have not folded are to be considered
            AddCommentary(Trigger);
            List<int> currentWinners = new List<int>();
            for (int pot = 0; pot < Pots.Count ; pot++) {
                // Identify the player or players who is/are winning this pot
                int winningPlayersHandRank = int.MaxValue; // Low values will win so this is guaranteed to be beaten
                AddCommentary("Determining winner(s) of pot #"+ (pot+1));
                for (int player = 0; player < Participants.Count ; player++) {
                    if ( Participants[player].HasFolded == false && ChipsInSpecifiedPotForSpecifiedPlayer(pot, player) > 0 ) {
                        if ( currentWinners.Count == 0 ) {
                            // First player who is still in, so assume they are the winner until we find out otherwise
                            currentWinners.Add(player);
                            winningPlayersHandRank = Participants[player]._FullHandRank;
                        }
                        else if ( Participants[player]._FullHandRank == winningPlayersHandRank ) {
                            // Record the fact of two or more players at this rank
                            currentWinners.Add(player);
                        }
                        else if ( Participants[player]._FullHandRank < winningPlayersHandRank ) {
                            // New winner
                            currentWinners.Clear(); // Remove details of players winning up to now
                            currentWinners.Add(player);
                            winningPlayersHandRank = Participants[player]._FullHandRank;
                        }
                    }
                }
                // Split the pot across the winning player(s) and record how much everyone won or lost (doing winners first)
                for (int player = 0; player < Participants.Count ; player++) {
                    Participant p = Participants[player];
                    if ( currentWinners.Contains(player) ) {
                        // Give them their share of this pot
                        int tp = TotalInSpecifiedPot(pot);
                        int share = tp / currentWinners.Count; // Discards any fractional winnings ... not sure how else to handle this
                        p.UncommittedChips += share;
                        int inPot = ChipsInSpecifiedPotForSpecifiedPlayer(pot, player);
                        AddCommentary(p.Name + " won " + ( share - inPot ) + " with " + p._HandSummary + " (" + p._FullHandDescription + ")");
                    }
                }
                for (int player = 0; player < Participants.Count ; player++) {
                    Participant p = Participants[player];
                    if ( ! currentWinners.Contains(player) ) {
                        // Record the loss of their investment
                        int inPot = ChipsInSpecifiedPotForSpecifiedPlayer(pot, player);
                        if ( inPot == 0 ) {
                            AddCommentary(p.Name + " had nothing in this pot");
                        }
                        else if ( p.HasFolded ) {
                            AddCommentary(p.Name + " lost " + ( inPot ) + " after folding");
                        }
                        else {
                            AddCommentary(p.Name + " lost " + ( inPot ) + " with " + p._HandSummary + " [rank=" + p._FullHandRank + "] (" + p._FullHandDescription + ")");
                        }
                    }
                }                
            }
            InitialiseHand();
            AddCommentary("End game complete. Started new hand. " + this.Participants[this.IndexOfParticipantToTakeNextAction].Name + " to bet");
            return                                                                                                                                                                                                                                                                                                                                                                                                                                                         this.Participants[this.IndexOfParticipantToTakeNextAction].Name + " to bet";
        }

        public int PlayerIndexFromName(string SearchName) {
            for (int player = 0; player < Participants.Count ; player++) {
                if ( Participants[player].Name == SearchName ) {
                    return player;
                }
            }
            return -1;
        }

        public void AddCommentary (string c){
            this.HandCommentary.Add(c);
        }

        public void ClearCommentary (){
            this.HandCommentary.Clear();
        }
        public string AsJson()
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