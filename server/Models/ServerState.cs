using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

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
        public static Boolean IsRunningOnPublicServer() {
            string origin_value = Environment.GetEnvironmentVariable("SevenStudsOrigin");
            if ( origin_value == null ) { return false; }
            return ( origin_value == "https://7studsserver.azurewebsites.net/" );
        }
        public static Random ServerLevelRandomNumberGenerator = new Random();
        public static Boolean RoomExists(string roomId) {
            return RoomList.ContainsKey(roomId.ToLower());
        }
        public static Room FindOrCreateRoom(string RoomId) {
            string roomIdToUse = RoomId; 
            string lowercaseId = RoomId.ToLower();
            if ( RoomList.ContainsKey(lowercaseId) ) {
                Room r =  (Room) RoomList[lowercaseId];
                roomIdToUse = r.RoomId; // Get the room id as originally supplied by whoever created the room
                if ( r.ActiveGame != null ) {
                    if ( r.ActiveGame.MinutesSinceLastAction() <= 120 ) {
                        return r;
                    }
                    else {
                        // Archive the current version of the room so that we can start again from scratch
                        r.RoomId += "-finished-"+r.ActiveGame.LastSuccessfulAction.ToString("yyyy-MM-dd.HHmm");
                        RoomList.Add(r.RoomId, r); 
                        // Remove the link with the old name (we'll add the new version of the room below)
                        RoomList.Remove(lowercaseId);  
                    }
                }
            }
            Room newRoom = new Room(roomIdToUse); // Keep original name (respecting upper/lowercase)
            RoomList.Add(lowercaseId, newRoom);

            // Note that other archived rooms may be using up memory ... clear them out after seven days
            foreach ( string id in RoomList.Keys )
            {
                Room r = (Room) RoomList[id];
                if ( r.ActiveGame != null && r.ActiveGame.MinutesSinceLastAction() > ( 7 * 24 * 60) ) {
                    RoomList.Remove(id);
                }
            }

            // If there is no active game against this room, create an 'empty', unstarted game
            if ( newRoom.ActiveGame == null){
                newRoom.ActiveGame = new Game(newRoom.RoomId, 0);
            }

            // Finally return the room we've just found or created
            return newRoom;
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