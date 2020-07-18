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
        public string ConnectionId { get; set; } // e.g. 3 alphanumeric characters that enables a disconnected player to rejoin as the same person
        [Required]
        public string _VisibleHandDescription { get; set; }
        public int _VisibleHandRank { get; set; }
        public string _FullHandDescription { get; set; }
        public int _FullHandRank { get; set; }
        public int UncommittedChips { get; set; }
        public int ChipsCommittedToCurrentBettingRound { get; set; }
        public Boolean HasFolded { get; set; }
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
    }
}