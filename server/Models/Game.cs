using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SevenStuds.Models
{
    public class Game
    {
        [Required]
        // Fixed game properties
        private Room Room;
        public int GameNumber { get; set; }
        public int HandsPlayedIncludingCurrent { get; set; } // 0 = game not yet started
        public int ActionNumber { get; set; }
        public List<string> rejoinCodes { get; set; }
        public GameModeEnum GameMode { get; set; }
        public int InitialChipQuantity { get; set; }
        public int Ante { get; set; }
        // Game state 
        public string StatusMessage { get; set; }
        public string LastEvent { get; set; }
        public string NextAction { get; set; }
        public List<string> HandCommentary { get; set; }
        //public List<List<string>> LastHandResult { get; set; }
        public List<List<PotResult>> MostRecentHandResult { get; set; } // New way of doing this 
        public int IndexOfParticipantDealingThisHand { get; set; } // Rotates from player 0
        public int IndexOfParticipantToTakeNextAction { get; set; } // Determined by cards showing (at start of round) then on player order
        public int CallAmountForParticipantToTakeNextAction { get; set; } // So that client doesn't need to work this out
        public int MaxRaiseForParticipantToTakeNextAction { get; set; } // So that client doesn't need to work this out
        public int RoundNumberIfCardsJustDealt { get; set; } // So that client know it can animate the deal
        public int RoundNumber { get; set; } 
        public int _CardsDealtIncludingCurrent { get; set; } // 0 = hand not started
        public int _IndexOfLastPlayerToRaise { get; set; } 
        public int _IndexOfLastPlayerToStartChecking { get; set; } 
        public bool _CheckIsAvailable { get; set; }
        public int CountOfLeavers { get; set; }
        public Card CommunityCard { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset LastSuccessfulAction { get; set; }
        public GameLog _GameLog { get; set; }
        protected GameLog _ReplayContext { get; set; }
        public List<LeavingRecord> LeaversLogForGame { get; set; }
        private Dictionary<string, Participant> _ConnectionToParticipantMap { get; set; } 
        private Dictionary<string, Spectator> _ConnectionToSpectatorMap { get; set; } 
        public List<List<int>> Pots { get; set; } // pot(s) built up in the current hand (over multiple rounds of betting)
        public List<Participant> Participants { get; set; } // ordered list of participants (order represents order around the table)
        public List<Spectator> Spectators { get; set; } // ordered list of spectators (no representation around the table)
        private Dictionary<ActionEnum, ActionAvailability> _ActionAvailability { get; set; } // Can't be public as JSON serialiser can't handle it
        public List<ActionAvailability> ActionAvailabilityList { get; set; } // This list contains references to the same objects as in the Dictionary       
        public List<Boolean> CardPositionIsVisible { get; } = new List<Boolean>{false, false, true, true, true, true, false};
        public LobbyData LobbyData { get; set; }
        public GameStatistics GameStatistics { get; set; }
        private Deck CardPack { get; set; } // Starts as a full shuffled deck but is depleted as cards are dealt from it
        public Deck SnapshotOfDeckForCurrentHand { get; set; } // Captures the full shuffled deck as at the start of the hand
        public Game(Room roomRef, int gameNumber) {
            this.Room = roomRef;
            this.GameNumber = gameNumber;
            this.InitialiseGame(null);
        }

        public Room ParentRoom() {
            return this.Room;
        }

        public void InitialiseGame(GameLog replayContext)
        {
            GameMode = GameModeEnum.LobbyOpen;
            LastSuccessfulAction = DateTimeOffset.Now; // This will be updated as the game progresses but need to set a baseline here
            StartTime = DateTimeOffset.Now; // At this point the time just represents the time the game was created
            SetReplayContext(replayContext);
            Participants = new List<Participant>(); // start with empty list of participants
            Spectators = new List<Spectator>(); // start with empty list of spectators
            InitialChipQuantity = 1000;
            Ante = 1;

            //CardPack = new Deck(true);
            HandCommentary = new List<string>();
            //LastHandResult = new List<List<string>>();
            MostRecentHandResult = new List<List<PotResult>>();
            GameStatistics = new GameStatistics(this); // initialise the game stats

            _ConnectionToParticipantMap = new Dictionary<string, Participant>(); 
            _ConnectionToSpectatorMap = new Dictionary<string, Spectator>(); 
            _ActionAvailability = new Dictionary<ActionEnum, ActionAvailability>();
            ActionAvailabilityList = new List<ActionAvailability>();
            HandsPlayedIncludingCurrent = 0;
            LeaversLogForGame = new List<LeavingRecord>();
            CountOfLeavers = 0;
            ActionNumber = 0;
            SetActionAvailabilityBasedOnCurrentPlayer(); // Ensures the initial section of available actions is set
            StartNewGameLog(); // This sets up a dummy log that will just capture joins ahead of any actual games
        }
        public void StartNewGameLog() {
            // Initialise the game log, which will be added to as game actions take place
            this._GameLog = new GameLog(); 
            this._GameLog.roomId = this.ParentRoom().RoomId;
            // Log player order
            this._GameLog.playersInOrderAtStartOfGame = new List<string>();
            foreach ( Participant p in this.Participants ) {
                this._GameLog.playersInOrderAtStartOfGame.Add(p.Name);
                //System.Diagnostics.Debug.WriteLine("Logging player "+p.Name);
                if ( p.IsGameAdministrator ) {
                    this._GameLog.administrator = p.Name; 
                }
            }
            this.ParentRoom().GameLogs.Add(this._GameLog); // Add the game log to the history of game logs for the room
        }
        public void SetReplayContext(GameLog replayContext)
        {
            _ReplayContext = replayContext; // Note: this affects some system behaviour ... need to search for IsRunningInReplayMode() to see where
        }
        public GameLog GetReplayContext()
        {
            return this._ReplayContext;
        }
        public bool IsRunningInReplayMode() {
            return this._ReplayContext != null;
        }

        public void ClearConnectionMappings() {
            _ConnectionToParticipantMap.Clear(); // Clear out the tester's current connection (and any other connections currently associated with the game)
            _ConnectionToSpectatorMap.Clear(); // Clear out the tester's current connection (and any other connections currently associated with the game)
        }

        public void LinkConnectionToParticipant(string connectionId, Participant p) 
        {
            _ConnectionToParticipantMap.Add(connectionId, p);
        }
        public void LinkConnectionToSpectator(string connectionId, Spectator p) 
        {
            _ConnectionToSpectatorMap.Add(connectionId, p);
        }

        public Participant GetParticipantFromConnection(string connectionId) 
        {
            Participant p;
            if ( _ConnectionToParticipantMap.TryGetValue(connectionId, out p) )
            {
                return p;
            }
            else 
            {
                return null;
            }
        }

        public Spectator GetSpectatorFromConnection(string connectionId) 
        {
            Spectator p;
            if ( _ConnectionToSpectatorMap.TryGetValue(connectionId, out p) )
            {
                return p;
            }
            else 
            {
                return null;
            }
        }

        public async Task StartNewGame()
        {
            GameNumber++;
            HandsPlayedIncludingCurrent = 0;
            ActionNumber = 0;
            CountOfLeavers = 0;
            RemoveDisconnectedPlayersFromGameState();
            foreach ( Participant p in Participants )
            {
                p.UncommittedChips = this.InitialChipQuantity;
                p.HandsWon = 0;
            }
            LeaversLogForGame = new List<LeavingRecord>();
            if ( this.IsRunningInReplayMode() == false ) {
                // Normal game, so randomise player order by picking a random player, deleting and moving to front, repeating a few times
                Random r = ServerState.ServerLevelRandomNumberGenerator;
                for (int i = 0; i < 20; i++) {
                    int randomPlayerIndex = r.Next(0, Participants.Count - 1); // Note: include player 0 to ensure randomness even when only two players
                    Participant p = Participants[randomPlayerIndex]; // Get reference to player to be moved
                    Participants.RemoveAt(randomPlayerIndex); // Remove it from the current list
                    Participants.Insert(0, p); // Move to front of the queue
                }
            }
            else {
                // We are running in test mode (i.e. under the control of an ActionReplay command) so set the original player order
                // First remove any players who have not yet joined (allows for players joining between hands)
                System.Diagnostics.Debug.WriteLine("Setting player order as per game log");
                List<String> correctedPlayersInOrder = new List<String>(this._ReplayContext.playersInOrderAtStartOfGame);
                for ( int playerPos = correctedPlayersInOrder.Count - 1; playerPos >= 0; playerPos-- ) {
                    string playerName = correctedPlayersInOrder[playerPos];
                    bool participantExists = false;
                    foreach ( Participant p in Participants) {
                        if ( p.Name == playerName) {
                            participantExists = true;
                            break;
                        }
                    }
                    if ( participantExists == false) {
                        correctedPlayersInOrder.RemoveAt(playerPos);
                        System.Diagnostics.Debug.WriteLine("Removing "+playerName+" from player order as they had not joined at this point");
                    }
                }  
                for ( int requiredPos = 0; requiredPos < correctedPlayersInOrder.Count; requiredPos++ ) {
                    // Find that player who should be at this position and move them to it
                    string playerToMove = correctedPlayersInOrder[requiredPos];
                    int currentIndexOfPlayerToMove = PlayerIndexFromName(playerToMove);
                    if ( requiredPos != currentIndexOfPlayerToMove ) {
                        // We need to remove them from current pos and reinsert them at the required pos
                        Participant p = Participants[currentIndexOfPlayerToMove];
                        Participants.RemoveAt(currentIndexOfPlayerToMove); // Remove it from the current list
                        Participants.Insert(requiredPos, p);
                        System.Diagnostics.Debug.WriteLine("Moved "+p.Name+" to position "+requiredPos);
                    }
                    else {
                        System.Diagnostics.Debug.WriteLine(Participants[requiredPos].Name+" was already at position "+requiredPos);
                    }
                }
                // Set the right player as the administrator 
                foreach ( Participant p in Participants) {
                    if ( p.IsGameAdministrator == false && p.Name == this._ReplayContext.administrator) {
                        p.IsGameAdministrator = true;
                        System.Diagnostics.Debug.WriteLine("Setting "+p.Name+" as administrator for the replay");
                    }
                    if ( p.IsGameAdministrator == true && p.Name != this._ReplayContext.administrator) {
                        p.IsGameAdministrator = false;
                    }  
                }                   
            }
            StartTime = DateTimeOffset.Now;
            StartNewGameLog(); // start a new log for this game
            await ServerState.OurDB.RecordGameStart(this); 
        }

        public void RemoveDisconnectedPlayersFromGameState() {
            // Remove any player(s) that disconnected during the last hand
            for (int player = Participants.Count - 1; player > 0; player--) {
                // Starting from the end of the player array, remove any players that have disconnected during the last hand
                if ( Participants[player].HasDisconnected == true ) {
                    if ( Participants[player].HasBeenActiveInCurrentGame == true ) {
                        for (int i = 0; i < Pots.Count; i++) {
                            Pots[i].RemoveAt(player); // Remove this player's slot from the pot array (the pot should be empty at this point anyway)
                        }
                        if ( player == IndexOfParticipantDealingThisHand ) {
                            // Removed player was the dealer, so notionally move the 'dealership' back one slot (and allow for wraparound)
                            IndexOfParticipantDealingThisHand = ( 
                                IndexOfParticipantDealingThisHand > 0 
                                ? IndexOfParticipantDealingThisHand - 1 // go back one slot
                                : Participants.Count - 1 // wraparound to last player in the list
                            )  ;
                        }
                    }
                    Participants.RemoveAt(player);
                }
            }
        }
        public void ClearRemnantsFromLastGame() {
            // Clear the pots 
            ClearHandDataBetweenHands();
        }
        public bool PotsAreEmpty() {
            // 
            if ( this.Pots == null ){
                return true;
            }
            return this.Pots[0].Count == 0;
        }
        public void InitialisePots() {
            // Reset the pots (one pot only and everyone has 0 until the next hand starts)
            this.Pots = new List<List<int>>();
            this.Pots.Add(new List<int>());
            foreach (Participant p in Participants)
            {
                this.Pots[0].Add(0); 
            }
        }
        public void ClearHandDataBetweenHands() {
            this.InitialisePots();
            // foreach (Participant p in Participants)
            // {
            //     p.Hand = new List<Card>();
            // }
        }

        public void StartNextHand()
        {
            // First remove any player(s) that disconnected during the last hand
            RemoveDisconnectedPlayersFromGameState(); // Shouldn't really be necessary here ... happens mainly on Join, Continue or Open

            HandsPlayedIncludingCurrent++;

            // Change the dealer to be the next player in turn
            if ( HandsPlayedIncludingCurrent == 1) {
                IndexOfParticipantDealingThisHand = 0; // First dealer is first player in the list
            }
            else {
                for (int i = 0; i < Participants.Count; i++) {
                    int nextCandidateForDealer = ( i + IndexOfParticipantDealingThisHand + 1 ) % Participants.Count;
                    if ( Participants[nextCandidateForDealer].HasDisconnected == false ) {
                        IndexOfParticipantDealingThisHand = nextCandidateForDealer;
                        break;
                    }
                }                
            }

            this.ClearCommentary();

            // Set up the pack again
            int newDeckNo = HandsPlayedIncludingCurrent;
            if ( this.IsRunningInReplayMode() == false ) {
                // In normal mode, just create a new deck
                CardPack = new Deck(newDeckNo, true ); 
            }
            else {
                // Need to replace the pack with the next one from the historical game log
                CardPack = this._ReplayContext.decks[HandsPlayedIncludingCurrent - 1].Clone(newDeckNo);
            }

            this.TakeSnapshotOfNewDeck();
            this.ClearHandDataBetweenHands();
            this.GameStatistics.UpdateStatistics(this);

            // Temporarily moved from ClearHandDataBetweenHands
            foreach (Participant p in Participants)
            {
                p.Hand = new List<Card>();
            }

            for (int i = 0; i < Participants.Count; i++ )
            {
                Participant p = Participants[i];
                p.IsSharingHandDetails = false;
                p.HasBeenActiveInCurrentGame = true;
                p.AllInDateTime = DateTimeOffset.MinValue;
                p.LastActionInThisHand = ActionEnum.Undefined;
                p.LastActionAmount = 0;  
                if ( p.UncommittedChips > 0 ) {
                    p.StartNewHandForActivePlayer(this);
                    this.Pots[0][i] = this.Ante; 
                }
                else {
                    p.StartNewHandForBankruptPlayer(this);
                    this.Pots[0][i] = 0; // Should already be 0 but setting it explicity to be clear
                }
            }
            this.IndexOfParticipantToTakeNextAction = GetIndexOfPlayerToBetFirst();
            _IndexOfLastPlayerToRaise = -1;
            _IndexOfLastPlayerToStartChecking = -1; 
            _CheckIsAvailable = true;
            _CardsDealtIncludingCurrent = MaxCardsDealtSoFar();
            RoundNumberIfCardsJustDealt = _CardsDealtIncludingCurrent; // Will be cleared as soon as next action comes in
            RoundNumber = _CardsDealtIncludingCurrent; // Kept for entire round
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


        public void SetActionAvailabilityBasedOnCurrentPlayer() 
        {
            // This should be called at the end of any action

            // First make all commands unavailable
            foreach (ActionEnum e in Enum.GetValues(typeof(ActionEnum)))
            {
                SetActionAvailability(e, AvailabilityEnum.NotAvailable); // All commands initially unavailable
            }

            // Set any commands that are always available
            SetActionAvailability(ActionEnum.Rejoin, AvailabilityEnum.AnyRegisteredPlayer); // Open up REJOIN to anyone who previously joined
            SetActionAvailability(ActionEnum.Leave, AvailabilityEnum.AnyRegisteredPlayer); // Open up LEAVE to anyone who has joined
            SetActionAvailability(ActionEnum.AdHocQuery, AvailabilityEnum.AnyRegisteredPlayer); // Open up test functions to anyone who previously joined
            SetActionAvailability(ActionEnum.Spectate, AvailabilityEnum.AnyUnregisteredPlayer); // Open up SPECTATE at any time to anyone who has not yet joined


            // Set any commands that are only available when we are not on the public server (i.e. test features)
            if ( ! ServerState.IsRunningOnPublicServer() ) {
                SetActionAvailability(ActionEnum.GetState, AvailabilityEnum.AnyRegisteredPlayer); // Open up test functions to anyone who previously joined
                SetActionAvailability(ActionEnum.GetLog, AvailabilityEnum.AnyRegisteredPlayer); // Open up test functions to anyone who previously joined
                SetActionAvailability(ActionEnum.Replay, AvailabilityEnum.AnyRegisteredPlayer); // Open up test functions to anyone who previously joined  
                SetActionAvailability(ActionEnum.GetMyState, AvailabilityEnum.AnyRegisteredPlayer); // Open up test functions to anyone who previously joined
            }
                      
            // Set different actions based on current game mode
            if ( GameMode == GameModeEnum.LobbyOpen ) {
                SetActionAvailability(ActionEnum.Join, AvailabilityEnum.AnyUnregisteredPlayer); // Open up JOIN to anyone who has not yet joined (including spectators)
                SetActionAvailability(ActionEnum.Spectate, AvailabilityEnum.AnyUnregisteredPlayer); // Open up SPECTATE to anyone who has not yet joined
                SetActionAvailability(ActionEnum.Open, AvailabilityEnum.NotAvailable); // OPEN is no longer possible as lobby is already open
                SetActionAvailability(ActionEnum.Start, ( this.Participants.Count >= 2 ) ? AvailabilityEnum.AdministratorOnly : AvailabilityEnum.NotAvailable ); 
                SetActionAvailability(ActionEnum.Continue, 
                    ( this.Participants.Count >= 2 && this.HandsPlayedIncludingCurrent > 0 ) ? AvailabilityEnum.AdministratorOnly : AvailabilityEnum.NotAvailable ); 
            }
            else if ( GameMode == GameModeEnum.HandsBeingRevealed ) {
                // Player can only fold or reveal in this phase
                SetActionAvailability(ActionEnum.Fold, AvailabilityEnum.ActivePlayerOnly); 
                SetActionAvailability(ActionEnum.Reveal, AvailabilityEnum.ActivePlayerOnly); 

            }
            else if ( GameMode == GameModeEnum.HandCompleted ) {
                SetActionAvailability(ActionEnum.Open, AvailabilityEnum.AdministratorOnly); // Admin can choose to reopen lobby at this point
                SetActionAvailability(ActionEnum.Reveal, AvailabilityEnum.AnyUnrevealedRegisteredPlayer); // Players may voluntarily reveal their hands at the end of a hand
                SetActionAvailability(ActionEnum.Continue, AvailabilityEnum.AdministratorOnly); // Admin can choose to continue with the next hand

            }
            else if ( GameMode == GameModeEnum.HandInProgress ) {
                int playerIndex = this.IndexOfParticipantToTakeNextAction;            
                Participant p = this.Participants[playerIndex];
                // Decide whether the player can fold at this stage
                // (actually always available as player becomes inactive on folding and so will never be current player)
                SetActionAvailability(ActionEnum.Fold, AvailabilityEnum.ActivePlayerOnly); 

                // Decide whether the player can check at this stage
                // (possible until someone does something other than checking)
                SetActionAvailability(
                    ActionEnum.Check, 
                    _CheckIsAvailable ? AvailabilityEnum.ActivePlayerOnly: AvailabilityEnum.NotAvailable
                );                 
                // Decide whether player can call, raise or cover at this stage
                // (depends on their uncommitted funds, how much they need to match the pot and other people's funds)
                int catchupAmount = MaxChipsInAllPotsForAnyPlayer() - ChipsInAllPotsForSpecifiedPlayer(playerIndex);
                CallAmountForParticipantToTakeNextAction = catchupAmount;
                MaxRaiseForParticipantToTakeNextAction = MaxRaiseInContextOfOtherPlayersFunds(playerIndex, catchupAmount);

                // To raise they need more than the matching amount and at least one person has to be able to call or cover following the raise
                SetActionAvailability(
                    ActionEnum.Raise, 
                    MaxRaiseForParticipantToTakeNextAction > 0 ? AvailabilityEnum.ActivePlayerOnly: AvailabilityEnum.NotAvailable);                
                // To call the matching amount needs to be more than zero and they need at least the matching amount
                SetActionAvailability(
                    ActionEnum.Call, 
                    ( catchupAmount > 0 & p.UncommittedChips >= catchupAmount ) ? AvailabilityEnum.ActivePlayerOnly: AvailabilityEnum.NotAvailable); 
                // To cover they need less than the matching amount
                SetActionAvailability(
                    ActionEnum.Cover, 
                    p.UncommittedChips < catchupAmount ? AvailabilityEnum.ActivePlayerOnly: AvailabilityEnum.NotAvailable); 
            }
        }
        public bool ActionIsAvailableToPlayer(ActionEnum actionType, int playerIndex) {
            // Check whether specified player is entitled to take this action at this stage (only valid during a started game)
            ActionAvailability aa = this._ActionAvailability.GetValueOrDefault(actionType);
            if ( aa.Availability == AvailabilityEnum.NotAvailable ) {
                return false;
            }
            else if ( aa.Availability == AvailabilityEnum.AnyUnregisteredPlayer & playerIndex == -1 ) { 
                return true;
            }               
            else if ( aa.Availability == AvailabilityEnum.AnyRegisteredPlayer & playerIndex != -1 ) {
                return true;
            }
            else if ( aa.Availability == AvailabilityEnum.AdministratorOnly & playerIndex != -1 & this.Participants[playerIndex].IsGameAdministrator ) {
                return true;
            }
            else if ( aa.Availability == AvailabilityEnum.ActivePlayerOnly & playerIndex != -1 & playerIndex == IndexOfParticipantToTakeNextAction ) { 
                return true;
            }
            else if ( aa.Availability == AvailabilityEnum.AnyUnrevealedRegisteredPlayer & playerIndex != -1 & ! Participants[playerIndex].IsSharingHandDetails ) { 
                return true;
            }
            return false;
        }
        public bool ActionIsAvailableToSpectator(ActionEnum actionType, int spectatorIndex) {
            // Check whether specified spectator is entitled to take this action at this stage
            return actionType == ActionEnum.Leave; // The only thing a spectator can do is leave
        }
        public void DealNextRound()
        {
            _CardsDealtIncludingCurrent += 1;
            RoundNumberIfCardsJustDealt = _CardsDealtIncludingCurrent; // Will be cleared as soon as next action comes in
            RoundNumber = _CardsDealtIncludingCurrent; // Kept for entire round
            CommunityCard = null;
            if ( _CardsDealtIncludingCurrent == 7 && CountOfPlayersLeftInHand() > CardPack.Cards.Count ) {
                // Edge case: we don't have enough cards to deal to all players (can only happen in round 7 if nearly everyone stayed in up to that point)
                CommunityCard = DealCard(); // Random card that will be dealt to all players
            }
            foreach (Participant p in Participants)
            {
                p.PrepareForNextBettingRound(this, _CardsDealtIncludingCurrent);
            }
            this.IndexOfParticipantToTakeNextAction = GetIndexOfPlayerToBetFirst();
            _IndexOfLastPlayerToRaise = -1;
            _IndexOfLastPlayerToStartChecking = -1;             
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
            // Start to left of dealer and check everyone (who is still in, and including dealer) for highest visible hand
            // Assumption: a player is still in if they have money in the pot and have not folded.
            // Assumption: someone else other than the dealer must still be in otherwise the hand has ended. 
            // Note: the dealer could be out too.
            //int ZbiLeftOfDealer = (this.IndexOfParticipantDealingThisHand + 1) % Participants.Count;
            int ZbiOfFirstToBet = -1;
            for (int i = 0; i < Participants.Count; i++) 
            {
                int ZbiOfNextPlayerToInspect = (this.IndexOfParticipantDealingThisHand + 1 + i) % Participants.Count; // starts one to left of dealer
                if (
                    Participants[ZbiOfNextPlayerToInspect].HasFolded == false // i.e. player has not folded out of this hand
                    && Participants[ZbiOfNextPlayerToInspect].HasCovered == false // i.e. player has not covered the pot 
                    && Participants[ZbiOfNextPlayerToInspect].StartedHandWithNoFunds == false // i.e. player was in the hand to start off with
                    //&& this.ChipsInAllPotsForSpecifiedPlayer(ZbiOfNextPlayerToInspect) > 0 // i.e. player was in the hand to start off with
                    && 
                    ( // players hand is the first to be checked or is better than any checked so far
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
        public int CountOfPlayersLeftInHand() {
           // Count how many players were in the round in the first place and have not folded in the meantime
            int stillIn = 0;
            for (int i = 0; i < Participants.Count; i++) 
            {
                 if ( 
                    Participants[i].HasFolded == false // i.e. player has not folded out of this hand
                    && Participants[i].StartedHandWithNoFunds == false // i.e. player has not yet lost all of their funds
                    && this.ChipsInAllPotsForSpecifiedPlayer(i) > 0 // i.e. player was in the hand to start off with
                )
                {
                    stillIn += 1; // This player is still in (includes situation where they are no longer able to bet because of covering a pot)
                }
            }
            return stillIn;
        }
        public int CountOfPlayersLeftInGame() {
           // Count how many players still have funds (this is intended for use after a hand completes)
            int stillIn = 0;
            for (int i = 0; i < Participants.Count; i++) 
            {
                 if ( Participants[i].UncommittedChips > 0 ) {
                    stillIn += 1;  
                }
            }
            return stillIn;
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
        public int MaxRaiseInContextOfOtherPlayersFunds(int playerIndex, int catchupAmount) {
            // Find the maximum level of funds available to any active player other than the specified player.
            // This will be the absolute limit of what the specified player could put into the current hand,
            // but we will then have to look at their own funds and what they have already contributed, as well
            // as what they would have to pay (if any) in order to call, to then determine the maximum raise.
            int maxAvailableToOtherActivePlayers = 0;
            Participant p; 
            for (int i = 0; i < this.Participants.Count; i++) {
                p = this.Participants[i];
                if ( i != playerIndex 
                    && p.HasCovered == false
                    && p.HasFolded == false
                    && p.StartedHandWithNoFunds == false
                ) {
                    int thisPlayersCommittedChips = ChipsInAllPotsForSpecifiedPlayer(i);
                    if ( ( p.UncommittedChips + thisPlayersCommittedChips ) > maxAvailableToOtherActivePlayers ) {
                        maxAvailableToOtherActivePlayers = p.UncommittedChips + thisPlayersCommittedChips;
                    }
                }
            }
            p = this.Participants[playerIndex];
            int currentPlayersCommittedChips = ChipsInAllPotsForSpecifiedPlayer(playerIndex);

            if ( p.UncommittedChips <= catchupAmount ) {
                // We can't raise at all because we don't have more than we need to just catch up
                return 0; 
            }
            else if ( ( currentPlayersCommittedChips + p.UncommittedChips ) > maxAvailableToOtherActivePlayers ) {
                // We have more funds than anyone else so any raise is capped (possibly at zero)
                return maxAvailableToOtherActivePlayers - ( currentPlayersCommittedChips + catchupAmount); 
            } 
            else {
                // We have less committed + uncommitted funds than other players, so we can raise up to our total (after catching up)
                return p.UncommittedChips - catchupAmount;
            }               
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
            if ( this.Participants[playerIndex].UncommittedChips == 0 && amt > 0 ) {
                // Note the time this player went all in (as it can make a difference to the player rankings at the end)
                this.Participants[playerIndex].AllInDateTime = DateTimeOffset.Now;
            }
            // if ( amountLeftToAdd > 0 ) {
            //     AddCommentary(amountLeftToAdd +" is to be added to the pot (or pots)");
            // }
            for ( int pot = 0; pot < Pots.Count; pot++) {
                int myExistingContributionToThisPot = ChipsInSpecifiedPotForSpecifiedPlayer (pot, playerIndex);
                AddCommentary("Own current contribution to pot " + (pot+1) + " = " + myExistingContributionToThisPot);
                int maxContributionToThisPotByAnyPlayer = MaxChipsInSpecifiedPotForAnyPlayer(pot);
                AddCommentary("Highest current contribution to pot " + (pot+1) + " = " + maxContributionToThisPotByAnyPlayer);
                if ( pot == Pots.Count - 1) {
                    // This is the open pot
                    if ( amountLeftToAdd >= ( maxContributionToThisPotByAnyPlayer - myExistingContributionToThisPot ) ) {
                        // Player is calling or raising
                        AddCommentary("Adding "+ amountLeftToAdd +" to pot " + (pot+1) + " (the open pot)");
                        this.Pots[pot][playerIndex] += amountLeftToAdd;
                        break; // Loop would end here anyway as there are no more pots to process, but putting this in for clarity
                    }
                    else {
                        // User is short of the amount required to stay in and is covering the pot
                        if ( amountLeftToAdd > 0 ) {
                            AddCommentary("Adding "+ amountLeftToAdd +" to cover pot " + (pot+1) + " (the open pot)");
                        }
                        this.Pots[pot][playerIndex] += amountLeftToAdd;
                        SplitPotAbovePlayersAmount(pot,  playerIndex); // This will change the pot structure but we'll break out of this loop now so not a problem
                        break; 
                    }                        
                }
                else {
                    // We are currently filling pots that have already been superseded by the open pot
                    int shortfallForThisPot = ( maxContributionToThisPotByAnyPlayer - myExistingContributionToThisPot );
                    if ( shortfallForThisPot == 0 ) {
                        AddCommentary("Player is already fully paid in to pot " + (pot+1) + " (with " + myExistingContributionToThisPot + " chips)");
                    }
                    else if ( amountLeftToAdd >= shortfallForThisPot ) {
                        // Player has at least enough to complete his commitment to this pot
                        AddCommentary("Adding "+ shortfallForThisPot +" to complete commitment to pot " + (pot+1));
                        this.Pots[pot][playerIndex] += shortfallForThisPot;
                        amountLeftToAdd -= shortfallForThisPot;
                        // Stop here if player has nothing left to add
                        if ( amountLeftToAdd == 0 ) {
                            AddCommentary("Player has nothing left to add, all relevant pots successfully covered");
                            break;
                        }                        
                    }
                    else if ( amountLeftToAdd == 0 ) {
                        // Can only get here if someone has raised beyond your all-in amount and you now need to cover 
                        AddCommentary("Covering pot " + (pot+1));
                        SplitPotAbovePlayersAmount(pot,  playerIndex); // This will change the pot structure but we'll break out of this loop now so not a problem
                        break;                         
                    }                    
                    else {
                        // Player does not have enough to complete their contribution to this pot
                        AddCommentary("Adding "+ amountLeftToAdd +" to partially satisfy commitment to pot #" + (pot+1));
                        this.Pots[pot][playerIndex] += amountLeftToAdd;
                        amountLeftToAdd = 0;
                        SplitPotAbovePlayersAmount(pot,  playerIndex); // This will change the pot structure but we'll break out of this loop now so not a problem
                        break; 
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
        public async Task SetNextPlayerToActOrHandleEndOfHand(int currentPlayerIndex, string Trigger) {
            // Check for scenario where only one active player is left
            if ( CountOfPlayersLeftInHand() == 1 ) {
                // Everyone has folded except one player
                NextAction = await ProcessEndOfHand(Trigger + ", only one player left in, hand ended"); // will also update commentary with hand results
                AddCommentary(NextAction);
                return;
            }
            if ( GameMode == GameModeEnum.HandInProgress ) {
                // Find and set next player (could be no one if all players have now called or folded)
                IndexOfParticipantToTakeNextAction = GetIndexOfPlayerToBetNext(currentPlayerIndex);
                if ( IndexOfParticipantToTakeNextAction > -1 ){
                    NextAction = Participants[IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                    AddCommentary(NextAction);
                    return;
                }
                else if ( _CardsDealtIncludingCurrent < 7 ) {
                    DealNextRound();
                    NextAction = "Started next round, " + Participants[IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                    AddCommentary(NextAction);
                    return;
                }
                else  {
                    // All 7 cards have now been bet on, so betting is completed and we now need players to reveal their hands in turn
                    GameMode = GameModeEnum.HandsBeingRevealed;
                    IndexOfParticipantToTakeNextAction  = ( _CheckIsAvailable ? _IndexOfLastPlayerToStartChecking : _IndexOfLastPlayerToRaise );
                    NextAction = /* Trigger + ", " + */ "Betting completed, proceeding to hand reveal stage, " 
                        + Participants[IndexOfParticipantToTakeNextAction].Name + " to reveal (or fold)"; 
                    AddCommentary(NextAction);
                    return;
                }
            }
            if ( GameMode == GameModeEnum.HandsBeingRevealed ) {
                // Find and set next player to reveal their hand (or fold)
                int firstToReveal = ( _CheckIsAvailable ? _IndexOfLastPlayerToStartChecking : _IndexOfLastPlayerToRaise );
                for ( int i = 0; i < Participants.Count; i++ ) {
                    int ZbiOfNextPlayerToInspect = (firstToReveal + i) % Participants.Count;
                    if ( Participants[ZbiOfNextPlayerToInspect].IsSharingHandDetails == false
                        && Participants[ZbiOfNextPlayerToInspect].HasFolded == false // i.e. player has not folded out of this hand
                        && Participants[ZbiOfNextPlayerToInspect].StartedHandWithNoFunds == false // i.e. player has not yet lost all of their funds
                    ) {
                        IndexOfParticipantToTakeNextAction = ZbiOfNextPlayerToInspect;
                        NextAction = /*Trigger + ", " +*/ Participants[IndexOfParticipantToTakeNextAction].Name + " to reveal (or fold)"; 
                        AddCommentary(NextAction);
                        return;
                    }
                }
                // If we get here there were no further people to reveal their cards, so it's time to determine the winner
                await ProcessEndOfHand(Trigger + ", all hands revealed, hand ended"); // will also update commentary with hand results
                AddCommentary(NextAction);
            }
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
                    && Participants[ZbiOfNextPlayerToInspect].StartedHandWithNoFunds == false // i.e. player has not yet lost all of their funds
                ) {
                    return ZbiOfNextPlayerToInspect;
                }
            }
            return -1;
        }

        public int GetIndexOfAdministrator()
        {
            for ( int i = 0; i < Participants.Count; i++ ) {
                if ( Participants[i].IsGameAdministrator ) {
                    return i;
                }
            }
            return -1; // shouldn't be possible as long as we pass admnistratorship on if the current admin leaves the game
        } 
        public async Task<string> ProcessEndOfHand(string Trigger) {
            // Something has triggered the end of the hand. Distribute each pot according to winner(s) of that pot.
            // Start with oldest pot and work forwards. 
            // Only players who have contributed to a pot and have not folded are to be considered
            AddCommentary(Trigger);
            await _GameLog.LogEndOfHand(this, this.SnapshotOfDeckForCurrentHand);

            ClearResultDetails();

            foreach ( Participant p in Participants ) {
                p.GainOrLossInLastHand = 0;
                p.WonSomethingInCurrentHand = false;
            }

            int numberOfPotentialWinners = CountOfPlayersLeftInHand();

            List<int> currentWinners = new List<int>();
            for (int pot = 0; pot < Pots.Count ; pot++) {
                // Identify the player or players who is/are winning this pot
                int winningPlayersHandRank = int.MaxValue; // Low values will win so this is guaranteed to be beaten
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
                        int inPot = ChipsInSpecifiedPotForSpecifiedPlayer(pot, player);
                        p.UncommittedChips += share;
                        p.GainOrLossInLastHand += ( share - inPot );
                        p.WonSomethingInCurrentHand = true;
                        if ( numberOfPotentialWinners == 1 ) {
                            AddCommentaryAndResultDetail(pot, p.Name, inPot, ( share - inPot ), PotResultReasonEnum.NoOneElseLeft, p._FullHandDescription, p.Hand,
                                p.Name + " won " + ( share - inPot ) + " as everyone else folded");
                        }
                        else {
                            AddCommentaryAndResultDetail(pot, p.Name, inPot, ( share - inPot ), PotResultReasonEnum.ViaHandComparisons, p._FullHandDescription, p.Hand,
                                p.Name + " won " + ( share - inPot ) + " with " + p._FullHandDescription);
                        }
                    }
                }
                for (int player = 0; player < Participants.Count ; player++) {
                    Participant p = Participants[player];
                    if ( ! currentWinners.Contains(player) ) {
                        // Record the loss of their investment
                        int inPot = ChipsInSpecifiedPotForSpecifiedPlayer(pot, player);
                        p.GainOrLossInLastHand -= inPot;
                        if ( inPot == 0 ) {
                            AddCommentaryAndResultDetail(pot, p.Name, inPot, 0, PotResultReasonEnum.PlayerWasNotInThisPot, p._FullHandDescription, p.Hand,
                                p.Name + " had nothing in this pot");
                        }
                        else if ( p.HasFolded ) {
                            AddCommentaryAndResultDetail(pot, p.Name, inPot, - inPot, PotResultReasonEnum.PlayerFolded, p._FullHandDescription, p.Hand,
                                p.Name + " lost " + ( inPot ) + " after folding");
                        }
                        else {
                            AddCommentaryAndResultDetail(pot, p.Name, inPot, - inPot, PotResultReasonEnum.ViaHandComparisons, p._FullHandDescription, p.Hand,
                                p.Name + " lost " + ( inPot ) + " with " + p._FullHandDescription);
                        }
                    }
                }                
            }

            ClearRemnantsFromLastGame(); // Ensures that player summaries will not show anything still in the pots

            // Identify anyone who became backrupt during the hand. 
            for (int p = 0; p < Participants.Count ; p++) {
                DateTimeOffset now = DateTimeOffset.Now;
                if ( Participants[p].UncommittedChips == 0 && Participants[p].StartedHandWithNoFunds == false ) {
                    // Player is now bankrupt having been in this hand with some funds at the beginning of the hand
                    Participants[p].TimeOfBankruptcy = now;
                }
            }
            
            // Work through each of the players and note/set various things 
            int unrevealedHands = 0;
            for (int p = 0; p < Participants.Count ; p++) {
                if ( Participants[p].IsSharingHandDetails == false && Participants[p].HasDisconnected == false) {
                    // Check for anyone who has not disconnected and not yet revealed their cards
                    unrevealedHands++;
                }
                if ( Participants[p].WonSomethingInCurrentHand == true) {
                    // Update the player's win count if they won anything in this hand
                    Participants[p].HandsWon++;
                }
            }

            // Clear the 'next player' setting as it's undefined now until the next hand starts
            IndexOfParticipantToTakeNextAction = -1;

            AddCommentary("Waiting for administrator to start next hand.");

            if ( CountOfPlayersLeftInGame() == 1 ) {
                NextAction = "You are the last player in the game, please either reopen the lobby or leave the game";
            }
            else if ( unrevealedHands > 0 ) {
                NextAction = "Reveal hands if desired, or administrator to start next hand (or reopen lobby)";
            }
            else {
                NextAction = "Administrator to start next hand (or reopen lobby)";
            }

            GameMode = GameModeEnum.HandCompleted; 
            return NextAction;
        }

        public int PlayerIndexFromName(string SearchName) {
            for (int player = 0; player < Participants.Count ; player++) {
                if ( Participants[player].Name == SearchName ) {
                    return player;
                }
            }
            return -1;
        }
        public int SpectatorIndexFromName(string SearchName) {
            for (int spectator = 0; spectator < Spectators.Count ; spectator++) {
                if ( Spectators[spectator].Name == SearchName ) {
                    return spectator;
                }
            }
            return -1;
        }
        public void RecordLastEvent(string a){
            LastEvent = a;
            ClearCommentary(); 
            AddCommentary(a);
        }
        public void AddCommentary (string c){
            this.HandCommentary.Add(c);
        }
        public void ClearCommentary (){
            this.HandCommentary.Clear();
        }
        public void AddCommentaryAndResultDetail (int pot, string player, int stake, int gain, PotResultReasonEnum reason, string handDesc, List<Card> hand, string c){
            this.AddCommentary("Pot "+pot+": "+c);
            this.AddResultDetail(pot, player, stake, gain, reason, handDesc, hand, c);
        }
        public void AddResultDetail (int pot, string player, int stake, int gain, PotResultReasonEnum reason, string handDesc, List<Card> hand, string c){
            if ( this.MostRecentHandResult.Count < ( pot + 1 ) ) {
                //this.LastHandResult.Add(new List<string>());
                this.MostRecentHandResult.Add(new List<PotResult>());
            }
            //this.LastHandResult[pot].Add(c);
            this.MostRecentHandResult[pot].Add(new PotResult(player, stake, gain, reason, handDesc, hand, c));
        }
        public void ClearResultDetails () {
            //this.LastHandResult = new List<List<string>>();
            this.MostRecentHandResult = new List<List<PotResult>>();
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

        public void UpdateGameStatistics() {
            this.GameStatistics.UpdateStatistics(this);
        }
        public void TakeSnapshotOfNewDeck(){
            this.SnapshotOfDeckForCurrentHand = this.CardPack.Clone();
        } 

        public async Task LogActionWithResults(Action a) {
            this.ActionNumber++;
            GameLogAction gla = new GameLogAction(
                a, 
                this.ActionNumber,
                this.StatusMessage,
                this.PlayerSummaries(),
                this.HandCommentary
                );
            this._GameLog.actions.Add(gla);
            this.LastSuccessfulAction = DateTimeOffset.Now; 
            // Log the action to the DB
            await ServerState.OurDB.RecordGameLogAction(this, gla);
        }

        public string PlayerSummaries()
        {
            string r = "";
            for ( int i = 0; i < Participants.Count; i++ ) {
                Participant p = Participants[i];
                r += 
                    p.Name.Substring(0,1) 
                    + "["
                    + ( p.StartedHandWithNoFunds ? "O" : "" )
                    + ( p.HasFolded ? "F" : "" )
                    + ( p.HasCovered ? "C" : "" )
                    + ( i == IndexOfParticipantDealingThisHand ? "D" : "" )
                    + ( p.HasDisconnected ? "X" : "" )
                    + "]"+ p.UncommittedChips + PlayersPotContributions(i)
                    + ( p._VisibleHandSummary == null ? "" : p._VisibleHandSummary.Replace(" ","") )
                    + ( i < Participants.Count ? " " : "" )
                ;
            }
            return r;
        }
        public string PlayersPotContributions(int player_no) {
            if ( PotsAreEmpty() ) {
                return "";
            }
            string r = "(";
            for ( int pot_no = 0; pot_no < Pots.Count; pot_no++ ){
                r += Pots[pot_no][player_no];
                r += ( ( pot_no + 1 < Pots.Count ) ? "," : "" );
            }
            r += ")";
            return r;
        }
        public string GameLogAsJson() {
            return this._GameLog.AsJson();
        }
        public int MinutesSinceLastAction() {
            return Convert.ToInt32( (DateTimeOffset.Now - this.LastSuccessfulAction).TotalMinutes);
        }
    }
}