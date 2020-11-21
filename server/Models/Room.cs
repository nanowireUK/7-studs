using System.Collections.Generic;
using System.Collections;
using System;

namespace SevenStuds.Models
{
    public class Room
    {
        public string RoomId { get; set; }
        public Hashtable GameList = new Hashtable(); // Maps Room name to current Game object
        public Hashtable RoomHistory = new Hashtable(); // Map Room name to a history of completed game results
        //public List<List<string>> RoomGameLogHistory = new List<List<string>>(); // Map Room name to a history of game logs
        public Game ActiveGame { get; set; }
        public List<GameLog> GameLogs { get; set; }
        public LobbyData LobbyData { get; set; }
        protected List<User> RegisteredUsers { get; set; }
        public string AdHocQueryType { get; set; }
        public Room(string roomId) {
            RoomId = roomId;
            RegisteredUsers = new List<User>();
            ActiveGame = null;
        }
        // public void AddCompletedGameToRoomHistory(Game g) {
        //     if ( RoomHistory.ContainsKey(g.GameId) == false ) {
        //         // Start a game history for this room
        //         RoomHistory.Add(g.GameId, new List<string>());
        //         RoomGameLogHistory.Add(g.GameId, new List<string>());
        //     }
        //     List<string> existingResults = (List<string>) RoomHistory[g.GameId];
        //     existingResults.Insert(0, OneLineSummaryOfResult(g)); // Ensure latest result appears at top
        //     List<string> existingLogs = (List<string>) RoomGameLogHistory[g.GameId];
        //     existingLogs.Insert(0, g.GameLogAsJson()); // Ensure latest result appears at top
        // }
        private static string OneLineSummaryOfResult(Game g) {
            // Return a one-liner that summarises the results in order of winning player to losing player
            DateTimeOffset now = DateTimeOffset.Now; // This is the completion date/time of the game
            string gameResult = "Ended " + now.ToString("HH:mm");
            List<LobbyDataCurrentGame> w = g.SortedListOfWinnersAndLosers();
            for ( int r = 0; r < w.Count; r++ ) {
                int f = w[r].RemainingFunds;
                string pref = ( r==0 ? " " : ", " );
                gameResult += pref + (r+1) + ":" + w[r].PlayerName + "(" + f + ")";
            }
            return gameResult;
        }
        public List<string> AdHocQueryResult() {
            // This is stupidly convoluted, but an ActionAdHocQuery command has recorded a command name in AdHocQueryType,
            // and we will now use a separate class to action the query and return 
            AdHocQuery q = new AdHocQuery(this, AdHocQueryType);
            return q.queryResults;
        }
    }
}