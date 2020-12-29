using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionCover' Class  
    /// </summary>  
    public class ActionCover : Action
    {  
        public ActionCover(string connectionId, ActionEnum actionType, string roomId, string user, string leavers) 
            : base(connectionId, actionType, roomId, user, leavers)
        {
        }
        public override async Task ProcessAction()
        {
            // Handle the cover (like a call but where player doesn't have enough to cover the current raise)
            // (note that the base class has already checked the player's eligibility for this action)
            Participant p = G.Participants[PlayerIndex];

            G.RecordLastEvent(UserName + " paid " + p.UncommittedChips + " to cover the pot");

            // Implement the cover (has to be done pot-by-pot, and could involve splitting a pot)
            p.HasCovered = true;
            G.MoveAmountToPotForSpecifiedPlayer(PlayerIndex, p.UncommittedChips);

            G.Participants[PlayerIndex].LastActionInThisHand = this.ActionType;
            G.Participants[PlayerIndex].LastActionAmount = p.UncommittedChips;
            
            // Find and set next player (could be no one if all players have now called or covered)
            await G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);
        }
    }     
}  
