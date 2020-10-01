namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionCover' Class  
    /// </summary>  
    public class ActionCover : Action
    {  
        public ActionCover(string connectionId, ActionEnum actionType, string gameId, string user, string leavers) 
            : base(connectionId, actionType, gameId, user, leavers)
        {
        }
        public override void ProcessAction()
        {
            // Handle the cover (like a call but where player doesn't have enough to cover the current raise)
            // (note that the base class has already checked the player's eligibility for this action)
            Participant p = G.Participants[PlayerIndex];

            G.RecordLastEvent(UserName + " paid " + p.UncommittedChips + " to cover the pot");

            // Implement the cover (has to be done pot-by-pot, and could involve splitting a pot)
            p.HasCovered = true;
            G.MoveAmountToPotForSpecifiedPlayer(PlayerIndex, p.UncommittedChips);
            
            // Find and set next player (could be no one if all players have now called or covered)
            G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);
        }
    }     
}  
