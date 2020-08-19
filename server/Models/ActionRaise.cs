namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionRaise' Class  
    /// </summary>  
    public class ActionRaise : Action
    {  
        public ActionRaise(string connectionId, ActionEnum actionType, string gameId, string user, string raiseAmount) 
            : base( connectionId, actionType, gameId, user, raiseAmount)
        {
        }
        public override void ProcessAction()
        {
            int amountAsInt;
            if ( int.TryParse(Parameters, out amountAsInt) == false ) {
                G.LastEvent = this.UserName + " entered a non-numeric amount to raise by";
            }
            else if ( amountAsInt < 1 ) {
                G.LastEvent = this.UserName + " did not enter a raise amount of 1 or more";
            }           
            else {
                // Handle the raise (note that the base class has already checked the player's eligibility for this action)
                Participant p = G.Participants[PlayerIndex];
                int catchupAmount = G.MaxChipsInAllPotsForAnyPlayer() - G.ChipsInAllPotsForSpecifiedPlayer(PlayerIndex);
                if ( p.UncommittedChips < ( catchupAmount + amountAsInt ) ) {
                    G.LastEvent = this.UserName + " tried to raise by " +  amountAsInt + " but maximum raise would be " + ( p.UncommittedChips - catchupAmount );
                }               
                else {
                    // Implement the raise
                    G.ClearCommentary(); 
                    string msg = this.UserName;
                    // Add this amount to the pot for this player
                    G.MoveAmountToPotForSpecifiedPlayer(PlayerIndex, catchupAmount + amountAsInt);
                    if ( catchupAmount > 0 ) {
                        msg += " paid " + catchupAmount + " to stay in and";
                    }
                    msg += " raised by " +  amountAsInt;
                    G.LastEvent = msg;
                    G.AddCommentary(G.LastEvent);                                        
                    G._IndexOfLastPlayerToRaise = PlayerIndex; // Note that this player was the last one to raise

                    // No one can check from this point onwards (until next card is dealt)
                    G._CheckIsAvailable = false;
                    G.SetActionAvailability(ActionEnum.Check, AvailabilityEnum.NotAvailable); 
                    
                    // Identifier player to play next and reset action message to reflect this (in all other cases it stays unchanged)
                    // Find and set next player (should always be able to find one but might be worth checking)
                    G.IndexOfParticipantToTakeNextAction = G.GetIndexOfPlayerToBetNext(PlayerIndex);
                    G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                    G.SetActionAvailabilityBasedOnCurrentPlayer();
                }
            }
        }
    }     
}  
