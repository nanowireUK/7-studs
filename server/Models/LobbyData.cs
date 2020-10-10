using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SevenStuds.Models
{
    public class LobbyData
    {
        // Used to present the current status of a room
        public List<string> GameStatistics { get; set; } 
        public List<LobbyDataCurrentGame> CurrentGameResult { get; set; } // Will show players in order from most successful to least successful in this game
        public List<string> ConnectedPlayers { get; set; } 
        public List<string> PreviousGameResults { get; set; } 
        public LobbyData(Game g) {
            this.AddGameStatistics(g);
            this.AddGameResult(g);
            this.AddConnectedPlayers(g);
            this.AddPreviousGameResults(g);
        }
        private void AddGameResult(Game g) {
            CurrentGameResult = g.SortedListOfWinnersAndLosers();
        }
        private void AddGameStatistics(Game g) {
            GameStatistics = new List<string>();
            GameStatistics.Add("Start Time: " + g.StartTime.ToString("HH:mm"));
            GameStatistics.Add("Time Now: " + DateTimeOffset.Now.ToString("HH:mm"));
            GameStatistics.Add("Duration: " + Convert.ToInt32( (DateTimeOffset.Now - g.StartTime).TotalMinutes));
            GameStatistics.Add("Hands Played: " + g.HandsPlayedIncludingCurrent);
            return ;
        }          
        private void AddConnectedPlayers(Game g) {
            ConnectedPlayers = new List<string>();
            // Add all players who are either in the game already or who have joined the lobby
            foreach ( Participant p in g.Participants ) {
                if ( p.HasDisconnected == false ) {
                    ConnectedPlayers.Add(p.Name);
                }
            }
        }  
        private void AddPreviousGameResults(Game g) {
            PreviousGameResults =  (List<string>) ServerState.RoomHistory[g.GameId];
        }      
    }
}