using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SevenStuds.Models
{
    public static class ServerState
    {
        // Maintains a registry of games which enables a game object to be found from its ID.
        // Also provides other room-level functions (where a room may have hosted a whole series of games)
        public static Hashtable RoomList = new Hashtable(); // Server-level list of Rooms
        public static StatefulGameData GameConnections = new StatefulGameData(); // Server-level list of connections
        public static PokerHandRankingTable RankingTable = new PokerHandRankingTable(); // Only need one of these
        public static Card DummyCard = new Card(CardEnum.Dummy, SuitEnum.Clubs);
        public static PokerDB OurDB = new PokerDB();
        public static Boolean AllowTestFunctions() {
            string env_value = Environment.GetEnvironmentVariable("SpcAllowTestFunctions");
            if ( env_value == null ) { return false; }
            return ( env_value.ToLower() == "yes" ? true : false );
        }
        public static Random ServerLevelRandomNumberGenerator = new Random();
        public static Boolean RoomExists(string roomId) {
            return RoomList.ContainsKey(roomId.ToLower());
        }
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
            if ( OurDB.dbMode == DatabaseModeEnum.NoDatabase || OurDB.dbStatus == DatabaseConnectionStatusEnum.ConnectionFailed ) {
                // We are working without a DB so will use the 'cached' game (if there is one) or create a new one
                // (this is more or less how it worked before stateless operation was implemented)
                if ( r.ActiveGame == null ) {
                    r.ActiveGame = new Game(r.RoomId, 0);
                    r.ActiveGame.InitialiseGame(null);
                }
                return r.ActiveGame;
            }
            // Otherwise we are working in more-or-less-stateless mode
            Game g;
            if ( r.ActiveGameId != null) {
                // This is the normal situation where we are just reloading the game state that was saved at the end of the previous action
                Console.WriteLine("Loading game state for game with id '{0}'.", r.ActiveGameId);
                g = await OurDB.LoadGameState(r.ActiveGameId);
                //
                // Need to catch failure here and start a new game (can happen if initial action failed to save a game state)
                //
                return g;
            }
            else {
                // Either this is a new room that has not yet had a game created for it, or the server has been restarted and all state has been lost
                Console.WriteLine("Loading most recent game associated with room '{0}'.", r.RoomId);
                g = await OurDB.LoadMostRecentGameState(r.RoomId); // If there is an existing game for this room then load it
                if ( g == null ) { 
                    // No previous games recorded for this room, so just create a new game
                    Console.WriteLine("No recent historical games found for room '{0}'. Creating new game.", r.RoomId);
                    g = new Game(r.RoomId, 0);
                    g.InitialiseGame(null);
                }
                r.ActiveGameId = g.GameId;
                Console.WriteLine("Active game id flagged as '{0}'.", r.ActiveGameId);
                return g;
            }
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
        public static void ClearConnectionMappings(Game g) {
            // Clear out the tester's current connection (and any other connections currently associated with the game)
            // Note that this is only expected to be called during a replay in a dev environment, not on the live server
            ServerState.GameConnections.MapOfConnectionIdToParticipantSignalRGroupName.Clear(); 
            ServerState.GameConnections.MapOfConnectionIdToSpectatorSignalRGroupName.Clear(); 
        }

        public static void LinkConnectionToParticipant(Game g, string connectionId, Participant p) 
        {
            ServerState.GameConnections.MapOfConnectionIdToParticipantSignalRGroupName.Add(connectionId, p.ParticipantSignalRGroupName);
        }
        public static void LinkConnectionToSpectator(Game g, string connectionId, Spectator p) 
        {
            ServerState.GameConnections.MapOfConnectionIdToSpectatorSignalRGroupName.Add(connectionId, p.SpectatorSignalRGroupName);
        }

        public static Participant GetParticipantFromConnection(Game g, string connectionId) 
        {
            string groupName;
            if ( ServerState.GameConnections.MapOfConnectionIdToParticipantSignalRGroupName.TryGetValue(connectionId, out groupName) )
            {
                foreach ( Participant p in g.Participants ) {
                    if ( p.ParticipantSignalRGroupName == groupName ) {
                        return p;
                    }
                }
                return null;
            }
            else 
            {
                return null;
            }
        }

        public static Spectator GetSpectatorFromConnection(Game g, string connectionId) 
        {
            string groupName;
            if ( ServerState.GameConnections.MapOfConnectionIdToSpectatorSignalRGroupName.TryGetValue(connectionId, out groupName) )
            {
                foreach ( Spectator s in g.Spectators ) {
                    if ( s.SpectatorSignalRGroupName == groupName ) {
                        return s;
                    }
                }
                return null;
            }
            else 
            {
                return null;
            }
        }
    }
}