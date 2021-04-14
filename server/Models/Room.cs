using System.Collections.Generic;
using System.Collections;
using System;

namespace SocialPokerClub.Models
{
    public class Room
    {
        public string RoomId { get; set; }
        public bool RecoveryAlreadyAttempted { get; set; }
        public Game ActiveGame { get; set; } // Only used when operating without a database
        public string ActiveGameId { get; set; } // Used when operating in database-backed stateless mode
        public DateTimeOffset LastGameAction { get; set; }
        public string AdHocQueryType { get; set; }
        public GameLog ReplayContext { get; set; }
        public int SavedCountOfLeavers { get; set; }
        public Room(string roomId) {
            RoomId = roomId;
            RecoveryAlreadyAttempted = false;
            ActiveGame = null;
            ActiveGameId = null;
            LastGameAction = DateTimeOffset.UtcNow;
            ReplayContext = null; // This will only be set for rooms created specifically to host a replayed game
            SavedCountOfLeavers = 0; // Again only used for rooms hosting a replayed game
        }
        public List<string> AdHocQueryResult() {
            // This is stupidly convoluted, but an ActionAdHocQuery command has recorded a command name in AdHocQueryType,
            // and we will now use a separate class to action the query and return the result
            AdHocQuery q = new AdHocQuery(this, AdHocQueryType);
            return q.queryResults;
        }
    }
}