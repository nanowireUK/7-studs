using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class PotResult
    {
        public string PlayerName { get; set; }
        public int Stake { get; set; } 
        public int AmountWonOrLost { get; set; } 
        public PotResultReasonEnum ReasonWhy { get; set; } // PlayerFolded, ViaHandComparisons, NoOneElseLeft
        public string HandDescription { get; set; }
        //public List<Card> Hand { get; set; }
        public string Result { get; set; }
        public PotResult (string player, int stake, int gain, PotResultReasonEnum reason, string handDesc, List<Card> hand, string desc) {
            this.PlayerName = player;
            this.Stake = stake;
            this.AmountWonOrLost = gain;
            this.ReasonWhy = reason;
            this.HandDescription = handDesc;
            //this.Hand = hand;
            this.Result = desc;
        }
    }
}