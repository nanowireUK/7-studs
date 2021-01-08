using System;
using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionFold' Class  
    /// </summary>  
    public class ActionFold : Action
    {  
        public ActionFold(string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers) 
            : base(connectionId, actionType, ourGame, user, leavers)
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

            G.Participants[PlayerIndex].LastActionInThisHand = this.ActionType;
            G.Participants[PlayerIndex].LastActionAmount = 0;
            G.Participants[PlayerIndex].RoundNumberOfLastAction = G._CardsDealtIncludingCurrent;

            // Find and set next player (could be no one if all players have now folded)
            await G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);  
        }
    }     
}  
