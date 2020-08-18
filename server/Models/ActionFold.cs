namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionFold' Class  
    /// </summary>  
    public class ActionFold : Action
    {  
        public ActionFold(string connectionId, ActionEnum actionType, string gameId, string user) : base(connectionId, actionType, gameId, user)
        {
        }
        public override void ProcessAction()
        {
            // Handle the Fold (note that the base class has already checked the player's eligibility for this action)
            Participant p = G.Participants[PlayerIndex];

            // Implement the Fold
            G.ClearCommentary(); 
            p.HasFolded = true;
            G.LastEvent = UserName + " folded";
            G.AddCommentary(G.LastEvent);               
           
            // Check for scenario where only one active player is left
            if ( G.CountOfPlayersLeftIn() == 1 ) {
                // Everyone has folded except one player
                G.NextAction = G.ProcessEndOfHand(UserName + " folded, only one player left in, hand ended"); // will also update commentary with hand results
            }
            else {
                // Find and set next player (could be no one if all players have now called or folded)
                G.IndexOfParticipantToTakeNextAction = G.GetIndexOfPlayerToBetNext(PlayerIndex);
                if ( G.IndexOfParticipantToTakeNextAction > -1 ){
                    G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                }
                else if ( G._CardsDealtIncludingCurrent < 7 ) {
                    G.DealNextRound();
                    G.NextAction = "Started next round, " + G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                }
                else  {
                    // All 7 cards have now been bet on, so this is the end of the hand
                    G.NextAction = G.ProcessEndOfHand(UserName + " folded, hand ended");  // will also update commentary with hand results
                }
                G.SetActionAvailabilityBasedOnCurrentPlayer();

            }
        }
    }     
}  
