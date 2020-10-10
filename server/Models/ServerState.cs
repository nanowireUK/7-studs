using System;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public static class ServerState
    {
        // Maintains a registry of games which enables a game object to be found from its ID.
        // Also provides other room-level functions (where a room may have hosted a whole series of games)
        public static Hashtable GameList = new Hashtable(); // Maps Room name to current Game object
        public static Hashtable RoomHistory = new Hashtable(); // Map Room name to a history of completed game results
        public static Hashtable RoomGameLogHistory = new Hashtable(); // Map Room name to a history of game logs
        public static PokerHandRankingTable RankingTable = new PokerHandRankingTable(); // Only need one of these
        public static Card DummyCard = new Card(CardEnum.Dummy, SuitEnum.Clubs);
        public static Boolean IsRunningOnPublicServer() {
            string origin_value = Environment.GetEnvironmentVariable("SevenStudsOrigin");
            if ( origin_value == null ) { return false; }
            return ( origin_value == "https://7studsserver.azurewebsites.net/" );
        }
        public static Boolean GameExists(string gameId) {
            return GameList.ContainsKey(gameId);
        }

        public static Game FindOrCreateGame(string gameId) {
            if ( GameList.ContainsKey(gameId) ) {
                Game g =  (Game) GameList[gameId];
                if ( g.MinutesSinceLastAction() <= 120 ) {
                    return g;
                }
                else {
                    // Delete the current version of this game so the new one starts 
                    EraseGame(gameId);
                }
            }
            Game newGame = new Game(gameId);
            GameList.Add(gameId, newGame);
            return newGame;
        }
        public static void EraseGame(string gameId) {
            GameList.Remove(gameId);
        }
        public static void AddCompletedGameToRoomHistory(Game g) {
            if ( RoomHistory.ContainsKey(g.GameId) == false ) {
                // Start a game history for this room
                RoomHistory.Add(g.GameId, new List<string>());
                RoomGameLogHistory.Add(g.GameId, new List<string>());
            }
            List<string> existingResults = (List<string>) RoomHistory[g.GameId];
            existingResults.Insert(0, OneLineSummaryOfResult(g)); // Ensure latest result appears at top
            List<string> existingLogs = (List<string>) RoomGameLogHistory[g.GameId];
            existingLogs.Insert(0, g.GameLogAsJson()); // Ensure latest result appears at top
        }
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
    }
}