namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionCheck' Class  
    /// </summary>  
    public class ActionCheck : Action
    {  
        public ActionCheck(ActionEnum actionType, string gameId, string user) : base(actionType, gameId, user)
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
            G.IndexOfParticipantToTakeNextAction = G.GetIndexOfPlayerToBetNext(PlayerIndex);
            if ( G.IndexOfParticipantToTakeNextAction > -1 ){
                G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                G.AddCommentary(G.NextAction);
            }
            else if ( G._CardsDealtIncludingCurrent < 7 ) {
                G.DealNextRound();
                G.NextAction = "Everyone checked; started next round, " + G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                G.AddCommentary(G.NextAction);
            }
            else  {
                // This is the end of the hand
                G.NextAction = G.ProcessEndOfHand(UserName + " checked, hand ended");
            }
            G.SetActionAvailabilityBasedOnCurrentPlayer();
        }
    }     
}  
