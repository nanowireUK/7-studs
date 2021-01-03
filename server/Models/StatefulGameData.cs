using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class StatefulGameData
    {
        // Objects of this class record the availability that is currently associated with a given action.
        // These are used in an ActionAvailabilityMap as the basis for a lookup table.
        // We are not using Hashtable because it doesn't appear to be supported by the JSON serialiser.

        public Dictionary<string, Participant> _ConnectionToParticipantMap { get; set; } 
        public Dictionary<string, Spectator> _ConnectionToSpectatorMap { get; set; } 
        public StatefulGameData()
        {
            _ConnectionToParticipantMap = new Dictionary<string, Participant>(); 
            _ConnectionToSpectatorMap = new Dictionary<string, Spectator>(); 
        }
    }
}