using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class Participant
    {
        public Participant(string PName) {
            this.Name = PName;
            this.RejoinCode = GenerateRejoinCode();
            this.ParticipantLevelSignalRGroupName = PName + '.' + Guid.NewGuid().ToString(); // Unique group id for this player (who may connect)
            this._ConnectionIds = new List<string>(); // This user's connection ids will be recorded here
            this.Hand = new List<Card>();
            this.IsLockedOutFollowingReplay = false;
            this.IsGameAdministrator = false;
            this.IsSharingHandDetails = false;
            this.HasBeenActiveInCurrentGame = false;
        }
        [Required]
        
        public string Name { get; set; }
        public int UncommittedChips { get; set; }
        public Boolean HasFolded { get; set; }
        public Boolean HasCovered { get; set; }
        public Boolean StartedHandWithNoFunds { get; set; } // i.e. had no funds at the start of the current hand
        public Boolean HasDisconnected { get; set; } // Player has chosen to leave the game (i.e. is no longer connected and will be removed at end of hand)
        public Boolean IsSharingHandDetails { get; set; }
        public Boolean WonSomethingInCurrentHand { get; set; }
        public ActionEnum LastActionInThisHand { get; set; }
        public int LastActionAmount { get; set; }
        public int RoundNumberOfLastAction { get; set; }
        public int HandsWon { get; set; } 
        public string RejoinCode { get; set; } // e.g. 3 alphanumeric characters that enables a disconnected player to rejoin as the same person
        public string ParticipantLevelSignalRGroupName { get; set; }
        public Boolean HasBeenActiveInCurrentGame { get; set; }
        public DateTimeOffset TimeOfBankruptcy { get; set; }
        private List<string> _ConnectionIds { get; set; }
                
        [Required]
        public int _VisibleHandRank { get; set; }
        public int _FullHandRank { get; set; }
        public string _VisibleHandDescription { get; set; }
        public string _FullHandDescription { get; set; }
        public string _VisibleHandSummary { get; set; }
        public string _HandSummary { get; set; }
        public int GainOrLossInLastHand { get; set; }
        private PokerHand _PokerHand { get; set; }
        public Boolean IsLockedOutFollowingReplay { get; set; }
        public Boolean IsGameAdministrator { get; set; }
        public List<Card> Hand { get; set; }
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
        public List<string> GetConnectionIds() {
            return _ConnectionIds;
        }

        public void NoteConnectionId(string connectionId) {
            if ( ! _ConnectionIds.Contains(connectionId)) {
                _ConnectionIds.Add(connectionId);
            }
        }
        
        public void StartNewHandForActivePlayer(Game g) {
            //this.ChipsCommittedToCurrentBettingRound = g.Ante;
            this.UncommittedChips -= g.Ante;
            this.HasFolded = false;
            this.HasCovered = false;
            this.StartedHandWithNoFunds = false;
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
            this._HandSummary = "";
            this._VisibleHandSummary = "";
            for ( int ci = 0; ci < this.Hand.Count; ci++ ) {
                Card c = this.Hand[ci];
                string cardCode = c.ToString(CardToStringFormatEnum.ShortCardName);
                this._HandSummary += cardCode + " ";
                if ( g.CardPositionIsVisible[ci] == true ) {
                    this._VisibleHandSummary += cardCode + ( ci+1 < this.Hand.Count ? " " : "");
                }
            }
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
                    ServerState.DummyCard, // never more than 4 visible
                    ServerState.RankingTable);
                this._VisibleHandDescription = /*visibleHand.ToString(HandToStringFormatEnum.ShortCardsHeld) + ": " + */ visibleHand.ToString(HandToStringFormatEnum.HandDescription);
                this._VisibleHandRank = visibleHand.Rank;

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
                    _PokerHand = new PokerHand( // Start off assuming first combination is best
                        this.Hand[0], 
                        this.Hand[1], 
                        this.Hand[2], 
                        this.Hand[3], 
                        this.Hand[4], 
                        ServerState.RankingTable);
                    List<List<int>> combos = new List<List<int>>();
                    //combos.Add(new List<int>(){0, 1, 2, 3, 4}); // already done
                    combos.Add(new List<int>(){0, 1, 2, 3, 5});
                    combos.Add(new List<int>(){0, 1, 2, 4, 5});
                    combos.Add(new List<int>(){0, 1, 3, 4, 5});
                    combos.Add(new List<int>(){0, 2, 3, 4, 5});
                    combos.Add(new List<int>(){1, 2, 3, 4, 5});
                    PokerHand testHand;
                    for (int i = 0; i < combos.Count ; i++) {
                        testHand = new PokerHand(
                            this.Hand[combos[i][0]], 
                            this.Hand[combos[i][1]], 
                            this.Hand[combos[i][2]], 
                            this.Hand[combos[i][3]], 
                            this.Hand[combos[i][4]], 
                            ServerState.RankingTable);                        
                        if ( testHand.Rank < _PokerHand.Rank) {
                            _PokerHand = testHand;
                        }
                    }
                }
                else {
                    // All 7 have been dealt
                    _PokerHand = new PokerHand( // Start off assuming first combination is best
                        this.Hand[0], 
                        this.Hand[1], 
                        this.Hand[2], 
                        this.Hand[3], 
                        this.Hand[4], 
                        ServerState.RankingTable);
                    List<List<int>> combos = new List<List<int>>();
                    //combos.Add(new List<int>(){0, 1, 2, 3, 4}); // already done
                    combos.Add(new List<int>(){0, 1, 2, 3, 5});
                    combos.Add(new List<int>(){0, 1, 2, 3, 6});
                    combos.Add(new List<int>(){0, 1, 2, 4, 5});
                    combos.Add(new List<int>(){0, 1, 2, 4, 6});
                    combos.Add(new List<int>(){0, 1, 3, 4, 5});
                    combos.Add(new List<int>(){0, 1, 3, 4, 6});
                    combos.Add(new List<int>(){0, 1, 2, 5, 6});
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
                    for (int i = 0; i < combos.Count ; i++) {
                        testHand = new PokerHand(
                            this.Hand[combos[i][0]], 
                            this.Hand[combos[i][1]], 
                            this.Hand[combos[i][2]], 
                            this.Hand[combos[i][3]], 
                            this.Hand[combos[i][4]], 
                            ServerState.RankingTable);                        
                        if ( testHand.Rank < _PokerHand.Rank) {
                            _PokerHand = testHand;
                        }
                    }
                }                
                this._FullHandDescription = /*_PokerHand.ToString(HandToStringFormatEnum.ShortCardsHeld) + ": " + */ _PokerHand.ToString(HandToStringFormatEnum.HandDescription);
                this._FullHandRank = _PokerHand.Rank;
                this._HandSummary = "";
                this._VisibleHandSummary = "";
                for ( int ci = 0; ci < this.Hand.Count; ci++ ) {
                    Card c = this.Hand[ci];
                    string cardCode = c.ToString(CardToStringFormatEnum.ShortCardName);
                    this._HandSummary += cardCode + " ";
                    if ( g.CardPositionIsVisible[ci] == true ) {
                        this._VisibleHandSummary += cardCode + ( ci+1 < this.Hand.Count ? " " : "");
                    }
                }
            }
        }              
    }
}