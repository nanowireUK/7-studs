using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class Participant
    {
        public Participant(string PName, string connectionId) {
            this.Name = PName;
            this.ConnectionId = connectionId;
        }
        [Required]
        
        public string Name { get; set; }
        public int UncommittedChips { get; set; }
        public int ChipsCommittedToCurrentBettingRound { get; set; }
        public Boolean HasFolded { get; set; }
        public string ConnectionId { get; set; } // e.g. 3 alphanumeric characters that enables a disconnected player to rejoin as the same person
        [Required]
        public int _VisibleHandRank { get; set; }
        public int _FullHandRank { get; set; }
        public string _VisibleHandDescription { get; set; }
        public string _FullHandDescription { get; set; }
        public string _HandSummary { get; set; }
        public List<Card> Hand { get; set; }
        public Boolean IsAllIn() {
            return UncommittedChips == 0 & ChipsCommittedToCurrentBettingRound > 0;
        }
        
        public void StartNewHand(Game g) {
            this.ChipsCommittedToCurrentBettingRound = g.Ante;
            this.UncommittedChips = g.InitialChipQuantity - g.Ante;
            this.HasFolded = false;
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
            this._VisibleHandDescription = visibleHand.ToString(HandToStringFormatEnum.ShortCardsHeld) + ": " + visibleHand.ToString(HandToStringFormatEnum.HandDescription);
            this._VisibleHandRank = visibleHand.Rank;
            PokerHand fullHand = new PokerHand(
                this.Hand[0], 
                this.Hand[1], 
                this.Hand[2], 
                ServerState.DummyCard, 
                ServerState.DummyCard, ServerState.RankingTable);
            this._FullHandDescription = fullHand.ToString(HandToStringFormatEnum.ShortCardsHeld) + ": " + fullHand.ToString(HandToStringFormatEnum.HandDescription);
            this._FullHandRank = fullHand.Rank;  
        }  

        public void PrepareForNextBettingRound(Game g, int roundNumber) {
            // Check whether player is still in, and deal them a new card if so
            if ( this.HasFolded == false ) {
                this.Hand.Add(g.DealCard()); // random card
                PokerHand visibleHand = new PokerHand(
                    this.Hand[2], 
                    roundNumber >= 4 ? this.Hand[3] : ServerState.DummyCard, 
                    roundNumber >= 5 ? this.Hand[4] : ServerState.DummyCard, 
                    roundNumber >= 6 ? this.Hand[5] : ServerState.DummyCard, 
                    ServerState.DummyCard, // never more than 4 visible
                    ServerState.RankingTable);
                this._VisibleHandDescription = visibleHand.ToString(HandToStringFormatEnum.ShortCardsHeld) + ": " + visibleHand.ToString(HandToStringFormatEnum.HandDescription);
                this._VisibleHandRank = visibleHand.Rank;

                PokerHand fullHand;
                if ( g._CardsDealtIncludingCurrent < 6 ) {
                    fullHand = new PokerHand(
                        this.Hand[0], 
                        this.Hand[1], 
                        this.Hand[2], 
                        roundNumber >= 4 ? this.Hand[3] : ServerState.DummyCard, 
                        roundNumber >= 5 ? this.Hand[4] : ServerState.DummyCard, 
                        ServerState.RankingTable);
                }
                else if ( g._CardsDealtIncludingCurrent == 6 ) {
                    fullHand = new PokerHand( // Start off assuming first combination is best
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
                        if ( testHand.Rank < fullHand.Rank) {
                            fullHand = testHand;
                        }
                    }
                }
                else {
                    // All 7 have been dealt
                    fullHand = new PokerHand( // Start off assuming first combination is best
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
                        if ( testHand.Rank < fullHand.Rank) {
                            fullHand = testHand;
                        }
                    }
                }                
                this._FullHandDescription = fullHand.ToString(HandToStringFormatEnum.ShortCardsHeld) + ": " + fullHand.ToString(HandToStringFormatEnum.HandDescription);
                this._FullHandRank = fullHand.Rank;
                this._HandSummary = "";
                foreach ( Card c in this.Hand) {
                    // Is it worth sorting this?
                    this._HandSummary += c.ToString(CardToStringFormatEnum.ShortCardName) + " ";
                }
            }
        }              
    }
}