using System;
using System.Timers;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialPokerClub.Models
{
    public static class ServerState
    {
        // Maintains a registry of games which enables a game object to be found from its ID.
        // Also provides other room-level functions (where a room may have hosted a whole series of games)

        public static Hashtable RoomList; // Server-level list of Rooms
        public static StatefulGameData StatefulData; // Server-level list of connections
        public static PokerHandRankingTable RankingTable; // Only need one of these
        public static Card DummyCard;
        public static PokerDB OurDB;
        public static MetricsSummary MetricsSummary;
        public static System.Timers.Timer MonitorTimer;
        public static MetricsManager MetricsManager;
        public static int TotalActionsProcessed = 0;
        static ServerState() {
            // Static constructor, runs initialisations in the order we require
            System.Diagnostics.Debug.WriteLine("ServerState static constructor running at at {0:HH:mm:ss.fff}", DateTimeOffset.UtcNow);
            RoomList = new Hashtable(); // Server-level list of Rooms
            StatefulData = new StatefulGameData(); // Server-level list of connections
            RankingTable = new PokerHandRankingTable(); // Only need one of these
            DummyCard = new Card(CardEnum.Dummy, SuitEnum.Clubs);
            OurDB = new PokerDB();
            MetricsSummary = new MetricsSummary(); // Just an initial 'zero' summary, but still needs OurDB to have been instantiated
            MetricsManager = new MetricsManager(); // Needs MetricsSummary to have been instantiated. This object will maintain and emit statistics
            System.Diagnostics.Debug.WriteLine("Initalising MonitorTimer");
            while ( DateTimeOffset.UtcNow.Millisecond < 100 || DateTimeOffset.UtcNow.Millisecond > 900 ) {
                System.Diagnostics.Debug.WriteLine("Waiting 200 milliseconds to ensure repeat timer does not fire close to a boundary between two seconds");
                System.Threading.Thread.Sleep(200);
            }
            MonitorTimer = new System.Timers.Timer(60000);
            MonitorTimer.Elapsed+=MonitorStatistics;
            MonitorTimer.Enabled=true;
        }
        public static Boolean AllowTestFunctions() {
            string env_value = Environment.GetEnvironmentVariable("SpcAllowTestFunctions");
            if ( env_value == null ) { return false; }
            return ( env_value.ToLower() == "yes" ? true : false );
        }
        public static Random ServerLevelRandomNumberGenerator = new Random();
        // public static Boolean RoomExists(string roomId) {
        //     return RoomList.ContainsKey(roomId.ToLower());
        // }
        public static async Task<Room> FindOrCreateRoom(string RoomId) {
            string roomIdToUse = RoomId;
            string lowercaseId = RoomId.ToLower();
            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
            if ( RoomList.ContainsKey(lowercaseId) ) {
                Room r =  (Room) RoomList[lowercaseId];
                return r;
            }
            else {
                Room newRoom = new Room(roomIdToUse); // Keep original name (respecting upper/lowercase)
                RoomList.Add(lowercaseId, newRoom);
                return newRoom;
            }
        }
        public static async Task<Game> LoadOrRecoverOrCreateGame(Room r) {
            Game g;
            if ( OurDB.dbMode == DatabaseModeEnum.NoDatabase
                || OurDB.dbStatus == DatabaseConnectionStatusEnum.ConnectionFailed ) {
                // We are working without a DB so will use the 'cached' game (if there is one) or create a new one
                // (this is more or less how it worked before stateless operation was implemented)
                if ( r.ActiveGame == null ) {
                    r.ActiveGame = new Game(r.RoomId, 0);
                    r.ActiveGame.InitialiseGame(null);
                    System.Diagnostics.Debug.WriteLine("New game created and active game reference cached");
                }
                return r.ActiveGame;
            }
            else if ( OurDB.dbMode == DatabaseModeEnum.Recoverability ) {
                if ( r.RecoveryAlreadyAttempted ) {
                     return r.ActiveGame;
                }
                else {
                    // Either this is a new room or there was an active game in this room but the server has been restarted and all state has been lost
                    g = await RecoverOrCreateGame(r);
                    r.RecoveryAlreadyAttempted = true; // make sure we don't try this again during the life of this server process
                    r.ActiveGame = g;
                    System.Diagnostics.Debug.WriteLine("Active game reference cached");
                    return g;
                }
            }
            // If we get here we are running Stateless. There will be no game in the server memory but we know the game id in order to reload its state
            if ( r.RecoveryAlreadyAttempted ) {
                // This is the normal situation where we are just reloading the game state that was saved at the end of the previous action
                System.Diagnostics.Debug.WriteLine("Loading game state for game with id '{0}'", r.ActiveGameId);
                g = await OurDB.LoadGameState(r.ActiveGameId);
                double lastMoveMinutesAgo = ( DateTimeOffset.UtcNow - g.LastSuccessfulAction ).TotalMinutes;
                if ( lastMoveMinutesAgo <= 60 ) {
                    // Use the returned game if the last action on it was less than an hour ago
                    g.AddToAccumulatedDbCost("Loading game in stateless mode", g.GameLoadDbCost); // Add the cost of reloading the game to its overall cost
                    return g;
                }
                else {
                    System.Diagnostics.Debug.WriteLine("Game recovered successfully but is more than an hour old, so creating new game instead\n");
                    g = new Game(r.RoomId, 0);
                    g.InitialiseGame(null);
                    return g;
                }
            }
            else {
                // Either this is a new room or there was an active game in this room but the server has been restarted and all state has been lost
                g = await RecoverOrCreateGame(r);
                r.RecoveryAlreadyAttempted = true; // make sure we don't try this again (for this room) during the life of this server process
                r.ActiveGameId = g.GameId;
                System.Diagnostics.Debug.WriteLine("Active game id noted as '{0}'.\n", r.ActiveGameId);
                return g;
            }
        }
        public static async Task<Game> RecoverOrCreateGame(Room r) {
            // Either this is a new room or there was an active game in this room but the server has been restarted and all state has been lost
            Game g;
            System.Diagnostics.Debug.WriteLine("Attempting recovery of most recent game associated with room '{0}'.\n", r.RoomId);
            g = await OurDB.LoadMostRecentGameState(r.RoomId); // If there is an existing game for this room then load it
            if ( g != null ) {
                double lastMoveMinutesAgo = ( DateTimeOffset.UtcNow - g.LastSuccessfulAction ).TotalMinutes;
                if ( lastMoveMinutesAgo <= 60 ) {
                    // Use the returned game if the last action on it was less than an hour ago
                    g.AddToAccumulatedDbCost("Recovering game", g.GameLoadDbCost); // Add the cost of reloading the game to its overall cost
                    return g;
                }
                else {
                    System.Diagnostics.Debug.WriteLine("Game recovered successfully but is more than an hour old, so creating new game instead\n");
                }
            }
            else {
                // No previous games recorded for this room, so just create a new game
                System.Diagnostics.Debug.WriteLine("No recent historical games found for room '{0}'. Creating new game.\n", r.RoomId);
            }
            g = new Game(r.RoomId, 0);
            g.InitialiseGame(null);
            return g;
        }
        public static string StringArrayAsJson(List<string> l)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,

            };
            options.Converters.Add(new JsonStringEnumConverter(null /*JsonNamingPolicy.CamelCase*/));
            string jsonString = JsonSerializer.Serialize(l, options);
            return jsonString;
        }
 
        public static int ActiveGames() {
            int a = 0;
            foreach ( Room r in RoomList.Values ) {
                if ( r.ActiveGame != null ) {
                    if ( ( DateTimeOffset.UtcNow - r.ActiveGame.LastSuccessfulAction ).TotalMinutes < 60 ) {
                        a++;
                    }
                }
            }
            return a;
        }
        public static bool TelemetryActive() {
            string metricsKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);
            return ( metricsKey != null );
        }

        private static void MonitorStatistics(Object source, ElapsedEventArgs e)
        {
            DateTimeOffset eventTimeUtc = new DateTimeOffset(e.SignalTime.ToUniversalTime());
            //System.Diagnostics.Debug.WriteLine("The MonitorStatistics event was raised at {0:HH:mm:ss.fff}", eventTimeUtc);
            ServerState.MetricsManager.GatherAndEmitStatistics(eventTimeUtc);
        }
    }
}