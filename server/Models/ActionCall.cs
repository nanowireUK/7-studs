namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionCall' Class  
    /// </summary>  
    public class ActionCall : Action
    {  
        public ActionCall(string connectionId, ActionEnum actionType, string gameId, string user) : base(connectionId, actionType, gameId, user)
        {
        }
        public override void ProcessAction()
        {         
            // Handle the Call (note that the base class has already checked the player's eligibility for this action)
            Participant p = G.Participants[PlayerIndex];
            int catchupAmount = G.MaxChipsInAllPotsForAnyPlayer() - G.ChipsInAllPotsForSpecifiedPlayer(PlayerIndex);
            G.MoveAmountToPotForSpecifiedPlayer(PlayerIndex, catchupAmount);
  
            G.LastEvent = UserName + " paid " + catchupAmount + " to call";
            G.ClearCommentary(); 
            G.AddCommentary(G.LastEvent);     

            // No one can check from this point onwards (until next card is dealt)
            G._CheckIsAvailable = false;
            
            // Find and set next player (could be no one if all players have now called)
            G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);
        }
    }     
}  
