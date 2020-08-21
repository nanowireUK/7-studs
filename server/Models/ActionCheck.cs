namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionCheck' Class  
    /// </summary>  
    public class ActionCheck : Action
    {  
        public ActionCheck(string connectionId, ActionEnum actionType, string gameId, string user) : base(connectionId, actionType, gameId, user)
        {
        }
        public override void ProcessAction()
        {          
            // Handle the Check (note that the base class has already checked the player's eligibility for this action)
            if ( G._IndexOfLastPlayerToStartChecking == -1 ){
                // If this player is the first to check then note this (noting that players can only do this at the start of a betting round)
                G._IndexOfLastPlayerToStartChecking = PlayerIndex;
            }
            Participant p = G.Participants[PlayerIndex];
            G.ClearCommentary(); 
            G.LastEvent = UserName + " checked";
            G.AddCommentary(G.LastEvent);

            // Find and set next player (could be no one if all players have now checked)
            G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);
        }
    }     
}  
