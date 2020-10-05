using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SevenStuds.Models
{
    public class LobbyData
    {
        // Used to present the current status of a game 
        public List<LobbyDataCurrentGame> currentGameResult { get; set; } // Will show players in order from most successful to least successful in this game
        public List<string> connectedPlayers { get; set; } 
        public LobbyData(Game g) {
            this.AddGameResult(g);
            this.AddConnectedPlayers(g);
        }
        private void AddGameResult(Game g) {
            DateTimeOffset now = DateTimeOffset.Now;
            currentGameResult = new List<LobbyDataCurrentGame>();
            // First add all players who are still in the game, without worrying about the order as the list will be sorted later
            foreach ( Participant p in g.Participants ) {
                if ( p.UncommittedChips > 0 ) {
                    this.currentGameResult.Add(new LobbyDataCurrentGame(p.Name, p.UncommittedChips, now));
                }
            }
            // Now add the details of the bankruptcies
            if ( g.BankruptcyEventHistoryForGame != null ) {
                foreach ( BankruptcyEvent e in g.BankruptcyEventHistoryForGame ) {
                    this.currentGameResult.Add(new LobbyDataCurrentGame(e.BankruptPlayerName, 0, e.OccurredAt_UTC));
                } 
            }           
            // Now sort the array by (1) funds descending, (2) date they went backrupt, descending and finally (3) by name
            currentGameResult.Sort(
                delegate(LobbyDataCurrentGame x, LobbyDataCurrentGame y) 
                {
                    // See https://www.codeproject.com/Tips/761275/How-to-Sort-a-List

                    // Sort by total funds in descending order
                    int a = y.RemainingFunds.CompareTo(x.RemainingFunds);
                    // If both players have same funds remaining (which might be zero) then sort by the time they went bankrupt
                    if (a == 0)
                        a = y.EventTimeAsUTC.CompareTo(x.EventTimeAsUTC);
                    // If both players 
                    if (a == 0)
                        a = x.PlayerName.CompareTo(y.PlayerName);                    

                    return a;
                }
            );
        }

       private void AddConnectedPlayers(Game g) {
            connectedPlayers = new List<string>();
            // Add all players who are either in the game already or who have joined the lobby
            foreach ( Participant p in g.Participants ) {
                if ( p.HasDisconnected == false ) {
                    this.connectedPlayers.Add(p.Name);
                }
            }
        }        
        // public string AsJson()
        // {
        //     var options = new JsonSerializerOptions
        //     {
        //         WriteIndented = true,
        //     };
        //     //options.Converters.Add(new JsonStringEnumConverter(null /*JsonNamingPolicy.CamelCase*/));
        //     string jsonString = JsonSerializer.Serialize(this, options);
        //     return jsonString;
        // }  
    }
}