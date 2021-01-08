using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionCover' Class  
    /// </summary>  
    public class ActionCover : Action
    {  
        public ActionCover(string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers) 
            : base(connectionId, actionType, ourGame, user, leavers)
        {
        }
        public override async Task ProcessAction()
        {
            // Handle the cover (like a call but where player doesn't have enough to cover the current raise)
            // (note that the base class has already checked the player's eligibility for this action)
            Participant p = G.Participants[PlayerIndex];

            if ( p.UncommittedChips == 0 ) {
                G.RecordLastEvent(UserName + " had no further chips and covered the pot");
            }
            else {
                G.RecordLastEvent(UserName + " paid " + p.UncommittedChips + " to cover the pot");
            }            

            // Implement the cover (has to be done pot-by-pot, and could involve splitting a pot)
            p.HasCovered = true;
            G.MoveAmountToPotForSpecifiedPlayer(PlayerIndex, p.UncommittedChips);

            G.Participants[PlayerIndex].LastActionInThisHand = this.ActionType;
            G.Participants[PlayerIndex].LastActionAmount = p.UncommittedChips;
            G.Participants[PlayerIndex].RoundNumberOfLastAction = G._CardsDealtIncludingCurrent;
            
            // Find and set next player (could be no one if all players have now called or covered)
            await G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);
        }
    }     
}  
