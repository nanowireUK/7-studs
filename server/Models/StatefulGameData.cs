using System.Collections.Generic;

namespace SocialPokerClub.Models
{
    public class StatefulGameData
    {
        // Objects of this class record game state information that needs to be persisted across
        // the otherwise stateless handling of game actions.
        // This is primarily the SignalR connections and the games/players they relate to.

        // Both dictionaries link a room to a dictionary that links connections ids to the SignalR group for a specific player

        // For each game, maintain a list of lists each with connection id, 

        public Dictionary<string, double> MapOfGameIdToDbCosts { get; set; }

        public Dictionary<string, Dictionary<string, Dictionary<string, bool>>> RoomLevelMapOfGroupToConnections { get; set; }
        //                RoomId          -> Group           -> Connections (with status)
        public StatefulGameData()
        {
            MapOfGameIdToDbCosts = new Dictionary<string, double>();
            RoomLevelMapOfGroupToConnections = new Dictionary<string, Dictionary<string, Dictionary<string, bool>>>();
        }
        public Dictionary<string, Dictionary<string, bool>> FindOrCreateRoomLevelGroupToConnectionMappings(Game g) {
            // Return a map of SignalR group name to connectionIds (creating an empty one if necessary)
            string roomId = g.ParentRoom().RoomId;
            Dictionary<string, Dictionary<string, bool>> groupsToConnections;
            if ( ! RoomLevelMapOfGroupToConnections.TryGetValue(roomId, out groupsToConnections) ) {
                groupsToConnections = new Dictionary<string, Dictionary<string, bool>>();
                RoomLevelMapOfGroupToConnections.Add(roomId, groupsToConnections);
            }
            return groupsToConnections;
        }
        public void ClearGroupToConnectionMappingsForGame(Game g) {
            // Clear out the tester's current connections (and any other connections currently associated with the game)
            // Note that this is only expected to be called during a replay in a dev environment, not on the live server
            FindOrCreateRoomLevelGroupToConnectionMappings(g).Clear();
        }
        public void LinkConnectionToGroup(Game g, string connectionId, Participant p)
        {
            System.Diagnostics.Debug.WriteLine(
                "Room '{0}': Linking group '{1}' to connection id '{2}'", g.ParentRoom().RoomId, p.ParticipantSignalRGroupName, connectionId);
            Dictionary<string, Dictionary<string, bool>> roomLevelGroups = FindOrCreateRoomLevelGroupToConnectionMappings(g);
            // First find or create the dictionary of connections for this SignalR group name
            Dictionary<string, bool> connsForThisGroup;
            if ( ! roomLevelGroups.TryGetValue(p.ParticipantSignalRGroupName, out connsForThisGroup) ) {
                connsForThisGroup = new Dictionary<string, bool>();
                roomLevelGroups.Add(p.ParticipantSignalRGroupName, connsForThisGroup);
            }
            // Then add the connection if it is a new one for this group
            if ( ! connsForThisGroup.ContainsKey(connectionId) ) {
                connsForThisGroup.Add(connectionId, false); // add a new connection and note the fact that it is a new one for this group
            }
         
            // Might want to add something here to check that the connection was not previously associated with any other player
            // (think this is something for Join/Rejoin though)
            // if ( connsForThisGroup != p.ParticipantSignalRGroupName ) {
            //     System.Diagnostics.Debug.WriteLine("Unexpected error: connection '{0}' is already linked to a different SignalR group '{1}'", connectionId, signalrGroup);
            // }
        }
        public Participant GetParticipantFromConnection(Game g, string connectionId)
        {
            Dictionary<string, Dictionary<string, bool>> roomLevelGroups = FindOrCreateRoomLevelGroupToConnectionMappings(g);
            foreach ( string groupName in roomLevelGroups.Keys ) {
                Dictionary<string, bool> dd = roomLevelGroups[groupName];
                foreach ( string conn in dd.Keys ) {
                    if ( conn == connectionId ) {
                        foreach ( Participant p in g.Participants ) {
                            if ( p.ParticipantSignalRGroupName == groupName ) {
                                return p;
                            }
                        }
                    }
                }
            }
            return null;
        }
        public Spectator GetSpectatorFromConnection(Game g, string connectionId)
        {
            Dictionary<string, Dictionary<string, bool>> roomLevelGroups = FindOrCreateRoomLevelGroupToConnectionMappings(g);
            foreach ( string groupName in roomLevelGroups.Keys ) {
                Dictionary<string, bool> dd = roomLevelGroups[groupName];
                foreach ( string conn in dd.Keys ) {
                    if ( conn == connectionId ) {
                        foreach ( Spectator p in g.Spectators ) {
                            if ( p.SpectatorSignalRGroupName == groupName ) {
                                return p;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public List<List<string>> MarkUnlinkedConnectionsAsLinkedAndReturnList(Game g) {
            // 
            List<List<string>> results = new List<List<string>>();
            string roomId = g.ParentRoom().RoomId;
            Dictionary<string, Dictionary<string, bool>> groupData = ServerState.StatefulData.RoomLevelMapOfGroupToConnections[roomId];
            foreach ( string groupId in groupData.Keys ) {
                Dictionary<string, bool> connData = groupData[groupId];
                foreach ( string connId in connData.Keys ) {
                    if ( connData[connId] == false ) {
                        List<string> r = new List<string>();
                        r.Add(connId);
                        r.Add(groupId);
                        results.Add(r);
                        System.Diagnostics.Debug.WriteLine("Room '{0}' : Group '{1}' : Connection '{2}' will be added", roomId, groupId, connId);
                    }
                }
            }  
            return results;
        }
    }
}