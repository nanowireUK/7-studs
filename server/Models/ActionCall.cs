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
            int playerIndex = G.PlayerIndexFromName(this.UserName);

            // Handle the Call (note that the base class has already checked the player's eligibility for this action)
            Participant p = G.Participants[playerIndex];
            int catchupAmount = G.MaxChipsInAllPotsForAnyPlayer() - G.ChipsInAllPotsForSpecifiedPlayer(playerIndex);
            G.MoveAmountToPotForSpecifiedPlayer(playerIndex, catchupAmount);
  
            G.LastEvent = UserName + " paid " + catchupAmount + " to call";
            G.ClearCommentary(); 
            G.AddCommentary(G.LastEvent);     

            // No one can check from this point onwards (until next card is dealt)
            G._CheckIsAvailable = false;
            
            // Find and set next player (could be no one if all players have now called)
            G.IndexOfParticipantToTakeNextAction = G.GetIndexOfPlayerToBetNext(playerIndex);
            if ( G.IndexOfParticipantToTakeNextAction > -1 ) {
                G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
            }
            else if ( G._CardsDealtIncludingCurrent < 7 ) { 
                G.DealNextRound();
                G.NextAction = "Started next round, " + G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                G.AddCommentary("End of round. Next card dealt. " + G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet");   
            }
            else  {
                // This is the end of the hand
                G.NextAction = G.ProcessEndOfHand(UserName + " called, hand ended");
            }
        }
    }     
}  
