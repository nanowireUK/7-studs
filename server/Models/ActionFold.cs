using System;
using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionFold' Class  
    /// </summary>  
    public class ActionFold : Action
    {  
        public ActionFold(string connectionId, ActionEnum actionType, string roomId, string user, string leavers) 
            : base(connectionId, actionType, roomId, user, leavers)
        {
        }
        public override async Task ProcessAction()
        {
            // Handle the fold (note that the base class has already checked the player's eligibility for this action)
            Participant p = G.Participants[PlayerIndex];

            G.RecordLastEvent(UserName + " folded");

            // Implement the Fold
            p.HasFolded = true;

            if ( p.UncommittedChips == 0 ) {
                // Player is now bankrupt
                p.TimeOfBankruptcy = DateTimeOffset.Now;
            }

            // Find and set next player (could be no one if all players have now folded)
            G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);  

            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
        }
    }     
}  
