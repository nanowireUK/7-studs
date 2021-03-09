﻿using System;
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
        public static bool IsInitialised = false;
        public static Hashtable RoomList; // Server-level list of Rooms
        public static StatefulGameData StatefulData; // Server-level list of connections
        public static PokerHandRankingTable RankingTable; // Only need one of these
        public static Card DummyCard;
        public static PokerDB OurDB;
        public static SpcMetricsSummary MetricsSummary;
        public static System.Timers.Timer MonitorTimer;
        public static SpcMetricsManager MetricsManager;
        public static int TotalActionsProcessed = 0;
        public static int ActiveGameAgeLimitInMinutes = 24 * 60; // keep active games (i.e. not in the lobby) open for a whole day
        public static int InactiveGameAgeLimitInMinutes = 1 * 60; // keep inactive games (i.e. those still - or back - in the lobby) open for 1 hour only
        static ServerState() {
            // Static constructor, runs initialisations in the order we require
            Console.WriteLine("ServerState static constructor running at {0:HH:mm:ss.fff}", DateTimeOffset.UtcNow);
            RoomList = new Hashtable(); // Server-level list of Rooms
            StatefulData = new StatefulGameData(); // Server-level list of connections
            RankingTable = new PokerHandRankingTable(); // Only need one of these
            DummyCard = new Card(CardEnum.Dummy, SuitEnum.Clubs);
            OurDB = new PokerDB();
            MetricsSummary = new SpcMetricsSummary(); // Just an initial 'zero' summary, but still needs OurDB to have been instantiated
            MetricsManager = new SpcMetricsManager(); // Needs MetricsSummary to have been instantiated. This object will maintain and emit statistics
            Console.WriteLine("Initalising MonitorTimer");
            while ( DateTimeOffset.UtcNow.Millisecond < 100 || DateTimeOffset.UtcNow.Millisecond > 900 ) {
                Console.WriteLine("Waiting 200 milliseconds to ensure repeat timer does not fire close to a boundary between two seconds");
                System.Threading.Thread.Sleep(200);
            }
            MonitorTimer = new System.Timers.Timer(60000);
            MonitorTimer.Elapsed+=MonitorStatistics;
            MonitorTimer.Enabled=true;
            IsInitialised = true;
            Console.WriteLine("ServerState fully initialised at {0:HH:mm:ss.fff}", DateTimeOffset.UtcNow);
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
            while ( ServerState.IsInitialised == false ) {
                Console.WriteLine("FindOrCreateRoom waiting 0.1 secs for ServerState to initialise (time now {0:HH:mm:ss.fff})", DateTimeOffset.UtcNow);
                System.Threading.Thread.Sleep(100); // wait for a 10th of a second
            }
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
                    Console.WriteLine("New game created and active game reference cached");
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
                    Console.WriteLine("Active game reference cached");
                    return g;
                }
            }
            // If we get here we are running Stateless, so there will be no game in the server memory.
            // If the server has just restarted, we will need to recover the game and record the game id on the room.
            // Otherwise (in normal stateless operation) we use the game id that we recorded on the room to go to the DB to reload the game state
            if ( r.RecoveryAlreadyAttempted == false ) {
                // Either this is a new room or there was an active game in this room but the server has been restarted and all state has been lost
                g = await RecoverOrCreateGame(r);
                r.RecoveryAlreadyAttempted = true; // make sure we don't try this again (for this room) during the life of this server process
                r.ActiveGameId = g.GameId;
                Console.WriteLine("Active game id noted as '{0}'.", r.ActiveGameId);
                return g;
            }
            else {
                // This is the normal situation where we are just reloading the game state that was saved at the end of the previous action
                Console.WriteLine("Reloading game state for game with id '{0}'", r.ActiveGameId);
                g = await OurDB.LoadGameState(r.ActiveGameId);
                if ( g is null ) {
                    Console.WriteLine("Game unexpectedly not found");
                }
                double lastMoveMinutesAgo = ( DateTimeOffset.UtcNow - g.LastSuccessfulAction ).TotalMinutes;
                if ( g.GameMode == GameModeEnum.LobbyOpen && lastMoveMinutesAgo <= InactiveGameAgeLimitInMinutes ) {
                    g.AddToAccumulatedDbCost("Loading inactive game in stateless mode", g.GameLoadDbCost); // Add the cost of reloading the game to its overall cost
                    return g;
                }
                else if ( lastMoveMinutesAgo <= ActiveGameAgeLimitInMinutes ) {
                    // Use the returned game if the last action on it was less than an hour ago
                    g.AddToAccumulatedDbCost("Loading active game in stateless mode", g.GameLoadDbCost); // Add the cost of reloading the game to its overall cost
                    return g;
                }
                else {
                    Console.WriteLine("Game recovered successfully but is {0} minutes old, which is longer than allowed for game mode '{1}', so creating new game instead", 
                        lastMoveMinutesAgo, g.GameMode.ToString());
                    g = new Game(r.RoomId, 0);
                    g.InitialiseGame(null);
                    return g;
                }
            }
        }
        public static async Task<Game> RecoverOrCreateGame(Room r) {
            // Either this is a new room or there was an active game in this room but the server has been restarted and all state has been lost
            Game g;
            Console.WriteLine("Attempting recovery of most recent game associated with room '{0}'.", r.RoomId);
            g = await OurDB.LoadMostRecentGameState(r.RoomId); // If there is an existing game for this room then load it
            if ( g != null ) {
                double lastMoveMinutesAgo = ( DateTimeOffset.UtcNow - g.LastSuccessfulAction ).TotalMinutes;
                if ( lastMoveMinutesAgo <= 60 ) {
                    // Use the returned game if the last action on it was less than an hour ago
                    g.AddToAccumulatedDbCost("Recovering game", g.GameLoadDbCost); // Add the cost of reloading the game to its overall cost
                    return g;
                }
                else {
                    Console.WriteLine("Game recovered successfully but is more than an hour old, so creating new game instead");
                }
            }
            else {
                // No previous games recorded for this room, so just create a new game
                Console.WriteLine("No recent historical games found for room '{0}'. Creating new game.", r.RoomId);
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
        public static int RoomsWithActivityInLastHour() {
            int a = 0;
            foreach ( Room r in RoomList.Values ) {
                if ( ( DateTimeOffset.UtcNow - r.LastGameAction ).TotalMinutes < 60 ) {
                    a++;
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
            //Console.WriteLine("The MonitorStatistics event was raised at {0:HH:mm:ss.fff}", eventTimeUtc);
            ServerState.MetricsManager.GatherAndEmitStatistics(eventTimeUtc);
        }
    }
}