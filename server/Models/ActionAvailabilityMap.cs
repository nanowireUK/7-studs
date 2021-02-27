using System.Collections.Generic;

namespace SocialPokerClub.Models
{
    public class ActionAvailabilityMap
    {
        // Implement our own Hashtable of permissions because the Hashtable class does not appear to be supported by the JSON serialiser
        public List<ActionAvailability> MapOfActionsToAvailabilities { get ; set; }

        public ActionAvailabilityMap()
        {
            MapOfActionsToAvailabilities = new List<ActionAvailability>();
        }

        public void SetAvailability(ActionEnum ac, AvailabilityEnum av)
        {
            bool entryFound = false;
            foreach ( ActionAvailability existingEntry in MapOfActionsToAvailabilities ) {
                if ( existingEntry.Action == ac ) {
                    existingEntry.Availability = av;
                    entryFound = true;
                    break;
                }
            }
            if ( ! entryFound ) {
                ActionAvailability newEntry = new ActionAvailability(ac, av);
                MapOfActionsToAvailabilities.Add(newEntry);
            }
        }

        public AvailabilityEnum GetAvailability(ActionEnum actionType)
        {
            // Check what permissions are currently required for a given action
            foreach ( ActionAvailability a in MapOfActionsToAvailabilities ) {
                if ( a.Action == actionType ) {
                    return a.Availability;
                }
            }
            return AvailabilityEnum.NotAvailable;
        }
    }
}