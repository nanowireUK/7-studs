using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionCall' Class  
    /// </summary>  
    public class ActionCall : Action
    {  
        public ActionCall(string connectionId, ActionEnum actionType, string roomId, string user, string leavers) 
            : base(connectionId, actionType, roomId, user, leavers)
        {
        }
        public override async Task ProcessAction()
        {         
            // Handle the Call (note that the base class has already checked the player's eligibility for this action)
            Participant p = G.Participants[PlayerIndex];
            int catchupAmount = G.MaxChipsInAllPotsForAnyPlayer() - G.ChipsInAllPotsForSpecifiedPlayer(PlayerIndex);
            G.MoveAmountToPotForSpecifiedPlayer(PlayerIndex, catchupAmount);

            G.RecordLastEvent(UserName + " paid " + catchupAmount + " to call");
  
            // No one can check from this point onwards (until next card is dealt)
            G._CheckIsAvailable = false;
            
            // Find and set next player (could be no one if all players have now called)
            G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);
        }
    }     
}  
