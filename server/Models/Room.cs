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
        public Game ActiveGame { get; set; }
        public List<GameLog> GameLogs { get; set; } // Copies of game logs get dumped here each time a game completes in this room
        public LobbyData LobbyData { get; set; }
        protected List<User> RegisteredUsers { get; set; }
        public string AdHocQueryType { get; set; }
        public Room(string roomId) {
            RoomId = roomId;
            RegisteredUsers = new List<User>();
            GameLogs = new List<GameLog>();
            ActiveGame = null;
        }
        public List<string> AdHocQueryResult() {
            // This is stupidly convoluted, but an ActionAdHocQuery command has recorded a command name in AdHocQueryType,
            // and we will now use a separate class to action the query and return 
            AdHocQuery q = new AdHocQuery(this, AdHocQueryType);
            return q.queryResults;
        }
    }
}