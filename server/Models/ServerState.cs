using System;
using System.Collections;
//using System.Collections.Generic;

namespace SevenStuds.Models
{
    public static class ServerState
    {
        // Maintains a registry of games which enables a game object to be found from its ID.
        public static Hashtable RoomList = new Hashtable(); // Server-level list of Rooms
        // Also provides other room-level functions (where a room may have hosted a whole series of games)
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
                newRoom.ActiveGame = new Game(newRoom, 0);
            }

            // Finally return the room we've just found or created
            return newRoom;
        }
    }
}