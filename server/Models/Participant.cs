using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SocialPokerClub.Models
{
    public class Participant
    {
        public Participant(string PName) {
            this.Name = PName;
            this.RejoinCode = GenerateRejoinCode();
            this.ParticipantSignalRGroupName = PName + '.' + Guid.NewGuid().ToString(); // Unique group id for this player (who may connect)
            //this._ConnectionIds = new List<string>(); // This user's connection ids will be recorded here
            this.Hand = new List<Card>();
            this.IsLockedOutFollowingReplay = false;
            this.IsGameAdministrator = false;
            this.IsSharingHandDetails = false;
            this.HasJustSharedHandDetails = false; // set when the player reveals; will be cleared as soon as anyone else does anything else
            this.HasBeenActiveInCurrentGame = false;
            this.TimeOfBankruptcy = DateTimeOffset.MinValue;
            this.AllInDateTime = DateTimeOffset.MinValue;
            this.IntendsToPlayBlindInNextHand = false;
            this.IsPlayingBlindInCurrentHand = false;
        }
        [Required]

        public string Name { get; set; }
        public int UncommittedChips { get; set; }
        public Boolean HasFolded { get; set; }
        public Boolean HasCovered { get; set; }
        public Boolean StartedHandWithNoFunds { get; set; } // i.e. had no funds at the start of the current hand
        public Boolean HasDisconnected { get; set; } // Player has chosen to leave the game (i.e. is no longer connected and will be removed at end of hand)
        public Boolean IsSharingHandDetails { get; set; }
        public Boolean HasJustSharedHandDetails { get; set; }
        public Boolean WonSomethingInCurrentHand { get; set; }
        public Boolean IsPlayingBlindInCurrentHand { get; set; }
        public Boolean IntendsToPlayBlindInNextHand { get; set; }
        public ActionEnum LastActionInThisHand { get; set; }
        public int LastActionAmount { get; set; }
        public int RoundNumberOfLastAction { get; set; }
        public int HandsWon { get; set; }
        public string RejoinCode { get; set; } // e.g. 3 alphanumeric characters that enables a disconnected player to rejoin as the same person
        public string ParticipantSignalRGroupName { get; set; }
        public Boolean HasBeenActiveInCurrentGame { get; set; }
        public DateTimeOffset TimeOfBankruptcy { get; set; }
        public DateTimeOffset AllInDateTime{ get; set; } // note that this may not be the time they went bankrupt, i.e. they could win the hand
        //private List<string> _ConnectionIds { get; set; }
        [Required]
        public int _VisibleHandRank { get; set; }
        public int _FullHandRank { get; set; }
        public string _VisibleHandDescription { get; set; }
        public string _FullHandDescription { get; set; }
        public string _VisibleHandSummary { get; set; }
        public string _HandSummary { get; set; }
        public int GainOrLossInLastHand { get; set; }
        public List<int> _CardIndexesInPresentationOrder { get; set; }
        public int _HandStrength { get; set; }
        public Boolean IsLockedOutFollowingReplay { get; set; }
        public Boolean IsGameAdministrator { get; set; }
        public List<Card> Hand { get; set; }
        // Note that these two fields are private (and therefore not persisted to the database in stateless mode)
        // They do not need to be persisted/restored because anything that derives from them is persisted in other fields 
        private PokerHand _PokerHand { get; set; }
        private PokerHand _PresentablePokerHand { get; set; }
        // public Boolean IsAllIn() {
        //     return UncommittedChips == 0 & ChipsCommittedToCurrentBettingRound > 0;
        // }
        public string GenerateRejoinCode() {
            string seed = "abcdefghijkmnopqrstuvwxyz023456789"; // no '1' and no 'l' as too easy to mix up
            string code = "";
            Random r = ServerState.ServerLevelRandomNumberGenerator;
            for ( int i = 0; i < 4; i++) {
                code += seed[r.Next(0, seed.Length - i)];
            }
            return code;
        }
        // public List<string> GetConnectionIds() {
        //     return _ConnectionIds;
        // }

        // public void NoteConnectionId(string connectionId) {
        //     if ( ! _ConnectionIds.Contains(connectionId)) {
        //          System.Diagnostics.Debug.WriteLine("Noting connection id '{0}' for player '{1} and group '{2}'", connectionId, this.Name, this.ParticipantSignalRGroupName);
        //         _ConnectionIds.Add(connectionId);
        //     }
        // }

        public void StartNewHandForActivePlayer(Game g) {
            //this.ChipsCommittedToCurrentBettingRound = g.Ante;
            this.UncommittedChips -= g.Ante;
            this.HasFolded = false;
            this.HasCovered = false;
            this.StartedHandWithNoFunds = false;
            this.IsPlayingBlindInCurrentHand = this.IntendsToPlayBlindInNextHand; // intent was noted via action in lobby or during last hand
            this.IntendsToPlayBlindInNextHand = false; // clear the intent for the current hand
            this.Hand = new List<Card>();
            this.Hand.Add(g.DealCard()); // 1st random card
            this.Hand.Add(g.DealCard()); // 2nd random card
            this.Hand.Add(g.DealCard()); // 3rd random card
            PokerHand visibleHand = new PokerHand(
                this.Hand[2],
                ServerState.DummyCard,
                ServerState.DummyCard,
                ServerState.DummyCard,
                ServerState.DummyCard, ServerState.RankingTable);
            this._VisibleHandDescription = /*visibleHand.ToString(HandToStringFormatEnum.ShortCardsHeld) + ": " + */ visibleHand.ToString(HandToStringFormatEnum.HandDescription);
            this._VisibleHandRank = visibleHand.Rank;
            _PokerHand = new PokerHand(
                this.Hand[0],
                this.Hand[1],
                this.Hand[2],
                ServerState.DummyCard,
                ServerState.DummyCard, ServerState.RankingTable);
            this._FullHandDescription = /*_PokerHand.ToString(HandToStringFormatEnum.ShortCardsHeld) + ": " + */ _PokerHand.ToString(HandToStringFormatEnum.HandDescription);
            this._FullHandRank = _PokerHand.Rank;
            RebuildMyHandSummary(g);
        }

        public void StartNewHandForBankruptPlayer(Game g) {
            //this.ChipsCommittedToCurrentBettingRound = 0;
            this.HasFolded = false;
            this.HasCovered = false;
            this.StartedHandWithNoFunds = true;
            this.Hand = new List<Card>();
            this._VisibleHandDescription = null;
            this._VisibleHandRank = int.MaxValue;
            this._FullHandDescription = null;
            this._FullHandRank = int.MaxValue;
            this._HandSummary = "";
            this._VisibleHandSummary = "";
        }

        public void PrepareForNextBettingRound(Game g, int roundNumber) {
            // Check whether player is still in, and deal them a new card if so (or the community card if specified)
            if ( this.HasFolded == false & this.StartedHandWithNoFunds == false ) {
                if ( g.CommunityCard == null ) {
                    this.Hand.Add(g.DealCard()); // random card, usual scenario
                }
                else {
                    this.Hand.Add(g.CommunityCard); // same card for each player in this round
                }

                PokerHand visibleHand = new PokerHand(
                    this.Hand[2],
                    roundNumber >= 4 ? this.Hand[3] : ServerState.DummyCard,
                    roundNumber >= 5 ? this.Hand[4] : ServerState.DummyCard,
                    roundNumber >= 6 ? this.Hand[5] : ServerState.DummyCard,
                    roundNumber == 7 && g.CardPositionIsVisible[6] == true ? this.Hand[6] : ServerState.DummyCard, // final card can be open if we're in a community card situation
                    ServerState.RankingTable);
                this._VisibleHandDescription = /*visibleHand.ToString(HandToStringFormatEnum.ShortCardsHeld) + ": " + */ visibleHand.ToString(HandToStringFormatEnum.HandDescription);
                this._VisibleHandRank = visibleHand.Rank;
                this._CardIndexesInPresentationOrder = null;

                if ( g._CardsDealtIncludingCurrent < 6 ) {
                    _PokerHand = new PokerHand(
                        this.Hand[0],
                        this.Hand[1],
                        this.Hand[2],
                        roundNumber >= 4 ? this.Hand[3] : ServerState.DummyCard,
                        roundNumber >= 5 ? this.Hand[4] : ServerState.DummyCard,
                        ServerState.RankingTable);
                }
                else if ( g._CardsDealtIncludingCurrent == 6 ) {
                    List<int> chosenCombo = null;
                    _PokerHand = null;
                    List<List<int>> combos = new List<List<int>>();
                    combos.Add(new List<int>(){0, 1, 2, 3, 4});
                    combos.Add(new List<int>(){0, 1, 2, 3, 5});
                    combos.Add(new List<int>(){0, 1, 2, 4, 5});
                    combos.Add(new List<int>(){0, 1, 3, 4, 5});
                    combos.Add(new List<int>(){0, 2, 3, 4, 5});
                    combos.Add(new List<int>(){1, 2, 3, 4, 5});
                    PokerHand testHand;
                    bool initDone = false;
                    for (int i = 0; i < combos.Count ; i++) {
                        testHand = new PokerHand(
                            this.Hand[combos[i][0]],
                            this.Hand[combos[i][1]],
                            this.Hand[combos[i][2]],
                            this.Hand[combos[i][3]],
                            this.Hand[combos[i][4]],
                            ServerState.RankingTable);
                        // Note current best hand and which card positions were used to form that hand
                        if ( initDone == false ) {
                            _PokerHand = testHand;
                            chosenCombo = combos[i];
                            initDone = true;
                        }
                        else if ( testHand.Rank < _PokerHand.Rank) {
                            _PokerHand = testHand;
                            chosenCombo = combos[i];
                        }
                    }
                    _CardIndexesInPresentationOrder = GetCardIndexesInPresentationOrder(chosenCombo);
                    _HandStrength = _PokerHand.Strength();
                }
                else {
                    // All 7 have been dealt
                    List<int> chosenCombo = null;
                    _PokerHand = null;
                    List<List<int>> combos = new List<List<int>>();
                    combos.Add(new List<int>(){0, 1, 2, 3, 4});
                    combos.Add(new List<int>(){0, 1, 2, 3, 5});
                    combos.Add(new List<int>(){0, 1, 2, 3, 6});
                    combos.Add(new List<int>(){0, 1, 2, 4, 5});
                    combos.Add(new List<int>(){0, 1, 2, 4, 6});
                    combos.Add(new List<int>(){0, 1, 2, 5, 6});
                    combos.Add(new List<int>(){0, 1, 3, 4, 5});
                    combos.Add(new List<int>(){0, 1, 3, 4, 6});
                    combos.Add(new List<int>(){0, 1, 3, 5, 6});
                    combos.Add(new List<int>(){0, 1, 4, 5, 6});
                    combos.Add(new List<int>(){0, 2, 3, 4, 5});
                    combos.Add(new List<int>(){0, 2, 3, 4, 6});
                    combos.Add(new List<int>(){0, 2, 3, 5, 6});
                    combos.Add(new List<int>(){0, 2, 4, 5, 6});
                    combos.Add(new List<int>(){0, 3, 4, 5, 6});
                    combos.Add(new List<int>(){1, 2, 3, 4, 5});
                    combos.Add(new List<int>(){1, 2, 3, 4, 6});
                    combos.Add(new List<int>(){1, 2, 3, 5, 6});
                    combos.Add(new List<int>(){1, 2, 4, 5, 6});
                    combos.Add(new List<int>(){1, 3, 4, 5, 6});
                    combos.Add(new List<int>(){2, 3, 4, 5, 6});
                    PokerHand testHand;
                    bool initDone = false;
                    for (int i = 0; i < combos.Count ; i++) {
                        testHand = new PokerHand(
                            this.Hand[combos[i][0]],
                            this.Hand[combos[i][1]],
                            this.Hand[combos[i][2]],
                            this.Hand[combos[i][3]],
                            this.Hand[combos[i][4]],
                            ServerState.RankingTable);
                        // Note current best hand and which card positions were used to form that hand
                        if ( initDone == false ) {
                            _PokerHand = testHand;
                            chosenCombo = combos[i];
                            initDone = true;
                        }
                        else if ( testHand.Rank < _PokerHand.Rank) {
                            _PokerHand = testHand;
                            chosenCombo = combos[i];
                        }
                    }
                    _CardIndexesInPresentationOrder = GetCardIndexesInPresentationOrder(chosenCombo);
                    _HandStrength = _PokerHand.Strength();
                }
                this._FullHandDescription = /*_PokerHand.ToString(HandToStringFormatEnum.ShortCardsHeld) + ": " + */ _PokerHand.ToString(HandToStringFormatEnum.HandDescription);
                this._FullHandRank = _PokerHand.Rank;
                RebuildMyHandSummary(g);
            }
        }
        public void RebuildMyHandSummary(Game g) {
            this._HandSummary = "";
            this._VisibleHandSummary = "";
            for ( int ci = 0; ci < this.Hand.Count; ci++ ) {
                Card c = this.Hand[ci];
                string cardCode = c.ToString(CardToStringFormatEnum.ShortCardName);
                this._HandSummary += cardCode + " ";
                if ( g.CardPositionIsVisible[ci] == true ) {
                    // Could hide folded cards here too but decided not too (as this data is not exposed to the clients anyway)
                    this._VisibleHandSummary += cardCode + ( ci+1 < this.Hand.Count ? " " : "");
                }
            }
            if ( this.IsPlayingBlindInCurrentHand ) {
                this._HandSummary = ""; // Clear the hand summary (not sure it is used in the client anyway)
            }
        }
        private List<int> GetCardIndexesInPresentationOrder(List<int> chosenCombo) {
            // Will use attributes Hand and _PokerHand
            // Note that chosenCombo contains card position indexers that are zero-based
            // Assumes all 3 arrays are the same length (5)
            List<int> presentationOrder = this._PokerHand.PresentationOrder(); // e.g. 1,4,5,3,2 (says what order to change a sorted card list to)
            List<int> cardValues = this._PokerHand.CardValues();

            // First sort chosenCombo in the same way as you would have to do to get the card values in descending order
            while (true)
            {
                bool swapped = false;
                for ( int i = 0; i < cardValues.Count - 1; i++ ) {
                    if (cardValues[i] < cardValues[i + 1]) // Use '<' for descending, '>' for ascending
                    {
                        // Swap the values in the list being sorted
                        int tmp = cardValues[i];
                        cardValues[i] = cardValues[i + 1];
                        cardValues[i + 1] = tmp;

                        // Swap the corresponding values in the list of card positions
                        tmp = chosenCombo[i];
                        chosenCombo[i] = chosenCombo[i + 1];
                        chosenCombo[i + 1] = tmp;

                        swapped = true;
                    }
                }
                if (!swapped) {
                    break;
                }
            }
            // Now further reorder chosenCombo to reflect the presentation order of the hand
            List<int> chosenComboInPresentationOrder = new List<int>();
            for ( int pos = 0; pos < 5; pos++ ) {
                int requiredOrder = presentationOrder[pos]-1; // Gets the index of the card to be used in position 'pos'
                chosenComboInPresentationOrder.Add(chosenCombo[requiredOrder]);
            }
            _PresentablePokerHand = new PokerHand(
                this.Hand[chosenComboInPresentationOrder[0]],
                this.Hand[chosenComboInPresentationOrder[1]],
                this.Hand[chosenComboInPresentationOrder[2]],
                this.Hand[chosenComboInPresentationOrder[3]],
                this.Hand[chosenComboInPresentationOrder[4]],
                ServerState.RankingTable);
            if ( _PresentablePokerHand.Rank != _PokerHand.Rank ) {
                throw new Exception("Poker hand in presentation order is not equivalent to unsorted hand");
            }
            return chosenComboInPresentationOrder; // This is the original card indexes
        }
        public int GetRankingOfThirdCard()
        {
            // Return a low-to-high integer representing the relative order of the cards 2c, 2d, 2h, 2s, 3c, 3d, etc.
            int valueRanking = (int) this.Hand[2].CardValue; // prime number from 1 to 41
            int suitRanking = 0;
            if ( this.Hand[2].CardSuit == SuitEnum.Clubs )    { suitRanking = 1; }
            else if ( this.Hand[2].CardSuit == SuitEnum.Diamonds ) { suitRanking = 2; }
            else if ( this.Hand[2].CardSuit == SuitEnum.Hearts )   { suitRanking = 3; }
            else if ( this.Hand[2].CardSuit == SuitEnum.Spades )   { suitRanking = 4; }
            // System.Diagnostics.Debug.WriteLine(this.Name + " 3rd card is " + this.Hand[2].ToString(CardToStringFormatEnum.ShortCardName)
            //     + ", value ranked as " + valueRanking + ", suit ranked as " + suitRanking + ", " + (( valueRanking * 10 ) + suitRanking) + " overall");
            return ( valueRanking * 10 ) + suitRanking;
        }
    }
}