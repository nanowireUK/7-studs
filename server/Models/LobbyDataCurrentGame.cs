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
        public PlayerStatusEnum Status { get; set; } 
        public int RemainingFunds { get; set; } 
        public Boolean HasLeftRoom { get; set; } // Could be a player who was in the game or could even be a new joiner who has left again
        public DateTimeOffset UTCTimeAsTieBreaker { get; set; } 
        public int DateComparisonModifier { get; set; } 
        public int PriorityOrderForLobbyData { get; set; } 
        public int LeaderBoardPosition { get; set; }
        public Boolean LeaderBoardPositionIsTied { get; set; }
        public LobbyDataCurrentGame (
            string argPlayerName, 
            PlayerStatusEnum argStatus,
            int argRemainingFunds, 
            Boolean argHasLeftRoom, 
            DateTimeOffset argUTCTimeAsTieBreaker
        ) 
        {
            this.PlayerName = argPlayerName;
            this.Status = argStatus;
            this.RemainingFunds = argRemainingFunds;
            this.HasLeftRoom = argHasLeftRoom;
            this.UTCTimeAsTieBreaker = argUTCTimeAsTieBreaker;
            this.LeaderBoardPosition = Int32.MaxValue; // 1 = winner, 2/3/4etc. = places, MaxInt = not in game
            this.LeaderBoardPositionIsTied = false; 

            // This is all so ugly but at least it's mostly in one place :-)

            // (1) Set the priority order for showing the players in the list
            // (2) Set the date sort order to 1 to have dates sort most recent first, or -1 to have dates sort oldest first
            if ( this.Status == PlayerStatusEnum.PartOfMostRecentGame )                                 { PriorityOrderForLobbyData = 5; DateComparisonModifier = 1; }
            else if ( this.Status == PlayerStatusEnum.QueuingForNextGame && this.HasLeftRoom == false ) { PriorityOrderForLobbyData = 4; DateComparisonModifier = -1; }
            else if ( this.Status == PlayerStatusEnum.Spectator && this.HasLeftRoom == false )          { PriorityOrderForLobbyData = 3; DateComparisonModifier = -1; }
            else if ( this.Status == PlayerStatusEnum.QueuingForNextGame && this.HasLeftRoom == true )  { PriorityOrderForLobbyData = 2; DateComparisonModifier = -1; }
            else                                                                                        { PriorityOrderForLobbyData = 1; DateComparisonModifier = -1; }
        }
    }
}