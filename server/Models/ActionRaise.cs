using Microsoft.AspNetCore.SignalR;

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
                throw new HubException("You entered a non-numeric amount to raise by");
            }
            if ( amountAsInt < 1 ) {
                throw new HubException("You must enter a raise amount of 1 or more");
            }           
            // Handle the raise (note that the base class has already checked the player's eligibility for this action)
            Participant p = G.Participants[PlayerIndex];
            int catchupAmount = G.MaxChipsInAllPotsForAnyPlayer() - G.ChipsInAllPotsForSpecifiedPlayer(PlayerIndex);
            int maxRaise = G.MaxRaiseForParticipantToTakeNextAction; // previously: ( p.UncommittedChips - catchupAmount );
            
            if ( amountAsInt > maxRaise ) {
                throw new HubException("You tried to raise by " + amountAsInt + " but the most you can raise by is " +  maxRaise);
            }               
            // Implement the raise
            G.ClearCommentary(); 
            string msg = this.UserName;
            // Add this amount to the pot for this player
            G.MoveAmountToPotForSpecifiedPlayer(PlayerIndex, catchupAmount + amountAsInt);
            if ( catchupAmount > 0 ) {
                msg += " paid " + catchupAmount + " to stay in and";
            }
            msg += " raised by " +  amountAsInt;
            G.RecordLastEvent(msg);
            G._IndexOfLastPlayerToRaise = PlayerIndex; // Note that this player was the last one to raise

            // No one can check from this point onwards (until next card is dealt)
            G._CheckIsAvailable = false;

            // Find and set next player (could be no one if all players have now checked)
            G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);                    
        }
    }     
}  
