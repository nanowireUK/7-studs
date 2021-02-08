using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionRaise' Class  
    /// </summary>  
    public class ActionRaise : Action
    {  
        public ActionRaise(string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers, string raiseAmount) 
            : base( connectionId, actionType, ourGame, user, leavers, raiseAmount)
        {
        }
        public override async Task ProcessAction()
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

            // If we're playing a limit game, check the raise is one of the previously set allowable values
            if ( G.IsLimitGame ) {
                if ( G._AllowedRaiseAmounts.Count == 0 ) {
                    throw new HubException("You tried to raise by " + amountAsInt + " but this is a limit game and the are no valid raises at this point");
                } 
                else if ( ! G._AllowedRaiseAmounts.Contains(amountAsInt) ) {
                    throw new HubException("You tried to raise by " + amountAsInt + " but this is a limit game and your options are " + string.Join(", ", G._AllowedRaiseAmounts) +" only");
                }    
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

            G.Participants[PlayerIndex].LastActionInThisHand = this.ActionType;
            G.Participants[PlayerIndex].LastActionAmount = amountAsInt;
            G.Participants[PlayerIndex].RoundNumberOfLastAction = G._CardsDealtIncludingCurrent;

            // If we're playing a limit game, note the raise
            if ( G.IsLimitGame ) {
                G._BringInBetHasBeenMade = true; // always true after first raise
                if ( amountAsInt > G.LimitGameBringInAmount) {
                    // i.e. the player is either completing the first small or big bet or is actually raising
                    G._SmallBetHasBeenCompleted = true;
                    G._NumberOfRaisesInThisRound++;
                    if ( amountAsInt == G.LimitGameBigBet ) {
                        G._SmallRaiseStillAllowable = false;
                    }
                }
            }

            // Find and set next player (we would expect there to be someone else or we would not have been able to raise in the first place)
            await G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent); 
        }
    }     
}  
