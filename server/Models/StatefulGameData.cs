using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class StatefulGameData
    {
        // Objects of this class record game state information that needs to be persisted across
        // the otherwise stateless handling of game actions.
        // This is primarily the SignalR connections and the games/players they relate to.

        // We are not using Hashtable because it doesn't appear to be supported by the JSON serialiser.

        public Dictionary<string, string> MapOfConnectionIdToParticipantSignalRGroupName { get; set; } 
        public Dictionary<string, string> MapOfConnectionIdToSpectatorSignalRGroupName { get; set; } 
        public Dictionary<string, double> MapOfGameIdToDbCosts { get; set; } 
        public StatefulGameData()
        {
            MapOfConnectionIdToParticipantSignalRGroupName = new Dictionary<string, string>(); 
            MapOfConnectionIdToSpectatorSignalRGroupName = new Dictionary<string, string>(); 
            MapOfGameIdToDbCosts = new Dictionary<string, double>(); 
        }
    }
}