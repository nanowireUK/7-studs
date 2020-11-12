using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class PotResult
    {
        public string PlayerName { get; set; }
        public int AmountWonOrLost { get; set; } // PlayerFolded, ViaHandComparisons, NoOneElseLeft
        public PotResultReasonEnum ReasonWhy { get; set; }
        public string HandDescription { get; set; }
        public List<Card> Hand { get; set; }
        public PotResult (string player, int gain, PotResultReasonEnum reason, string handDesc, List<Card> hand) {
            this.PlayerName = player;
            this.AmountWonOrLost = gain;
            this.ReasonWhy = reason;
            this.HandDescription = handDesc;
            this.Hand = hand;
        }
    }
}