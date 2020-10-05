using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SevenStuds.Models
{
    public class LobbyDataCurrentGame
    {
        // Used to present the current status of a game 
        public string PlayerName { get; set; } 
        public int RemainingFunds { get; set; } 
        public DateTimeOffset EventTimeAsUTC { get; set; } 

        public LobbyDataCurrentGame(string argPlayerName, int argRemainingFunds, DateTimeOffset argUTCTimeOfBankruptcy) {
            this.PlayerName = argPlayerName;
            this.RemainingFunds = argRemainingFunds;
            this.EventTimeAsUTC = argUTCTimeOfBankruptcy;
        }
    }
}