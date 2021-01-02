using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class ActionAvailabilityMap
    {
        // Implement our own Hashtable of permissions because the Hashtable class does not appear to be supported by the JSON serialiser
        public List<ActionAvailability> map { get ; set; }

        public ActionAvailabilityMap()
        {
            map = new List<ActionAvailability>();
        }

        public void SetAvailability(ActionEnum ac, AvailabilityEnum av) 
        {
            bool entryFound = false;
            foreach ( ActionAvailability existingEntry in map ) {
                if ( existingEntry.Action == ac ) {
                    existingEntry.Availability = av;
                    entryFound = true;
                    break;
                }
            }
            if ( ! entryFound ) {
                ActionAvailability newEntry = new ActionAvailability(ac, av);
                map.Add(newEntry);
            }
        } 

        public AvailabilityEnum GetAvailability(ActionEnum actionType) 
        {
            // Check what permissions are currently required for a given action
            foreach ( ActionAvailability a in map ) {
                if ( a.Action == actionType ) {
                    return a.Availability;
                }
            }  
            return AvailabilityEnum.NotAvailable;
        }      
    }
}