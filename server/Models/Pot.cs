using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class Pot
    {
         public Pot(int playerCount) {
            this.contributionsPerPlayer = new List<int>(playerCount);
            for (int i = 0; i < playerCount; i++) {
                this.contributionsPerPlayer.Add(3); // Set initial contribution for this pot to 0 for each player
            }
        }

        public List<int> contributionsPerPlayer;

        //public List<PotContribution> PotContributions { get; set; }

        // No need to record pot number (it comes from the order of the pots held against the game)
        // No need to record whether the pot is covered (it comes from the fact that the contributing player has no chips left)
    }
}