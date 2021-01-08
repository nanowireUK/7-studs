using System.Collections.Generic;
using System.Collections;
using System;

namespace SevenStuds.Models
{
    public class Room
    {
        public string RoomId { get; set; }
        public Game ActiveGame { get; set; } // Only used when operating without a database
        public string ActiveGameId { get; set; } // Used when operating in database-backed stateless mode
        public List<GameLog> GameLogs { get; set; } // Copies of game logs get dumped here each time a game completes in this room
        public LobbyData LobbyData { get; set; }
        protected List<User> RegisteredUsers { get; set; }
        public string AdHocQueryType { get; set; }
        public Room(string roomId) {
            RoomId = roomId;
            RegisteredUsers = new List<User>();
            GameLogs = new List<GameLog>();
            ActiveGame = null;
            ActiveGameId = null;
        }
        public List<string> AdHocQueryResult() {
            // This is stupidly convoluted, but an ActionAdHocQuery command has recorded a command name in AdHocQueryType,
            // and we will now use a separate class to action the query and return the result
            AdHocQuery q = new AdHocQuery(this, AdHocQueryType);
            return q.queryResults;
        }
    }
}